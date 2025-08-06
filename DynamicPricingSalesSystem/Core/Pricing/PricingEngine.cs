using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicPricingSalesSystem.Core.Pricing
{
    public class PricingEngine
    {
        private readonly List<IPricingStrategy> _strategies;
        private readonly Dictionary<string, decimal> _strategyWeights;

        public PricingEngine()
        {
            _strategies = new List<IPricingStrategy>
            {
                new CompetitorBasedPricing(),
                new DemandBasedPricing(),
                new CostPlusPricing(),
                new ValueBasedPricing()
            };

            _strategyWeights = new Dictionary<string, decimal>
            {
                { "Competitor-Based Pricing", 0.3m },
                { "Demand-Based Pricing", 0.3m },
                { "Cost-Plus Pricing", 0.2m },
                { "Value-Based Pricing", 0.2m }
            };
        }

        public PricingResult CalculateOptimalPrice(Product product, PricingContext context)
        {
            var strategyResults = new List<StrategyResult>();
            decimal weightedSum = 0;
            decimal totalWeight = 0;

            foreach (var strategy in _strategies)
            {
                var price = strategy.CalculatePrice(product, context);
                var weight = _strategyWeights.GetValueOrDefault(strategy.Name, 0.25m);
                
                strategyResults.Add(new StrategyResult
                {
                    StrategyName = strategy.Name,
                    Price = price,
                    Weight = weight,
                    Reason = strategy.GetPricingReason()
                });

                weightedSum += price * weight;
                totalWeight += weight;
            }

            var optimalPrice = totalWeight > 0 ? weightedSum / totalWeight : product.BasePrice;
            
            // Apply seasonal adjustments
            if (context.IsSeasonalPeriod)
            {
                optimalPrice = ApplySeasonalAdjustment(optimalPrice, context.Season);
            }

            return new PricingResult
            {
                OptimalPrice = Math.Round(optimalPrice, 2),
                StrategyResults = strategyResults,
                PriceChange = optimalPrice - product.CurrentPrice,
                PriceChangePercent = product.CurrentPrice > 0 ? 
                    ((optimalPrice - product.CurrentPrice) / product.CurrentPrice) * 100 : 0,
                Timestamp = DateTime.Now,
                Context = context
            };
        }

        private decimal ApplySeasonalAdjustment(decimal price, string season)
        {
            var seasonalMultiplier = season.ToLower() switch
            {
                "holiday" => 1.1m,
                "christmas" => 1.15m,
                "black_friday" => 0.8m,
                "summer_sale" => 0.9m,
                "back_to_school" => 1.05m,
                _ => 1.0m
            };

            return price * seasonalMultiplier;
        }

        public void UpdateStrategyWeights(Dictionary<string, decimal> newWeights)
        {
            foreach (var weight in newWeights)
            {
                if (_strategyWeights.ContainsKey(weight.Key))
                {
                    _strategyWeights[weight.Key] = weight.Value;
                }
            }
        }

        public List<string> GetAvailableStrategies()
        {
            return _strategies.Select(s => s.Name).ToList();
        }

        public ABTestResult RunABTest(Product product, PricingContext context, 
            decimal priceA, decimal priceB, int daysToRun = 7)
        {
            // Simulate A/B test results
            var random = new Random();
            
            var testA = new ABTestVariant
            {
                Price = priceA,
                Conversions = random.Next(50, 200),
                Views = random.Next(1000, 3000),
                Revenue = 0
            };
            testA.ConversionRate = (decimal)testA.Conversions / testA.Views * 100;
            testA.Revenue = testA.Conversions * priceA;

            var testB = new ABTestVariant
            {
                Price = priceB,
                Conversions = random.Next(50, 200),
                Views = random.Next(1000, 3000),
                Revenue = 0
            };
            testB.ConversionRate = (decimal)testB.Conversions / testB.Views * 100;
            testB.Revenue = testB.Conversions * priceB;

            var winner = testA.Revenue > testB.Revenue ? "A" : "B";
            var confidence = Math.Abs(testA.ConversionRate - testB.ConversionRate) * 10;

            return new ABTestResult
            {
                ProductId = product.Id,
                VariantA = testA,
                VariantB = testB,
                Winner = winner,
                Confidence = Math.Min(confidence, 95),
                StartDate = DateTime.Now.AddDays(-daysToRun),
                EndDate = DateTime.Now,
                Recommendations = GenerateABTestRecommendations(testA, testB, winner)
            };
        }

        private List<string> GenerateABTestRecommendations(ABTestVariant a, ABTestVariant b, string winner)
        {
            var recommendations = new List<string>();
            
            if (winner == "A")
            {
                recommendations.Add($"Implement Price A (${a.Price:F2}) for higher revenue");
                recommendations.Add($"Revenue increase of ${a.Revenue - b.Revenue:F2} expected");
            }
            else
            {
                recommendations.Add($"Implement Price B (${b.Price:F2}) for higher revenue");
                recommendations.Add($"Revenue increase of ${b.Revenue - a.Revenue:F2} expected");
            }

            if (Math.Abs(a.ConversionRate - b.ConversionRate) < 1)
            {
                recommendations.Add("Conversion rates are similar - consider other factors like inventory");
            }

            return recommendations;
        }
    }

    public class PricingResult
    {
        public decimal OptimalPrice { get; set; }
        public List<StrategyResult> StrategyResults { get; set; } = new List<StrategyResult>();
        public decimal PriceChange { get; set; }
        public decimal PriceChangePercent { get; set; }
        public DateTime Timestamp { get; set; }
        public PricingContext Context { get; set; } = new PricingContext();
    }

    public class StrategyResult
    {
        public string StrategyName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ABTestResult
    {
        public int ProductId { get; set; }
        public ABTestVariant VariantA { get; set; } = new ABTestVariant();
        public ABTestVariant VariantB { get; set; } = new ABTestVariant();
        public string Winner { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class ABTestVariant
    {
        public decimal Price { get; set; }
        public int Views { get; set; }
        public int Conversions { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal Revenue { get; set; }
    }
}