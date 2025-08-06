using System;

namespace DynamicPricingSalesSystem.Models
{
    public enum PricingRuleType
    {
        InventoryBased,
        DemandBased,
        CompetitorBased,
        TimeBased,
        CustomerSegmentBased,
        SeasonalBased
    }

    public class PricingRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PricingRuleType RuleType { get; set; }
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1;
        public string Conditions { get; set; } = string.Empty; // JSON conditions
        public decimal MinPriceMultiplier { get; set; } = 0.8m;
        public decimal MaxPriceMultiplier { get; set; } = 1.5m;
        public string ApplicableCategories { get; set; } = string.Empty; // Comma-separated
        public DateTime CreatedDate { get; set; }
        public DateTime? LastApplied { get; set; }
        public string Description { get; set; } = string.Empty;

        public bool IsApplicableToCategory(string category)
        {
            if (string.IsNullOrEmpty(ApplicableCategories))
                return true;

            return ApplicableCategories.Contains(category, StringComparison.OrdinalIgnoreCase);
        }

        public decimal CalculatePriceAdjustment(decimal basePrice, double factor)
        {
            var multiplier = Math.Max(MinPriceMultiplier, Math.Min(MaxPriceMultiplier, (decimal)factor));
            return basePrice * multiplier;
        }
    }
}