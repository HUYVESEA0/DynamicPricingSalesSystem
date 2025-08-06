using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Models;
using DynamicPricingSalesSystem.Services;
using DynamicPricingSalesSystem.Utils;

namespace DynamicPricingSalesSystem.Engines
{
    public class DynamicPricingEngine
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private readonly List<PricingRule> _pricingRules;
        private readonly List<CompetitorPrice> _competitorPrices;

        public DynamicPricingEngine(ProductService productService, OrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
            _pricingRules = new List<PricingRule>();
            _competitorPrices = new List<CompetitorPrice>();
        }

        public void LoadPricingRules(List<PricingRule> rules)
        {
            _pricingRules.Clear();
            _pricingRules.AddRange(rules.Where(r => r.IsActive).OrderBy(r => r.Priority));
        }

        public void LoadCompetitorPrices(List<CompetitorPrice> prices)
        {
            _competitorPrices.Clear();
            _competitorPrices.AddRange(prices.Where(p => p.IsAvailable));
        }

        public decimal CalculateOptimalPrice(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return 0;

            var recentOrders = _orderService.GetRecentOrders();
            return CalculateOptimalPrice(product, recentOrders);
        }

        public decimal CalculateOptimalPrice(Product product, List<Order> recentOrders)
        {
            var pricingFactors = new Dictionary<string, decimal>();

            // 1. Supply-Demand Pricing
            var supplyDemandPrice = PricingCalculator.CalculateSupplyDemandPrice(product, recentOrders);
            pricingFactors["SupplyDemand"] = supplyDemandPrice;

            // 2. Competitor-Based Pricing
            var competitorPrice = PricingCalculator.CalculateCompetitorBasedPrice(product, _competitorPrices);
            pricingFactors["Competitor"] = competitorPrice;

            // 3. Time-Based Pricing
            var timeBasedPrice = PricingCalculator.CalculateTimeBasedPrice(product, DateTime.Now);
            pricingFactors["TimeBased"] = timeBasedPrice;

            // 4. Seasonal Pricing
            var seasonalPrice = PricingCalculator.CalculateSeasonalPrice(product, DateTime.Now);
            pricingFactors["Seasonal"] = seasonalPrice;

            // 5. Rule-Based Adjustments
            var ruleAdjustedPrice = ApplyPricingRules(product, pricingFactors);

            // Calculate weighted optimal price
            var optimalPrice = CalculateWeightedPrice(pricingFactors, ruleAdjustedPrice);

            // Ensure price is within bounds
            return Math.Max(product.MinPrice, Math.Min(product.MaxPrice, Math.Round(optimalPrice, 2)));
        }

        private decimal ApplyPricingRules(Product product, Dictionary<string, decimal> pricingFactors)
        {
            var adjustedPrice = pricingFactors.Values.Average();

            foreach (var rule in _pricingRules.Where(r => r.IsApplicableToCategory(product.Category)))
            {
                adjustedPrice = ApplyRule(rule, product, adjustedPrice);
            }

            return adjustedPrice;
        }

        private decimal ApplyRule(PricingRule rule, Product product, decimal currentPrice)
        {
            switch (rule.RuleType)
            {
                case PricingRuleType.InventoryBased:
                    return ApplyInventoryRule(rule, product, currentPrice);
                
                case PricingRuleType.DemandBased:
                    return ApplyDemandRule(rule, product, currentPrice);
                
                case PricingRuleType.CompetitorBased:
                    return ApplyCompetitorRule(rule, product, currentPrice);
                
                case PricingRuleType.TimeBased:
                    return ApplyTimeRule(rule, product, currentPrice);
                
                case PricingRuleType.CustomerSegmentBased:
                    return currentPrice; // Applied at order level
                
                case PricingRuleType.SeasonalBased:
                    return ApplySeasonalRule(rule, product, currentPrice);
                
                default:
                    return currentPrice;
            }
        }

