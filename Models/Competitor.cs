using System;

namespace DynamicPricingSalesSystem.Models
{
    public class Competitor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public double MarketShare { get; set; }
        public string PricingStrategy { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class CompetitorPrice
    {
        public int Id { get; set; }
        public int CompetitorId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime DateRecorded { get; set; }
        public string Source { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}