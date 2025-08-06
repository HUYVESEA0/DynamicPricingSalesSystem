using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Utils
{
    public static class PricingCalculator
    {
        public static decimal CalculateSupplyDemandPrice(Product product, List<Order> recentOrders, int daysPeriod = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysPeriod);
            var productOrders = recentOrders
                .Where(o => o.OrderDate >= cutoffDate && 
                           o.Items.Any(i => i.ProductId == product.Id))
                .SelectMany(o => o.Items.Where(i => i.ProductId == product.Id))
                .ToList();

            // Calculate demand score based on recent sales
            var totalSold = productOrders.Sum(i => i.Quantity);
            var demandFactor = Math.Max(0.5, Math.Min(2.0, totalSold / 10.0)); // Normalize to 0.5-2.0

            // Calculate supply pressure based on stock levels
            var stockPressure = product.CalculateStockRatio();

            // Combine factors
            var combinedFactor = (demandFactor + stockPressure) / 2;
            var adjustedPrice = product.BasePrice * (decimal)combinedFactor;

            return Math.Max(product.MinPrice, Math.Min(product.MaxPrice, adjustedPrice));
        }

        public static decimal CalculateCompetitorBasedPrice(Product product, List<CompetitorPrice> competitorPrices)
        {
            var relevantPrices = competitorPrices
                .Where(cp => cp.ProductName.Contains(product.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase) ||
                            cp.Category.Equals(product.Category, StringComparison.OrdinalIgnoreCase))
                .Where(cp => cp.IsAvailable)
                .ToList();

            if (!relevantPrices.Any())
                return product.CurrentPrice;

            var avgCompetitorPrice = relevantPrices.Average(cp => cp.Price);
            var minCompetitorPrice = relevantPrices.Min(cp => cp.Price);
            var maxCompetitorPrice = relevantPrices.Max(cp => cp.Price);

            // Position slightly below average but above minimum
            var targetPrice = avgCompetitorPrice * 0.95m;
            
            return Math.Max(product.MinPrice, Math.Min(product.MaxPrice, targetPrice));
        }

        public static decimal CalculateTimeBasedPrice(Product product, DateTime targetTime)
        {
            var baseMultiplier = 1.0m;
            
            // Weekend premium
            if (targetTime.DayOfWeek == DayOfWeek.Saturday || targetTime.DayOfWeek == DayOfWeek.Sunday)
            {
                baseMultiplier *= 1.1m;
            }

            // Hour-based adjustments (higher prices during peak hours)
            var hour = targetTime.Hour;
            if (hour >= 18 && hour <= 21) // Evening peak
            {
                baseMultiplier *= 1.05m;
            }
            else if (hour >= 2 && hour <= 6) // Early morning discount
            {
                baseMultiplier *= 0.95m;
            }

            // Holiday premium (simplified - just check if it's a common holiday)
            if (IsHoliday(targetTime))
            {
                baseMultiplier *= 1.15m;
            }

            var adjustedPrice = product.BasePrice * baseMultiplier;
            return Math.Max(product.MinPrice, Math.Min(product.MaxPrice, adjustedPrice));
        }

        public static decimal CalculateCustomerSegmentPrice(Product product, CustomerSegment segment)
        {
            var discountRate = segment switch
            {
                CustomerSegment.VIP => 0.10m,
                CustomerSegment.Regular => 0.05m,
                CustomerSegment.AtRisk => 0.15m,
                CustomerSegment.New => 0.02m,
                _ => 0m
            };

            var discountedPrice = product.CurrentPrice * (1 - discountRate);
            return Math.Max(product.MinPrice, discountedPrice);
        }

        public static decimal CalculateSeasonalPrice(Product product, DateTime targetDate)
        {
            var seasonMultiplier = GetSeasonalMultiplier(product.Category, targetDate);
            var adjustedPrice = product.BasePrice * (decimal)seasonMultiplier * (decimal)product.SeasonalityFactor;
            
            return Math.Max(product.MinPrice, Math.Min(product.MaxPrice, adjustedPrice));
        }

        public static double CalculatePriceElasticity(Product product, List<Order> historicalOrders, int daysPeriod = 90)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysPeriod);
            var productSales = historicalOrders
                .Where(o => o.OrderDate >= cutoffDate)
                .SelectMany(o => o.Items.Where(i => i.ProductId == product.Id))
                .GroupBy(i => i.UnitPrice)
                .Select(g => new { Price = g.Key, Quantity = g.Sum(i => i.Quantity) })
                .OrderBy(x => x.Price)
                .ToList();

            if (productSales.Count < 2)
                return -1.0; // Default elasticity

            // Simple elasticity calculation using first and last price points
            var firstPoint = productSales.First();
            var lastPoint = productSales.Last();

            var priceChange = (double)(lastPoint.Price - firstPoint.Price) / (double)firstPoint.Price;
            var quantityChange = (double)(lastPoint.Quantity - firstPoint.Quantity) / (double)firstPoint.Quantity;

            if (Math.Abs(priceChange) < 0.001)
                return -1.0;

            return quantityChange / priceChange;
        }

        public static decimal CalculateOptimalPrice(Product product, List<Order> orders, List<CompetitorPrice> competitorPrices)
        {
            var prices = new List<decimal>
            {
                CalculateSupplyDemandPrice(product, orders),
                CalculateCompetitorBasedPrice(product, competitorPrices),
                CalculateTimeBasedPrice(product, DateTime.Now),
                CalculateSeasonalPrice(product, DateTime.Now)
            };

            // Remove extreme outliers and calculate weighted average
            var validPrices = prices.Where(p => p >= product.MinPrice && p <= product.MaxPrice).ToList();
            
            if (!validPrices.Any())
                return product.CurrentPrice;

            // Weighted average (can be enhanced with ML)
            var optimalPrice = validPrices.Average();
            
            return Math.Round(Math.Max(product.MinPrice, Math.Min(product.MaxPrice, optimalPrice)), 2);
        }

        public static double CalculateDemandForecast(Product product, List<Order> historicalOrders, int forecastDays = 30)
        {
            var dailySales = historicalOrders
                .Where(o => o.OrderDate >= DateTime.Now.AddDays(-90))
                .SelectMany(o => o.Items.Where(i => i.ProductId == product.Id))
                .GroupBy(i => i.OrderId) // Group by order to get daily data
                .Join(historicalOrders, g => g.Key, o => o.Id, (g, o) => new { o.OrderDate, Quantity = g.Sum(i => i.Quantity) })
                .GroupBy(x => x.OrderDate.Date)
                .Select(g => new { Date = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.Date)
                .ToList();

            if (dailySales.Count < 7)
                return product.SalesCount * 0.1; // Default forecast

            // Simple moving average forecast
            var recentAverage = dailySales.TakeLast(14).Average(x => x.TotalQuantity);
            var trend = CalculateTrend(dailySales.Select(x => (double)x.TotalQuantity).ToList());
            
            return Math.Max(0, recentAverage * forecastDays * (1 + trend));
        }

        private static double CalculateTrend(List<double> values)
        {
            if (values.Count < 2)
                return 0;

            var n = values.Count;
            var sumX = (n * (n - 1)) / 2; // Sum of indices
            var sumY = values.Sum();
            var sumXY = values.Select((y, x) => x * y).Sum();
            var sumX2 = values.Select((y, x) => x * x).Sum();

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            return slope / (sumY / n); // Normalize by average
        }

        private static bool IsHoliday(DateTime date)
        {
            // Simplified holiday detection
            return date.Month == 12 && (date.Day == 25 || date.Day == 31) || // Christmas, New Year's Eve
                   date.Month == 1 && date.Day == 1 || // New Year's Day
                   date.Month == 7 && date.Day == 4 || // Independence Day
                   date.Month == 11 && date.Day >= 22 && date.Day <= 28 && date.DayOfWeek == DayOfWeek.Thursday; // Thanksgiving
        }

        private static double GetSeasonalMultiplier(string category, DateTime date)
        {
            return category.ToLower() switch
            {
                "clothing" => GetClothingSeasonality(date),
                "toys" => GetToysSeasonality(date),
                "sports" => GetSportsSeasonality(date),
                "home & garden" => GetHomeGardenSeasonality(date),
                _ => 1.0
            };
        }

        private static double GetClothingSeasonality(DateTime date)
        {
            return date.Month switch
            {
                3 or 4 or 5 => 1.1, // Spring
                6 or 7 or 8 => 0.9, // Summer
                9 or 10 or 11 => 1.2, // Fall
                12 or 1 or 2 => 1.3, // Winter
                _ => 1.0
            };
        }

        private static double GetToysSeasonality(DateTime date)
        {
            return date.Month switch
            {
                11 or 12 => 1.5, // Holiday season
                6 or 7 or 8 => 1.2, // Summer vacation
                _ => 0.8
            };
        }

        private static double GetSportsSeasonality(DateTime date)
        {
            return date.Month switch
            {
                3 or 4 or 5 => 1.3, // Spring sports
                6 or 7 or 8 => 1.4, // Summer sports
                9 or 10 or 11 => 1.2, // Fall sports
                _ => 0.9
            };
        }

        private static double GetHomeGardenSeasonality(DateTime date)
        {
            return date.Month switch
            {
                3 or 4 or 5 => 1.4, // Spring gardening
                6 or 7 or 8 => 1.2, // Summer maintenance
                _ => 0.8
            };
        }
    }
}