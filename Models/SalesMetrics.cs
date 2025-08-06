using System;

namespace DynamicPricingSalesSystem.Models
{
    public class SalesMetrics
    {
        public int ProductId { get; set; }
        public DateTime Date { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
        public int ViewCount { get; set; }
        public double ConversionRate { get; set; }
        public int StockLevel { get; set; }
        public double DemandScore { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class DashboardMetrics
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double ConversionRate { get; set; }
        public int LowStockProducts { get; set; }
        public int ActivePricingRules { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal RevenueGrowth { get; set; }
        public double CustomerSatisfaction { get; set; }
    }
}