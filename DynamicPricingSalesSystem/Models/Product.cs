using System;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal Weight { get; set; }
        public string Dimensions { get; set; } = string.Empty;
        public List<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
        public ProductMetrics Metrics { get; set; } = new ProductMetrics();
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        
        public decimal ProfitMargin => CurrentPrice > 0 ? ((CurrentPrice - Cost) / CurrentPrice) * 100 : 0;
        public bool NeedsReorder => Stock <= ReorderLevel;
        public decimal InventoryValue => Stock * Cost;
    }

    public class PriceHistory
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    public class ProductMetrics
    {
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ViewCount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime LastSold { get; set; }
        public decimal DemandScore { get; set; }
        public decimal CompetitorPrice { get; set; }
        public decimal PriceElasticity { get; set; }
    }
}