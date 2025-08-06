using DynamicPricingSalesSystem.Models;
using System;
using System.Linq;

namespace DynamicPricingSalesSystem.Core.Pricing
{
    public class CompetitorBasedPricing : IPricingStrategy
    {
        public string Name => "Competitor-Based Pricing";
        private string _pricingReason = string.Empty;

        public decimal CalculatePrice(Product product, PricingContext context)
        {
            if (!context.CompetitorProducts.Any())
            {
                _pricingReason = "No competitor data available, using base price";
                return product.BasePrice;
            }

            var avgCompetitorPrice = context.CompetitorProducts.Average(p => p.CurrentPrice);
            var adjustmentFactor = 0.95m; // Price 5% below average competitor

            var newPrice = avgCompetitorPrice * adjustmentFactor;
            
            // Ensure we don't go below cost
            if (newPrice < product.Cost * 1.1m) // Minimum 10% margin
            {
                newPrice = product.Cost * 1.1m;
                _pricingReason = "Price adjusted to maintain minimum margin";
            }
            else
            {
                _pricingReason = $"Priced 5% below average competitor price ({avgCompetitorPrice:C})";
            }

            return Math.Round(newPrice, 2);
        }

        public string GetPricingReason() => _pricingReason;
    }

    public class DemandBasedPricing : IPricingStrategy
    {
        public string Name => "Demand-Based Pricing";
        private string _pricingReason = string.Empty;

        public decimal CalculatePrice(Product product, PricingContext context)
        {
            var basePrice = product.BasePrice;
            var demandMultiplier = context.DemandFactor;
            
            // High demand = higher price, low demand = lower price
            var newPrice = basePrice * demandMultiplier;
            
            // Apply inventory level adjustments
            if (context.InventoryLevel < 0.2m) // Low inventory
            {
                newPrice *= 1.1m; // Increase price by 10%
                _pricingReason = $"High demand ({demandMultiplier:F2}x) with low inventory - premium pricing";
            }
            else if (context.InventoryLevel > 0.8m) // High inventory
            {
                newPrice *= 0.9m; // Decrease price by 10%
                _pricingReason = $"Lower demand ({demandMultiplier:F2}x) with high inventory - clearance pricing";
            }
            else
            {
                _pricingReason = $"Demand-adjusted pricing (factor: {demandMultiplier:F2}x)";
            }

            // Ensure minimum margin
            if (newPrice < product.Cost * 1.05m)
            {
                newPrice = product.Cost * 1.05m;
                _pricingReason += " - adjusted to maintain minimum margin";
            }

            return Math.Round(newPrice, 2);
        }

        public string GetPricingReason() => _pricingReason;
    }

    public class CostPlusPricing : IPricingStrategy
    {
        public string Name => "Cost-Plus Pricing";
        private string _pricingReason = string.Empty;
        private readonly decimal _markupPercentage;

        public CostPlusPricing(decimal markupPercentage = 50m)
        {
            _markupPercentage = markupPercentage;
        }

        public decimal CalculatePrice(Product product, PricingContext context)
        {
            var markup = _markupPercentage / 100m;
            var newPrice = product.Cost * (1 + markup);
            
            _pricingReason = $"Cost ({product.Cost:C}) + {_markupPercentage}% markup";
            
            return Math.Round(newPrice, 2);
        }

        public string GetPricingReason() => _pricingReason;
    }

    public class ValueBasedPricing : IPricingStrategy
    {
        public string Name => "Value-Based Pricing";
        private string _pricingReason = string.Empty;

        public decimal CalculatePrice(Product product, PricingContext context)
        {
            var basePrice = product.BasePrice;
            var customerSegmentMultiplier = GetCustomerSegmentMultiplier(context.Customer?.Segment);
            var brandPremium = GetBrandPremium(product.Brand);
            var ratingBonus = GetRatingBonus(product.Metrics.AverageRating);
            
            var newPrice = basePrice * customerSegmentMultiplier * brandPremium * ratingBonus;
            
            // Apply loyalty discount if applicable
            if (context.CustomerLoyaltyDiscount > 0)
            {
                newPrice *= (1 - context.CustomerLoyaltyDiscount);
                _pricingReason = $"Value-based pricing with {context.CustomerLoyaltyDiscount:P0} loyalty discount";
            }
            else
            {
                _pricingReason = $"Value-based pricing (segment: {customerSegmentMultiplier:F2}x, brand: {brandPremium:F2}x, rating: {ratingBonus:F2}x)";
            }

            return Math.Round(newPrice, 2);
        }

        private decimal GetCustomerSegmentMultiplier(CustomerSegment? segment)
        {
            return segment switch
            {
                CustomerSegment.VIP => 1.2m,
                CustomerSegment.Premium => 1.15m,
                CustomerSegment.Regular => 1.0m,
                CustomerSegment.New => 0.95m,
                CustomerSegment.Churned => 0.9m,
                _ => 1.0m
            };
        }

        private decimal GetBrandPremium(string brand)
        {
            return brand.ToLower() switch
            {
                "apple" or "samsung" or "nike" or "sony" => 1.1m,
                "premium" or "luxury" => 1.15m,
                _ => 1.0m
            };
        }

        private decimal GetRatingBonus(decimal rating)
        {
            if (rating >= 4.5m) return 1.05m;
            if (rating >= 4.0m) return 1.02m;
            if (rating >= 3.5m) return 1.0m;
            return 0.95m;
        }

        public string GetPricingReason() => _pricingReason;
    }
}