using DynamicPricingSalesSystem.Data.Repositories;
using DynamicPricingSalesSystem.Models;
using DynamicPricingSalesSystem.Services.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Services.Analytics
{
    public class AnalyticsService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        public AnalyticsService(CustomerRepository customerRepository,
                              ProductRepository productRepository,
                              OrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            var now = DateTime.Now;
            var today = now.Date;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var orders = await _orderRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var customers = await _customerRepository.GetAllAsync();

            var todaysOrders = orders.Where(o => o.OrderDate.Date == today).ToList();
            var thisMonthsOrders = orders.Where(o => o.OrderDate >= thisMonth).ToList();
            var lastMonthsOrders = orders.Where(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth).ToList();

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var thisMonthCompleted = thisMonthsOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            var lastMonthCompleted = lastMonthsOrders.Where(o => o.Status == OrderStatus.Delivered).ToList();

            var metrics = new DashboardMetrics
            {
                TotalCustomers = customers.Count,
                TotalProducts = products.Count,
                TotalOrders = orders.Count,
                TodaysOrders = todaysOrders.Count,
                TodaysRevenue = todaysOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                ThisMonthRevenue = thisMonthCompleted.Sum(o => o.TotalAmount),
                LastMonthRevenue = lastMonthCompleted.Sum(o => o.TotalAmount),
                AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0,
                ConversionRate = CalculateConversionRate(customers, completedOrders),
                LowStockAlerts = products.Count(p => p.NeedsReorder),
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
                CustomerSegmentDistribution = await _customerRepository.GetSegmentDistributionAsync(),
                RevenueGrowth = CalculateGrowthRate(thisMonthCompleted.Sum(o => o.TotalAmount), 
                                                   lastMonthCompleted.Sum(o => o.TotalAmount)),
                TopSellingProducts = await GetTopSellingProducts(5),
                RecentActivity = await GetRecentActivity(10)
            };

            return metrics;
        }

        public async Task<CustomerAnalytics> GetCustomerAnalyticsAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            var now = DateTime.Now;

            var analytics = new CustomerAnalytics
            {
                TotalCustomers = customers.Count,
                NewCustomersThisMonth = customers.Count(c => c.RegistrationDate >= new DateTime(now.Year, now.Month, 1)),
                ActiveCustomers = customers.Count(c => c.DaysSinceLastPurchase <= 90),
                ChurnedCustomers = customers.Count(c => c.Segment == CustomerSegment.Churned),
                AverageLifetimeValue = customers.Any() ? customers.Average(c => c.LifetimeValue) : 0,
                CustomerAcquisitionTrend = GetCustomerAcquisitionTrend(customers),
                SegmentPerformance = await GetSegmentPerformance(customers, orders),
                TopCustomers = await _customerRepository.GetTopCustomersAsync(10),
                GeographicDistribution = GetGeographicDistribution(orders),
                CustomerRetentionRate = CalculateRetentionRate(customers, orders)
            };

            return analytics;
        }

        public async Task<ProductAnalytics> GetProductAnalyticsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            var orderItems = orders.SelectMany(o => o.Items).ToList();

            var analytics = new ProductAnalytics
            {
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.IsActive),
                OutOfStockProducts = products.Count(p => p.Stock == 0),
                LowStockProducts = products.Count(p => p.NeedsReorder),
                TotalInventoryValue = products.Sum(p => p.InventoryValue),
                BestSellingProducts = await _productRepository.GetTopSellingProductsAsync(10),
                CategoryPerformance = await GetCategoryPerformance(products, orderItems),
                PriceAnalysis = GetPriceAnalysis(products),
                InventoryTurnover = CalculateInventoryTurnover(products, orderItems),
                ProfitMarginAnalysis = GetProfitMarginAnalysis(products)
            };

            return analytics;
        }

        public async Task<CompetitiveAnalysis> GetCompetitiveAnalysisAsync()
        {
            var products = await _productRepository.GetAllAsync();
            
            // Simulate competitive data (in a real system, this would come from web scraping or APIs)
            var analysis = new CompetitiveAnalysis
            {
                TotalProductsTracked = products.Count,
                CompetitivePriceAdvantage = CalculateCompetitivePriceAdvantage(products),
                PricePositioning = GetPricePositioning(products),
                MarketShareEstimate = CalculateMarketShareEstimate(products),
                CompetitorInsights = GenerateCompetitorInsights(products)
            };

            return analysis;
        }

        private decimal CalculateConversionRate(List<Customer> customers, List<Order> completedOrders)
        {
            if (!customers.Any()) return 0;
            var customersWithOrders = completedOrders.Select(o => o.CustomerId).Distinct().Count();
            return (decimal)customersWithOrders / customers.Count * 100;
        }

        private decimal CalculateGrowthRate(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return ((current - previous) / previous) * 100;
        }

        private async Task<List<ProductSalesInfo>> GetTopSellingProducts(int count)
        {
            var orders = await _orderRepository.GetAllAsync();
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            
            return completedOrders.SelectMany(o => o.Items)
                                 .GroupBy(i => i.ProductId)
                                 .Select(g => new ProductSalesInfo
                                 {
                                     ProductId = g.Key,
                                     ProductName = g.First().ProductName,
                                     QuantitySold = g.Sum(i => i.Quantity),
                                     Revenue = g.Sum(i => i.LineTotal)
                                 })
                                 .OrderByDescending(p => p.Revenue)
                                 .Take(count)
                                 .ToList();
        }

        private async Task<List<string>> GetRecentActivity(int count)
        {
            var orders = await _orderRepository.GetRecentOrdersAsync(count);
            return orders.Select(o => $"Order #{o.Id} - {o.Status} - ${o.TotalAmount:F2}").ToList();
        }

        private Dictionary<string, int> GetCustomerAcquisitionTrend(List<Customer> customers)
        {
            return customers.Where(c => c.RegistrationDate >= DateTime.Now.AddMonths(-12))
                           .GroupBy(c => c.RegistrationDate.ToString("yyyy-MM"))
                           .OrderBy(g => g.Key)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<Dictionary<string, object>> GetSegmentPerformance(List<Customer> customers, List<Order> orders)
        {
            var performance = new Dictionary<string, object>();
            
            foreach (CustomerSegment segment in Enum.GetValues<CustomerSegment>())
            {
                var segmentCustomers = customers.Where(c => c.Segment == segment).ToList();
                var segmentOrders = orders.Where(o => segmentCustomers.Any(c => c.Id == o.CustomerId) && 
                                                     o.Status == OrderStatus.Delivered).ToList();
                
                performance[segment.ToString()] = new
                {
                    CustomerCount = segmentCustomers.Count,
                    Revenue = segmentOrders.Sum(o => o.TotalAmount),
                    AverageOrderValue = segmentOrders.Any() ? segmentOrders.Average(o => o.TotalAmount) : 0,
                    OrderFrequency = segmentCustomers.Any() ? (decimal)segmentOrders.Count / segmentCustomers.Count : 0
                };
            }
            
            return performance;
        }

        private Dictionary<string, string> GetGeographicDistribution(List<Order> orders)
        {
            // Simulate geographic data
            var random = new Random();
            var regions = new[] { "North", "South", "East", "West", "Central" };
            
            return regions.ToDictionary(r => r, r => $"{random.Next(10, 30)}%");
        }

        private decimal CalculateRetentionRate(List<Customer> customers, List<Order> orders)
        {
            var activeCustomers = customers.Where(c => c.DaysSinceLastPurchase <= 90).Count();
            return customers.Any() ? (decimal)activeCustomers / customers.Count * 100 : 0;
        }

        private async Task<Dictionary<string, object>> GetCategoryPerformance(List<Product> products, List<OrderItem> orderItems)
        {
            return products.GroupBy(p => p.Category)
                          .ToDictionary(g => g.Key, g => new
                          {
                              ProductCount = g.Count(),
                              Revenue = orderItems.Where(i => g.Any(p => p.Id == i.ProductId))
                                                 .Sum(i => i.LineTotal),
                              AveragePrice = g.Average(p => p.CurrentPrice),
                              TotalStock = g.Sum(p => p.Stock)
                          } as object);
        }

        private Dictionary<string, object> GetPriceAnalysis(List<Product> products)
        {
            return new Dictionary<string, object>
            {
                ["AveragePrice"] = products.Average(p => p.CurrentPrice),
                ["MedianPrice"] = CalculateMedian(products.Select(p => p.CurrentPrice).ToList()),
                ["PriceRange"] = $"${products.Min(p => p.CurrentPrice):F2} - ${products.Max(p => p.CurrentPrice):F2}",
                ["PriceDistribution"] = GetPriceDistribution(products)
            };
        }

        private decimal CalculateMedian(List<decimal> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            var count = sorted.Count;
            
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            else
                return sorted[count / 2];
        }

        private Dictionary<string, int> GetPriceDistribution(List<Product> products)
        {
            return new Dictionary<string, int>
            {
                ["$0-$25"] = products.Count(p => p.CurrentPrice <= 25),
                ["$26-$50"] = products.Count(p => p.CurrentPrice > 25 && p.CurrentPrice <= 50),
                ["$51-$100"] = products.Count(p => p.CurrentPrice > 50 && p.CurrentPrice <= 100),
                ["$101-$250"] = products.Count(p => p.CurrentPrice > 100 && p.CurrentPrice <= 250),
                ["$250+"] = products.Count(p => p.CurrentPrice > 250)
            };
        }

        private Dictionary<string, decimal> CalculateInventoryTurnover(List<Product> products, List<OrderItem> orderItems)
        {
            return products.GroupBy(p => p.Category)
                          .ToDictionary(g => g.Key, g =>
                          {
                              var categoryProducts = g.ToList();
                              var categorySold = orderItems.Where(i => categoryProducts.Any(p => p.Id == i.ProductId))
                                                          .Sum(i => i.Quantity);
                              var avgInventory = (decimal)categoryProducts.Average(p => p.Stock);
                              return avgInventory > 0 ? (decimal)categorySold / avgInventory : 0m;
                          });
        }

        private Dictionary<string, decimal> GetProfitMarginAnalysis(List<Product> products)
        {
            return products.GroupBy(p => p.Category)
                          .ToDictionary(g => g.Key, g => g.Average(p => p.ProfitMargin));
        }

        private decimal CalculateCompetitivePriceAdvantage(List<Product> products)
        {
            // Simulate competitive advantage calculation
            return 12.5m; // 12.5% average price advantage
        }

        private Dictionary<string, string> GetPricePositioning(List<Product> products)
        {
            return new Dictionary<string, string>
            {
                ["Premium"] = "25%",
                ["Competitive"] = "60%",
                ["Discount"] = "15%"
            };
        }

        private decimal CalculateMarketShareEstimate(List<Product> products)
        {
            // Simulate market share calculation
            return 8.3m; // 8.3% estimated market share
        }

        private List<string> GenerateCompetitorInsights(List<Product> products)
        {
            return new List<string>
            {
                "Competitor A has increased prices by 3% this month",
                "Competitor B launched 5 new products in Electronics category",
                "Market average price decreased by 1.2% for Clothing items",
                "Seasonal demand spike expected for Home & Garden products"
            };
        }
    }

    // Analytics model classes
    public class DashboardMetrics
    {
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TodaysOrders { get; set; }
        public decimal TodaysRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ConversionRate { get; set; }
        public int LowStockAlerts { get; set; }
        public int PendingOrders { get; set; }
        public Dictionary<CustomerSegment, int> CustomerSegmentDistribution { get; set; } = new();
        public decimal RevenueGrowth { get; set; }
        public List<ProductSalesInfo> TopSellingProducts { get; set; } = new();
        public List<string> RecentActivity { get; set; } = new();
    }

    public class CustomerAnalytics
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveCustomers { get; set; }
        public int ChurnedCustomers { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public Dictionary<string, int> CustomerAcquisitionTrend { get; set; } = new();
        public Dictionary<string, object> SegmentPerformance { get; set; } = new();
        public List<Customer> TopCustomers { get; set; } = new();
        public Dictionary<string, string> GeographicDistribution { get; set; } = new();
        public decimal CustomerRetentionRate { get; set; }
    }

    public class ProductAnalytics
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<Product> BestSellingProducts { get; set; } = new();
        public Dictionary<string, object> CategoryPerformance { get; set; } = new();
        public Dictionary<string, object> PriceAnalysis { get; set; } = new();
        public Dictionary<string, decimal> InventoryTurnover { get; set; } = new();
        public Dictionary<string, decimal> ProfitMarginAnalysis { get; set; } = new();
    }

    public class CompetitiveAnalysis
    {
        public int TotalProductsTracked { get; set; }
        public decimal CompetitivePriceAdvantage { get; set; }
        public Dictionary<string, string> PricePositioning { get; set; } = new();
        public decimal MarketShareEstimate { get; set; }
        public List<string> CompetitorInsights { get; set; } = new();
    }
}