using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Data.Repositories
{
    public class CustomerRepository
    {
        private readonly JsonDataStorage _storage;
        private List<Customer> _customers;

        public CustomerRepository(JsonDataStorage storage)
        {
            _storage = storage;
            _customers = new List<Customer>();
        }

        public async Task LoadAsync()
        {
            _customers = await _storage.LoadDataAsync<Customer>("customers");
        }

        public async Task SaveAsync()
        {
            await _storage.SaveDataAsync(_customers, "customers");
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return _customers.FirstOrDefault(c => c.Id == id);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return _customers.ToList();
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            if (customer.Id == 0)
            {
                customer.Id = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
            }
            
            customer.RegistrationDate = DateTime.Now;
            _customers.Add(customer);
            await SaveAsync();
            return customer;
        }

        public async Task<Customer?> UpdateAsync(Customer customer)
        {
            var existingCustomer = _customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existingCustomer == null) return null;

            var index = _customers.IndexOf(existingCustomer);
            _customers[index] = customer;
            await SaveAsync();
            return customer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return false;

            _customers.Remove(customer);
            await SaveAsync();
            return true;
        }

        public async Task<List<Customer>> GetBySegmentAsync(CustomerSegment segment)
        {
            return _customers.Where(c => c.Segment == segment).ToList();
        }

        public async Task<List<Customer>> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            return _customers.Where(c => 
                c.Name.ToLower().Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm) ||
                c.Phone.Contains(searchTerm)
            ).ToList();
        }

        public async Task UpdateCustomerMetricsAsync(int customerId, decimal orderAmount)
        {
            var customer = await GetByIdAsync(customerId);
            if (customer == null) return;

            customer.TotalSpent += orderAmount;
            customer.TotalOrders++;
            customer.LastPurchaseDate = DateTime.Now;
            
            // Update customer segment based on spending
            UpdateCustomerSegment(customer);
            
            await UpdateAsync(customer);
        }

        private void UpdateCustomerSegment(Customer customer)
        {
            if (customer.TotalSpent >= 10000)
                customer.Segment = CustomerSegment.Premium;
            else if (customer.TotalSpent >= 5000)
                customer.Segment = CustomerSegment.VIP;
            else if (customer.TotalOrders >= 5)
                customer.Segment = CustomerSegment.Regular;
            else if (customer.DaysSinceLastPurchase > 90)
                customer.Segment = CustomerSegment.Churned;
            else
                customer.Segment = CustomerSegment.New;
        }

        public async Task<Dictionary<CustomerSegment, int>> GetSegmentDistributionAsync()
        {
            return _customers.GroupBy(c => c.Segment)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<List<Customer>> GetTopCustomersAsync(int count = 10)
        {
            return _customers.OrderByDescending(c => c.TotalSpent)
                           .Take(count)
                           .ToList();
        }
    }
}