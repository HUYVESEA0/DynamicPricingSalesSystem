namespace DynamicPricingSalesSystem.Models
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountApplied { get; set; }

        public void CalculateTotal()
        {
            TotalPrice = (UnitPrice * Quantity) - DiscountApplied;
        }

        public decimal EffectiveUnitPrice => Quantity > 0 ? TotalPrice / Quantity : 0;
    }
}