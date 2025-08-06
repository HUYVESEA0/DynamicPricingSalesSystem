using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Data.Repositories
{
    public class OrderRepository
    {
        private readonly JsonDataStorage _storage;
        private List<Order> _orders;

        public OrderRepository(JsonDataStorage storage)
        {
            _storage = storage;
            _orders = new List<Order>();
        }

        public async Task LoadAsync()
        {
            _orders = await _storage.LoadDataAsync<Order>("orders");
        }

        public async Task SaveAsync()
        {
            await _storage.SaveDataAsync(_orders, "orders");
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return _orders.FirstOrDefault(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return _orders.ToList();
        }

        public async Task<Order> AddAsync(Order order)
        {
            if (order.Id == 0)
            {
                order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;
            }
            
            order.OrderDate = DateTime.Now;
            order.CalculateTotals();
            
            _orders.Add(order);
            await SaveAsync();
            return order;
        }

        public async Task<Order?> UpdateAsync(Order order)
        {
            var existingOrder = _orders.FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder == null) return null;

            order.CalculateTotals();
            var index = _orders.IndexOf(existingOrder);
            _orders[index] = order;
            await SaveAsync();
            return order;
        }

        public async Task<List<Order>> GetByCustomerAsync(int customerId)
        {
            return _orders.Where(o => o.CustomerId == customerId).ToList();
        }

        public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
        {
            return _orders.Where(o => o.Status == status).ToList();
        }

        public async Task<List<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return _orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
        }

        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            
            if (newStatus == OrderStatus.Shipped && !order.ShippingDate.HasValue)
            {
                order.ShippingDate = DateTime.Now;
            }
            else if (newStatus == OrderStatus.Delivered && !order.DeliveryDate.HasValue)
            {
                order.DeliveryDate = DateTime.Now;
            }

            await UpdateAsync(order);
            return true;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var filteredOrders = _orders.AsQueryable();
            
            if (startDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= startDate.Value);
            
            if (endDate.HasValue)
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= endDate.Value);

            return filteredOrders.Where(o => o.Status != OrderStatus.Cancelled && 
                                           o.Status != OrderStatus.Returned)
                                .Sum(o => o.TotalAmount);
        }

        public async Task<Dictionary<string, object>> GetOrderAnalyticsAsync()
        {
            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);
            var yearAgo = now.AddYears(-1);

            var recentOrders = _orders.Where(o => o.OrderDate >= thirtyDaysAgo).ToList();
            var yearlyOrders = _orders.Where(o => o.OrderDate >= yearAgo).ToList();

            var analytics = new Dictionary<string, object>
            {
                ["TotalOrders"] = _orders.Count,
                ["RecentOrders"] = recentOrders.Count,
                ["TotalRevenue"] = await GetTotalRevenueAsync(),
                ["RecentRevenue"] = recentOrders.Where(o => o.Status != OrderStatus.Cancelled && 
                                                            o.Status != OrderStatus.Returned)
                                                .Sum(o => o.TotalAmount),
                ["AverageOrderValue"] = _orders.Any() ? 
                    _orders.Where(o => o.Status != OrderStatus.Cancelled && 
                                      o.Status != OrderStatus.Returned)
                           .Average(o => o.TotalAmount) : 0,
                ["OrdersByStatus"] = _orders.GroupBy(o => o.Status)
                                            .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                ["MonthlyTrend"] = GetMonthlyTrend(yearlyOrders),
                ["TopCustomers"] = GetTopCustomersByOrders(10)
            };

            return analytics;
        }

        private Dictionary<string, decimal> GetMonthlyTrend(List<Order> orders)
        {
            return orders.Where(o => o.Status != OrderStatus.Cancelled && 
                                   o.Status != OrderStatus.Returned)
                        .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
                        .OrderBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
        }

        private Dictionary<int, object> GetTopCustomersByOrders(int count)
        {
            return _orders.Where(o => o.Status != OrderStatus.Cancelled && 
                                     o.Status != OrderStatus.Returned)
                         .GroupBy(o => o.CustomerId)
                         .Select(g => new 
                         { 
                             CustomerId = g.Key, 
                             OrderCount = g.Count(), 
                             TotalSpent = g.Sum(o => o.TotalAmount) 
                         })
                         .OrderByDescending(x => x.TotalSpent)
                         .Take(count)
                         .ToDictionary(x => x.CustomerId, 
                                     x => (object)new { OrderCount = x.OrderCount, TotalSpent = x.TotalSpent });
        }

        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            return _orders.Where(o => o.Status == OrderStatus.Pending || 
                                     o.Status == OrderStatus.Confirmed).ToList();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return _orders.OrderByDescending(o => o.OrderDate).Take(count).ToList();
        }
    }
}