using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Services
{
    public class OrderService
    {
        private readonly JsonDataManager _dataManager;
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;
        private List<Order> _orders;

        public OrderService(JsonDataManager dataManager, ProductService productService, CustomerService customerService)
        {
            _dataManager = dataManager;
            _productService = productService;
            _customerService = customerService;
            _orders = _dataManager.LoadData<Order>("orders");
        }

        public List<Order> GetAllOrders() => _orders.ToList();

        public Order? GetOrderById(int id) => _orders.FirstOrDefault(o => o.Id == id);

        public List<Order> GetOrdersByCustomer(int customerId) =>
            _orders.Where(o => o.CustomerId == customerId).OrderByDescending(o => o.OrderDate).ToList();

        public List<Order> GetOrdersByStatus(OrderStatus status) =>
            _orders.Where(o => o.Status == status).ToList();

        public List<Order> GetOrdersByDateRange(DateTime startDate, DateTime endDate) =>
            _orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();

        public Order CreateOrder(int customerId, List<(int ProductId, int Quantity)> items)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            var order = new Order
            {
                Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1,
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            };

            foreach (var (productId, quantity) in items)
            {
                var product = _productService.GetProductById(productId);
                if (product == null || product.Stock < quantity)
                    continue;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.CurrentPrice,
                    DiscountApplied = product.CurrentPrice * customer.GetDiscountRate() * quantity
                };
                
                orderItem.CalculateTotal();
                order.AddItem(orderItem);

                // Update product stock
                _productService.UpdateStock(productId, quantity, false);
            }

            if (!order.Items.Any())
                throw new InvalidOperationException("No valid items in order");

            // Apply customer-level discount
            order.DiscountAmount = order.TotalAmount * customer.GetDiscountRate();
            order.CalculateTotals();

            _orders.Add(order);
            _dataManager.SaveData(_orders, "orders");

            // Update customer purchase history
            _customerService.UpdateCustomerPurchase(customerId, order.FinalAmount);

            return order;
        }

        public bool UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = GetOrderById(orderId);
            if (order == null)
                return false;

            order.UpdateStatus(newStatus);
            _dataManager.SaveData(_orders, "orders");
            return true;
        }

        public bool CancelOrder(int orderId)
        {
            var order = GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.Pending)
                return false;

            // Restore product stock
            foreach (var item in order.Items)
            {
                _productService.UpdateStock(item.ProductId, item.Quantity, true);
            }

            order.UpdateStatus(OrderStatus.Cancelled);
            _dataManager.SaveData(_orders, "orders");
            return true;
        }

        public bool AddItemToOrder(int orderId, int productId, int quantity)
        {
            var order = GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.Pending)
                return false;

            var product = _productService.GetProductById(productId);
            if (product == null || product.Stock < quantity)
                return false;

            var customer = _customerService.GetCustomerById(order.CustomerId);
            if (customer == null)
                return false;

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = productId,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.CurrentPrice,
                DiscountApplied = product.CurrentPrice * customer.GetDiscountRate() * quantity
            };
            
            orderItem.CalculateTotal();
            order.AddItem(orderItem);
            order.CalculateTotals();

            _productService.UpdateStock(productId, quantity, false);
            _dataManager.SaveData(_orders, "orders");
            
            return true;
        }

        public bool RemoveItemFromOrder(int orderId, int productId)
        {
            var order = GetOrderById(orderId);
            if (order == null || order.Status != OrderStatus.Pending)
                return false;

            var item = order.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return false;

            // Restore stock
            _productService.UpdateStock(productId, item.Quantity, true);
            
            order.RemoveItem(productId);
            _dataManager.SaveData(_orders, "orders");
            
            return true;
        }

        public List<Order> GetRecentOrders(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return _orders.Where(o => o.OrderDate >= cutoffDate)
                         .OrderByDescending(o => o.OrderDate)
                         .ToList();
        }

        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            
            return _orders.Where(o => o.OrderDate >= start && o.OrderDate <= end && 
                                o.Status != OrderStatus.Cancelled)
                         .Sum(o => o.FinalAmount);
        }

        public Dictionary<string, decimal> GetRevenueByCategory(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            
            return _orders.Where(o => o.OrderDate >= start && o.OrderDate <= end && 
                                o.Status != OrderStatus.Cancelled)
                         .SelectMany(o => o.Items)
                         .Join(_productService.GetAllProducts(), 
                               item => item.ProductId, 
                               product => product.Id,
                               (item, product) => new { Category = product.Category, Revenue = item.TotalPrice })
                         .GroupBy(x => x.Category)
                         .ToDictionary(g => g.Key, g => g.Sum(x => x.Revenue));
        }

        public List<(int ProductId, string ProductName, int TotalSold)> GetTopSellingProducts(int count = 10, int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            
            return _orders.Where(o => o.OrderDate >= cutoffDate && o.Status != OrderStatus.Cancelled)
                         .SelectMany(o => o.Items)
                         .GroupBy(i => new { i.ProductId, i.ProductName })
                         .Select(g => new { g.Key.ProductId, g.Key.ProductName, TotalSold = g.Sum(i => i.Quantity) })
                         .OrderByDescending(x => x.TotalSold)
                         .Take(count)
                         .Select(x => (x.ProductId, x.ProductName, x.TotalSold))
                         .ToList();
        }

        public decimal GetAverageOrderValue(int days = 30)
        {
            var recentOrders = GetRecentOrders(days).Where(o => o.Status != OrderStatus.Cancelled);
            return recentOrders.Any() ? recentOrders.Average(o => o.FinalAmount) : 0;
        }

        public Dictionary<OrderStatus, int> GetOrderStatusDistribution()
        {
            return _orders.GroupBy(o => o.Status)
                         .ToDictionary(g => g.Key, g => g.Count());
        }

        public List<Order> GetPendingOrders() => GetOrdersByStatus(OrderStatus.Pending);

        public List<Order> GetOrdersToShip() => GetOrdersByStatus(OrderStatus.Processing);

        public int GetTotalOrderCount(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;
            
            return _orders.Count(o => o.OrderDate >= start && o.OrderDate <= end && 
                               o.Status != OrderStatus.Cancelled);
        }

        public List<Order> SearchOrders(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _orders.ToList();

            return _orders.Where(o => 
                o.Id.ToString().Contains(searchTerm) ||
                o.Notes.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                o.Items.Any(i => i.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        public void RefreshData()
        {
            _orders = _dataManager.LoadData<Order>("orders");
        }
    }
}