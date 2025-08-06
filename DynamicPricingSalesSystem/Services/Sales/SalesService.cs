using DynamicPricingSalesSystem.Data.Repositories;
using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Services.Sales
{
    public class SalesService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        public SalesService(CustomerRepository customerRepository, 
                           ProductRepository productRepository, 
                           OrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            // Validate products and stock
            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product {item.ProductId} not found");
                
                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                
                item.ProductName = product.Name;
                item.UnitPrice = product.CurrentPrice;
            }

            var order = new Order
            {
                CustomerId = customerId,
                Items = items,
                Status = OrderStatus.Pending,
                TaxAmount = CalculateTax(items),
                ShippingCost = CalculateShipping(items, customer),
                DiscountAmount = CalculateDiscount(items, customer)
            };

            order.CalculateTotals();
            return await _orderRepository.AddAsync(order);
        }

        public async Task<bool> ProcessOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Pending)
                return false;

            // Update inventory
            foreach (var item in order.Items)
            {
                await _productRepository.UpdateStockAsync(item.ProductId, 
                    (await _productRepository.GetByIdAsync(item.ProductId))!.Stock - item.Quantity);
                
                // Update product metrics
                await _productRepository.UpdateProductMetricsAsync(item.ProductId, 
                    item.Quantity, item.LineTotal);
            }

            // Update customer metrics
            await _customerRepository.UpdateCustomerMetricsAsync(order.CustomerId, order.TotalAmount);

            // Update order status
            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Confirmed);

            return true;
        }

        public async Task<bool> ShipOrderAsync(int orderId, string trackingNumber = "")
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Confirmed)
                return false;

            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Processing);
            await Task.Delay(100); // Simulate processing time
            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Shipped);

            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason = "")
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                return false;

            // Restore inventory if order was confirmed
            if (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Processing)
            {
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        await _productRepository.UpdateStockAsync(item.ProductId, 
                            product.Stock + item.Quantity);
                    }
                }
            }

            await _orderRepository.UpdateStatusAsync(orderId, OrderStatus.Cancelled);
            return true;
        }

        public async Task<SalesReport> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderRepository.GetByDateRangeAsync(startDate, endDate);
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

            var report = new SalesReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = orders.Count,
                CompletedOrders = completedOrders.Count,
                TotalRevenue = completedOrders.Sum(o => o.TotalAmount),
                AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0,
                TopProducts = await GetTopProductsInPeriod(completedOrders, 5),
                CustomerSegmentAnalysis = await GetCustomerSegmentAnalysis(completedOrders),
                DailySales = GetDailySales(completedOrders)
            };

            return report;
        }

        private decimal CalculateTax(List<OrderItem> items)
        {
            var subtotal = items.Sum(i => i.LineTotal);
            return subtotal * 0.08m; // 8% tax rate
        }

        private decimal CalculateShipping(List<OrderItem> items, Customer customer)
        {
            var totalQuantity = items.Sum(i => i.Quantity);
            var baseShipping = 5.99m;
            
            if (customer.IsVip)
                return 0; // Free shipping for VIP
            
            if (items.Sum(i => i.LineTotal) > 100)
                return 0; // Free shipping over $100
            
            return baseShipping + (totalQuantity > 5 ? (totalQuantity - 5) * 1.50m : 0);
        }

        private decimal CalculateDiscount(List<OrderItem> items, Customer customer)
        {
            var subtotal = items.Sum(i => i.LineTotal);
            decimal discount = 0;

            // Customer segment discount
            discount += customer.Segment switch
            {
                CustomerSegment.VIP => subtotal * 0.10m,
                CustomerSegment.Premium => subtotal * 0.05m,
                CustomerSegment.Regular => subtotal * 0.02m,
                _ => 0
            };

            // Volume discount
            if (items.Sum(i => i.Quantity) >= 10)
                discount += subtotal * 0.05m;

            return discount;
        }

        private async Task<List<ProductSalesInfo>> GetTopProductsInPeriod(List<Order> orders, int count)
        {
            var productSales = orders.SelectMany(o => o.Items)
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

            return productSales;
        }

        private async Task<Dictionary<string, object>> GetCustomerSegmentAnalysis(List<Order> orders)
        {
            var customerIds = orders.Select(o => o.CustomerId).Distinct();
            var segmentAnalysis = new Dictionary<string, object>();

            foreach (CustomerSegment segment in Enum.GetValues<CustomerSegment>())
            {
                var segmentCustomers = await _customerRepository.GetBySegmentAsync(segment);
                var segmentCustomerIds = segmentCustomers.Select(c => c.Id).ToHashSet();
                
                var segmentOrders = orders.Where(o => segmentCustomerIds.Contains(o.CustomerId)).ToList();
                
                segmentAnalysis[segment.ToString()] = new
                {
                    OrderCount = segmentOrders.Count,
                    Revenue = segmentOrders.Sum(o => o.TotalAmount),
                    AverageOrderValue = segmentOrders.Any() ? segmentOrders.Average(o => o.TotalAmount) : 0
                };
            }

            return segmentAnalysis;
        }

        private Dictionary<string, decimal> GetDailySales(List<Order> orders)
        {
            return orders.GroupBy(o => o.OrderDate.Date)
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), 
                                    g => g.Sum(o => o.TotalAmount));
        }

        public async Task<CustomerLifetimeValue> CalculateCustomerLifetimeValueAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var orders = await _orderRepository.GetByCustomerAsync(customerId);
            var completedOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

            if (!completedOrders.Any())
                return new CustomerLifetimeValue { CustomerId = customerId, Value = 0 };

            var totalSpent = completedOrders.Sum(o => o.TotalAmount);
            var averageOrderValue = completedOrders.Average(o => o.TotalAmount);
            var orderFrequency = CalculateOrderFrequency(completedOrders);
            var customerLifespan = CalculateCustomerLifespan(customer, completedOrders);

            var clv = averageOrderValue * orderFrequency * customerLifespan;

            return new CustomerLifetimeValue
            {
                CustomerId = customerId,
                Value = clv,
                AverageOrderValue = averageOrderValue,
                OrderFrequency = orderFrequency,
                CustomerLifespan = customerLifespan,
                TotalOrders = completedOrders.Count,
                TotalSpent = totalSpent
            };
        }

        private decimal CalculateOrderFrequency(List<Order> orders)
        {
            if (orders.Count < 2) return 1;

            var daysBetweenOrders = new List<double>();
            for (int i = 1; i < orders.Count; i++)
            {
                var days = (orders[i].OrderDate - orders[i - 1].OrderDate).TotalDays;
                if (days > 0) daysBetweenOrders.Add(days);
            }

            if (!daysBetweenOrders.Any()) return 1;

            var averageDaysBetween = daysBetweenOrders.Average();
            return 365m / (decimal)averageDaysBetween; // Orders per year
        }

        private decimal CalculateCustomerLifespan(Customer customer, List<Order> orders)
        {
            if (!orders.Any()) return 1;

            var daysSinceFirst = (DateTime.Now - customer.RegistrationDate).TotalDays;
            var daysSinceLastOrder = (DateTime.Now - orders.Max(o => o.OrderDate)).TotalDays;

            // If customer is active (ordered within 90 days), predict future lifespan
            if (daysSinceLastOrder <= 90)
            {
                return Math.Max(2, (decimal)(daysSinceFirst / 365) * 1.5m); // Predict 1.5x current lifespan
            }

            return Math.Max(1, (decimal)(daysSinceFirst / 365)); // Use actual lifespan
        }
    }

    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<ProductSalesInfo> TopProducts { get; set; } = new List<ProductSalesInfo>();
        public Dictionary<string, object> CustomerSegmentAnalysis { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, decimal> DailySales { get; set; } = new Dictionary<string, decimal>();
    }

    public class ProductSalesInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerLifetimeValue
    {
        public int CustomerId { get; set; }
        public decimal Value { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal OrderFrequency { get; set; }
        public decimal CustomerLifespan { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}