        private decimal ApplyInventoryRule(PricingRule rule, Product product, decimal currentPrice)
        {
            if (product.Stock < 10) // Low stock threshold
            {
                var multiplier = Math.Max(rule.MinPriceMultiplier, 
                    Math.Min(rule.MaxPriceMultiplier, 1.0m + (10 - product.Stock) * 0.02m));
                return currentPrice * multiplier;
            }
            return currentPrice;
        }

        private decimal ApplyDemandRule(PricingRule rule, Product product, decimal currentPrice)
        {
            if (product.DemandScore > 1.5)
            {
                var multiplier = Math.Max(rule.MinPriceMultiplier,
                    Math.Min(rule.MaxPriceMultiplier, (decimal)product.DemandScore * 0.8m));
                return currentPrice * multiplier;
            }
            return currentPrice;
        }

        private decimal ApplyCompetitorRule(PricingRule rule, Product product, decimal currentPrice)
        {
            var relevantCompetitorPrices = _competitorPrices
                .Where(cp => cp.Category.Equals(product.Category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (relevantCompetitorPrices.Any())
            {
                var avgCompetitorPrice = relevantCompetitorPrices.Average(cp => cp.Price);
                if (currentPrice > avgCompetitorPrice * 1.1m) // If we're 10% above average
                {
                    return Math.Max(product.MinPrice, avgCompetitorPrice * 1.05m); // Price 5% above average
                }
            }
            return currentPrice;
        }

        private decimal ApplyTimeRule(PricingRule rule, Product product, decimal currentPrice)
        {
            var now = DateTime.Now;
            var multiplier = 1.0m;

            // Weekend premium
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                multiplier = 1.1m;
            }

            // Peak hours premium (6-9 PM)
            if (now.Hour >= 18 && now.Hour <= 21)
            {
                multiplier *= 1.05m;
            }

            multiplier = Math.Max(rule.MinPriceMultiplier, Math.Min(rule.MaxPriceMultiplier, multiplier));
            return currentPrice * multiplier;
        }

        private decimal ApplySeasonalRule(PricingRule rule, Product product, decimal currentPrice)
        {
            var seasonalMultiplier = (decimal)product.SeasonalityFactor;
            seasonalMultiplier = Math.Max(rule.MinPriceMultiplier, 
                Math.Min(rule.MaxPriceMultiplier, seasonalMultiplier));
            
            return currentPrice * seasonalMultiplier;
        }

        private decimal CalculateWeightedPrice(Dictionary<string, decimal> pricingFactors, decimal ruleAdjustedPrice)
        {
            // Define weights for different pricing strategies
            var weights = new Dictionary<string, double>
            {
                ["SupplyDemand"] = 0.3,
                ["Competitor"] = 0.25,
                ["TimeBased"] = 0.15,
                ["Seasonal"] = 0.15,
                ["Rules"] = 0.15
            };

            var weightedSum = 0m;
            var totalWeight = 0.0;

            foreach (var factor in pricingFactors)
            {
                if (weights.ContainsKey(factor.Key))
                {
                    var weight = (decimal)weights[factor.Key];
                    weightedSum += factor.Value * weight;
                    totalWeight += weights[factor.Key];
                }
            }

            // Add rule-adjusted price
            weightedSum += ruleAdjustedPrice * (decimal)weights["Rules"];
            totalWeight += weights["Rules"];

            return totalWeight > 0 ? weightedSum / (decimal)totalWeight : pricingFactors.Values.Average();
        }

        public List<(int ProductId, decimal CurrentPrice, decimal OptimalPrice, decimal PriceDifference)> 
            AnalyzeAllProducts()
        {
            var products = _productService.GetAllProducts();
            var recentOrders = _orderService.GetRecentOrders();
            var results = new List<(int, decimal, decimal, decimal)>();

            foreach (var product in products)
            {
                var optimalPrice = CalculateOptimalPrice(product, recentOrders);
                var priceDifference = optimalPrice - product.CurrentPrice;
                
                results.Add((product.Id, product.CurrentPrice, optimalPrice, priceDifference));
            }

            return results.OrderByDescending(r => Math.Abs(r.Item4)).ToList();
        }

        public void UpdateAllPrices(bool applyChanges = false)
        {
            var analysis = AnalyzeAllProducts();
            var priceUpdates = new List<(int ProductId, decimal NewPrice, string Reason)>();

            foreach (var (productId, currentPrice, optimalPrice, priceDifference) in analysis)
            {
                // Only update if difference is significant (more than 5% or $1)
                if (Math.Abs(priceDifference) > Math.Max(currentPrice * 0.05m, 1.0m))
                {
                    var reason = $"Dynamic pricing adjustment: {priceDifference:C} change";
                    priceUpdates.Add((productId, optimalPrice, reason));
                }
            }

            if (applyChanges && priceUpdates.Any())
            {
                _productService.BulkUpdatePrices(priceUpdates);
            }
        }

        public decimal CalculateCustomerSpecificPrice(int productId, int customerId)
        {
            var product = _productService.GetProductById(productId);
            var customer = new CustomerService(new Data.JsonDataManager()).GetCustomerById(customerId);
            
            if (product == null || customer == null)
                return 0;

            var basePrice = CalculateOptimalPrice(productId);
            return PricingCalculator.CalculateCustomerSegmentPrice(product, customer.Segment);
        }

        public PricingRecommendation GetPricingRecommendation(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new PricingRecommendation();

            var optimalPrice = CalculateOptimalPrice(productId);
            var currentPrice = product.CurrentPrice;
            var priceDifference = optimalPrice - currentPrice;
            var percentageChange = currentPrice != 0 ? (priceDifference / currentPrice) * 100 : 0;

            return new PricingRecommendation
            {
                ProductId = productId,
                ProductName = product.Name,
                CurrentPrice = currentPrice,
                RecommendedPrice = optimalPrice,
                PriceDifference = priceDifference,
                PercentageChange = percentageChange,
                Confidence = CalculateConfidence(product),
                Reasons = GenerateReasons(product, optimalPrice),
                ExpectedImpact = EstimateImpact(product, optimalPrice)
            };
        }

        private double CalculateConfidence(Product product)
        {
            var factors = 0;
            var confidence = 0.0;

            // Stock level confidence
            if (product.Stock > 0)
            {
                confidence += 0.2;
                factors++;
            }

            // Sales history confidence
            if (product.SalesCount > 5)
            {
                confidence += 0.3;
                factors++;
            }

            // Price history confidence
            if (product.PriceHistory.Any())
            {
                confidence += 0.2;
                factors++;
            }

            // Competitor data confidence
            if (_competitorPrices.Any(cp => cp.Category == product.Category))
            {
                confidence += 0.3;
                factors++;
            }

            return factors > 0 ? confidence / factors * 100 : 50.0;
        }

        private List<string> GenerateReasons(Product product, decimal optimalPrice)
        {
            var reasons = new List<string>();
            var currentPrice = product.CurrentPrice;

            if (optimalPrice > currentPrice)
            {
                if (product.Stock < 10)
                    reasons.Add("Low inventory levels justify price increase");
                if (product.DemandScore > 1.5)
                    reasons.Add("High demand detected");
            }
            else if (optimalPrice < currentPrice)
            {
                if (product.Stock > 50)
                    reasons.Add("High inventory suggests price reduction");
                if (product.DemandScore < 0.7)
                    reasons.Add("Low demand indicates price adjustment needed");
            }

            return reasons;
        }

        private string EstimateImpact(Product product, decimal newPrice)
        {
            var priceChange = ((newPrice - product.CurrentPrice) / product.CurrentPrice) * 100;
            var elasticity = -1.2; // Assumed price elasticity

            var demandChange = elasticity * (double)priceChange;
            var revenueChange = (double)priceChange + demandChange + ((double)priceChange * demandChange / 100);

            return $"Estimated {revenueChange:F1}% revenue change, {demandChange:F1}% demand change";
        }
    }

    public class PricingRecommendation
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal RecommendedPrice { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal PercentageChange { get; set; }
        public double Confidence { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public string ExpectedImpact { get; set; } = string.Empty;
    }
}