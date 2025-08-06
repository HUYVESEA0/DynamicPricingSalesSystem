using System;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
        public int SalesCount { get; set; }
        public double DemandScore { get; set; } = 1.0;
        public string Supplier { get; set; } = string.Empty;
        public double SeasonalityFactor { get; set; } = 1.0;

        public decimal CalculateProfitMargin()
        {
            if (Cost == 0) return 0;
            return Math.Round(((CurrentPrice - Cost) / Cost) * 100, 2);
        }

        public void UpdatePrice(decimal newPrice, string reason = "")
        {
            if (newPrice < MinPrice || newPrice > MaxPrice)
                return;

            var history = new PriceHistory
            {
                ProductId = Id,
                OldPrice = CurrentPrice,
                NewPrice = newPrice,
                ChangeDate = DateTime.Now,
                Reason = reason
            };

            PriceHistory.Add(history);
            CurrentPrice = newPrice;
            LastUpdated = DateTime.Now;
        }

        public List<PriceHistory> GetPriceHistory(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return PriceHistory.FindAll(h => h.ChangeDate >= cutoffDate);
        }

        public bool IsLowStock(int threshold = 10)
        {
            return Stock <= threshold;
        }

        public double CalculateStockRatio()
        {
            // Simple stock pressure calculation (lower stock = higher pressure)
            return Math.Max(0.1, Math.Min(2.0, 50.0 / Math.Max(1, Stock)));
        }
    }
}