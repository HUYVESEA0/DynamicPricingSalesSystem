using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Services
{
    public class CustomerService
    {
        private readonly JsonDataManager _dataManager;
        private List<Customer> _customers;

        public CustomerService(JsonDataManager dataManager)
        {
            _dataManager = dataManager;
            _customers = _dataManager.LoadData<Customer>("customers");
        }

        public List<Customer> GetAllCustomers() => _customers.ToList();

        public Customer? GetCustomerById(int id) => _customers.FirstOrDefault(c => c.Id == id);

        public Customer? GetCustomerByEmail(string email) => 
            _customers.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public List<Customer> GetCustomersBySegment(CustomerSegment segment) =>
            _customers.Where(c => c.Segment == segment).ToList();

        public Customer CreateCustomer(Customer customer)
        {
            customer.Id = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
            customer.RegistrationDate = DateTime.Now;
            customer.IsActive = true;
            
            _customers.Add(customer);
            _dataManager.SaveData(_customers, "customers");
            
            return customer;
        }

        public bool UpdateCustomer(Customer customer)
        {
            var existingCustomer = GetCustomerById(customer.Id);
            if (existingCustomer == null)
                return false;

            var index = _customers.IndexOf(existingCustomer);
            _customers[index] = customer;
            
            _dataManager.SaveData(_customers, "customers");
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            var customer = GetCustomerById(id);
            if (customer == null)
                return false;

            customer.IsActive = false; // Soft delete
            _dataManager.SaveData(_customers, "customers");
            return true;
        }

        public List<Customer> SearchCustomers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _customers.Where(c => c.IsActive).ToList();

            return _customers.Where(c => c.IsActive && (
                c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            )).ToList();
        }

        public void UpdateCustomerPurchase(int customerId, decimal orderValue)
        {
            var customer = GetCustomerById(customerId);
            if (customer == null)
                return;

            customer.TotalSpent += orderValue;
            customer.TotalOrders++;
            customer.LastPurchaseDate = DateTime.Now;
            
            // Update loyalty score based on purchase behavior
            UpdateLoyaltyScore(customer);
            customer.UpdateSegment();
            
            _dataManager.SaveData(_customers, "customers");
        }

        private void UpdateLoyaltyScore(Customer customer)
        {
            var daysSinceRegistration = Math.Max(1, (DateTime.Now - customer.RegistrationDate).Days);
            var purchaseFrequency = customer.TotalOrders / (daysSinceRegistration / 30.0);
            var spendingLevel = Math.Min(1.0, (double)customer.TotalSpent / 1000.0);
            
            customer.LoyaltyScore = Math.Min(1.0, (purchaseFrequency * 0.6 + spendingLevel * 0.4));
        }

        public List<Customer> GetAtRiskCustomers()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            return _customers.Where(c => c.IsActive && 
                                   c.LastPurchaseDate < thirtyDaysAgo &&
                                   c.TotalOrders > 0)
                           .ToList();
        }

        public List<Customer> GetVIPCustomers()
        {
            return _customers.Where(c => c.IsActive && c.Segment == CustomerSegment.VIP).ToList();
        }

        public List<Customer> GetNewCustomers(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return _customers.Where(c => c.IsActive && c.RegistrationDate >= cutoffDate).ToList();
        }

        public Dictionary<CustomerSegment, int> GetSegmentDistribution()
        {
            return _customers.Where(c => c.IsActive)
                           .GroupBy(c => c.Segment)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        public List<Customer> GetTopCustomersBySpending(int count = 10)
        {
            return _customers.Where(c => c.IsActive)
                           .OrderByDescending(c => c.TotalSpent)
                           .Take(count)
                           .ToList();
        }

        public List<Customer> GetTopCustomersByOrders(int count = 10)
        {
            return _customers.Where(c => c.IsActive)
                           .OrderByDescending(c => c.TotalOrders)
                           .Take(count)
                           .ToList();
        }

        public decimal GetCustomerLifetimeValue(int customerId)
        {
            var customer = GetCustomerById(customerId);
            return customer != null ? (decimal)customer.CalculateCustomerLifetimeValue() : 0;
        }

        public void UpdateAllSegments()
        {
            foreach (var customer in _customers.Where(c => c.IsActive))
            {
                customer.UpdateSegment();
            }
            _dataManager.SaveData(_customers, "customers");
        }

        public List<string> GetPopularCategories(int customerId)
        {
            var customer = GetCustomerById(customerId);
            return customer?.PreferredCategories ?? new List<string>();
        }

        public void AddPreferredCategory(int customerId, string category)
        {
            var customer = GetCustomerById(customerId);
            if (customer != null && !customer.PreferredCategories.Contains(category))
            {
                customer.PreferredCategories.Add(category);
                _dataManager.SaveData(_customers, "customers");
            }
        }

        public CustomerSegment PredictCustomerSegment(Customer customer)
        {
            // Simple rule-based prediction
            var daysSinceRegistration = (DateTime.Now - customer.RegistrationDate).Days;
            
            if (daysSinceRegistration <= 30)
                return CustomerSegment.New;
            
            if (customer.TotalSpent >= 1000 && customer.LoyaltyScore >= 0.8)
                return CustomerSegment.VIP;
            
            if (customer.DaysSinceLastPurchase > 90)
                return CustomerSegment.AtRisk;
            
            return CustomerSegment.Regular;
        }

        public void RefreshData()
        {
            _customers = _dataManager.LoadData<Customer>("customers");
        }
    }
}