using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Services
{
    public class AnalyticsService
    {
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;
        private readonly OrderService _orderService;
        private readonly JsonDataManager _dataManager;

        public AnalyticsService(ProductService productService, CustomerService customerService, OrderService orderService, JsonDataManager dataManager)
        {
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _dataManager = dataManager;
        }

        public DashboardMetrics GetDashboardMetrics(int daysPeriod = 30)
        {
            var startDate = DateTime.Now.AddDays(-daysPeriod);
            var endDate = DateTime.Now;

            var recentOrders = _orderService.GetOrdersByDateRange(startDate, endDate)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToList();

            var totalRevenue = recentOrders.Sum(o => o.FinalAmount);
            var totalOrders = recentOrders.Count;
            var totalCustomers = _customerService.GetAllCustomers().Count(c => c.IsActive);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var lowStockProducts = _productService.GetLowStockProducts().Count;
            var activePricingRules = _dataManager.LoadData<PricingRule>("pricing-rules").Count(r => r.IsActive);

            // Calculate growth (simplified)
            var previousPeriodRevenue = _orderService.GetTotalRevenue(startDate.AddDays(-daysPeriod), startDate);
            var revenueGrowth = previousPeriodRevenue > 0 ? 
                ((totalRevenue - previousPeriodRevenue) / previousPeriodRevenue) * 100 : 0;

            return new DashboardMetrics
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                AverageOrderValue = averageOrderValue,
                ConversionRate = CalculateConversionRate(),
                LowStockProducts = lowStockProducts,
                ActivePricingRules = activePricingRules,
                LastUpdated = DateTime.Now,
                RevenueGrowth = revenueGrowth,
                CustomerSatisfaction = CalculateCustomerSatisfaction()
            };
        }

        public List<SalesMetrics> GetProductSalesMetrics(int daysPeriod = 30)
        {
            var startDate = DateTime.Now.AddDays(-daysPeriod);
            var products = _productService.GetAllProducts();
            var orders = _orderService.GetOrdersByDateRange(startDate, DateTime.Now);
            
            var metrics = new List<SalesMetrics>();

            foreach (var product in products)
            {
                var productSales = orders
                    .SelectMany(o => o.Items.Where(i => i.ProductId == product.Id))
                    .ToList();

                var unitsSold = productSales.Sum(i => i.Quantity);
                var revenue = productSales.Sum(i => i.TotalPrice);
                var averagePrice = unitsSold > 0 ? revenue / unitsSold : 0;

                metrics.Add(new SalesMetrics
                {
                    ProductId = product.Id,
                    Date = DateTime.Now.Date,
                    UnitsSold = unitsSold,
                    Revenue = revenue,
                    AveragePrice = averagePrice,
                    ViewCount = 0, // Would be tracked in real system
                    ConversionRate = CalculateProductConversionRate(product.Id),
                    StockLevel = product.Stock,
                    DemandScore = product.DemandScore,
                    Category = product.Category
                });
            }

            return metrics.OrderByDescending(m => m.Revenue).ToList();
        }

        public Dictionary<string, decimal> GetCategoryPerformance(int daysPeriod = 30)
        {
            var startDate = DateTime.Now.AddDays(-daysPeriod);
            return _orderService.GetRevenueByCategory(startDate, DateTime.Now);
        }

        public List<(string Period, decimal Revenue, int Orders)> GetRevenuetrends(int periods = 12)
        {
            var trends = new List<(string, decimal, int)>();
            
            for (int i = periods - 1; i >= 0; i--)
            {
                var endDate = DateTime.Now.AddMonths(-i);
                var startDate = endDate.AddMonths(-1);
                
                var periodOrders = _orderService.GetOrdersByDateRange(startDate, endDate)
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .ToList();
                
                var revenue = periodOrders.Sum(o => o.FinalAmount);
                var orderCount = periodOrders.Count;
                
                trends.Add((endDate.ToString("MMM yyyy"), revenue, orderCount));
            }

            return trends;
        }

        public double CalculatePriceElasticity(int productId, int daysPeriod = 90)
        {
            var product = _productService.GetProductById(productId);
            if (product == null) return -1.0;

            var startDate = DateTime.Now.AddDays(-daysPeriod);
            var historicalOrders = _orderService.GetOrdersByDateRange(startDate, DateTime.Now);
            
            return Utils.PricingCalculator.CalculatePriceElasticity(product, historicalOrders, daysPeriod);
        }

        public List<(int CustomerId, string Name, decimal PredictedValue, double ChurnProbability)> 
            GetCustomerInsights(int topCount = 20)
        {
            var customers = _customerService.GetAllCustomers().Where(c => c.IsActive).ToList();
            var insights = new List<(int, string, decimal, double)>();

            foreach (var customer in customers)
            {
                var clv = (decimal)customer.CalculateCustomerLifetimeValue();
                var churnProbability = CalculateChurnProbability(customer);
                
                insights.Add((customer.Id, customer.FullName, clv, churnProbability));
            }

            return insights.OrderByDescending(i => i.Item3).Take(topCount).ToList();
        }

        public Dictionary<CustomerSegment, (int Count, decimal AvgValue, decimal TotalRevenue)> 
            GetSegmentAnalysis()
        {
            var customers = _customerService.GetAllCustomers().Where(c => c.IsActive);
            var result = new Dictionary<CustomerSegment, (int, decimal, decimal)>();

            foreach (var segment in Enum.GetValues<CustomerSegment>())
            {
                var segmentCustomers = customers.Where(c => c.Segment == segment).ToList();
                var count = segmentCustomers.Count;
                var avgValue = count > 0 ? segmentCustomers.Average(c => c.TotalSpent) : 0;
                var totalRevenue = segmentCustomers.Sum(c => c.TotalSpent);

                result[segment] = (count, avgValue, totalRevenue);
            }

            return result;
        }

        private double CalculateConversionRate()
        {
            // Simplified conversion rate calculation
            var allCustomers = _customerService.GetAllCustomers().Count;
            var customersWithOrders = _customerService.GetAllCustomers().Count(c => c.TotalOrders > 0);
            
            return allCustomers > 0 ? (double)customersWithOrders / allCustomers * 100 : 0;
        }

        private double CalculateProductConversionRate(int productId)
        {
            // Simplified product conversion rate
            // In real system, would track views vs purchases
            var product = _productService.GetProductById(productId);
            return product?.DemandScore * 10 ?? 0; // Convert demand score to percentage
        }

        private double CalculateCustomerSatisfaction()
        {
            // Simplified satisfaction calculation based on repeat customers
            var customers = _customerService.GetAllCustomers().Where(c => c.IsActive);
            var totalCustomers = customers.Count();
            var repeatCustomers = customers.Count(c => c.TotalOrders > 1);
            
            return totalCustomers > 0 ? (double)repeatCustomers / totalCustomers * 100 : 0;
        }

        private double CalculateChurnProbability(Customer customer)
        {
            var daysSinceLastPurchase = customer.DaysSinceLastPurchase;
            var avgDaysBetweenOrders = customer.TotalOrders > 1 ? 
                (DateTime.Now - customer.RegistrationDate).Days / customer.TotalOrders : 30;

            // Simple churn probability based on purchase recency
            if (daysSinceLastPurchase > avgDaysBetweenOrders * 3)
                return 0.8; // High churn risk
            else if (daysSinceLastPurchase > avgDaysBetweenOrders * 2)
                return 0.5; // Medium churn risk
            else
                return 0.2; // Low churn risk
        }
    }
}