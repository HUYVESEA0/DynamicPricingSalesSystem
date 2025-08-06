using DynamicPricingSalesSystem.Models;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Core.Pricing
{
    public interface IPricingStrategy
    {
        string Name { get; }
        decimal CalculatePrice(Product product, PricingContext context);
        string GetPricingReason();
    }

    public class PricingContext
    {
        public Customer? Customer { get; set; }
        public int Quantity { get; set; } = 1;
        public List<Product> CompetitorProducts { get; set; } = new List<Product>();
        public Dictionary<string, object> MarketData { get; set; } = new Dictionary<string, object>();
        public bool IsSeasonalPeriod { get; set; }
        public string Season { get; set; } = string.Empty;
        public decimal DemandFactor { get; set; } = 1.0m;
        public decimal InventoryLevel { get; set; }
        public bool IsPromotionalPeriod { get; set; }
        public decimal CustomerLoyaltyDiscount { get; set; }
    }
}