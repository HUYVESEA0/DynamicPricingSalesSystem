using System;

namespace DynamicPricingSalesSystem.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal PriceChange => NewPrice - OldPrice;
        public decimal PercentageChange => OldPrice != 0 ? Math.Round(((NewPrice - OldPrice) / OldPrice) * 100, 2) : 0;
    }
}