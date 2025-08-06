using System;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Models
{
    public enum CustomerSegment
    {
        New,
        Regular,
        VIP,
        AtRisk
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public CustomerSegment Segment { get; set; } = CustomerSegment.New;
        public DateTime RegistrationDate { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public double PriceSensitivity { get; set; } = 1.0; // 0.5 = price sensitive, 2.0 = price insensitive
        public List<string> PreferredCategories { get; set; } = new List<string>();
        public double LoyaltyScore { get; set; } = 0.0;
        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}";

        public decimal AverageOrderValue => TotalOrders > 0 ? TotalSpent / TotalOrders : 0;

        public int DaysSinceLastPurchase => (DateTime.Now - LastPurchaseDate).Days;

        public double CalculateCustomerLifetimeValue()
        {
            var avgOrderValue = (double)AverageOrderValue;
            var purchaseFrequency = TotalOrders / Math.Max(1, (DateTime.Now - RegistrationDate).Days / 30.0);
            var customerLifespan = Math.Max(1, (DateTime.Now - RegistrationDate).Days / 365.0);
            
            return avgOrderValue * purchaseFrequency * customerLifespan * LoyaltyScore;
        }

        public void UpdateSegment()
        {
            var daysSinceRegistration = (DateTime.Now - RegistrationDate).Days;
            var daysSinceLastPurchase = DaysSinceLastPurchase;

            if (daysSinceRegistration <= 30)
            {
                Segment = CustomerSegment.New;
            }
            else if (TotalSpent >= 1000 && LoyaltyScore >= 0.8)
            {
                Segment = CustomerSegment.VIP;
            }
            else if (daysSinceLastPurchase > 90)
            {
                Segment = CustomerSegment.AtRisk;
            }
            else
            {
                Segment = CustomerSegment.Regular;
            }
        }

        public decimal GetDiscountRate()
        {
            return Segment switch
            {
                CustomerSegment.VIP => 0.10m,
                CustomerSegment.Regular => 0.05m,
                CustomerSegment.AtRisk => 0.15m,
                CustomerSegment.New => 0.02m,
                _ => 0m
            };
        }
    }
}