using System;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public CustomerSegment Segment { get; set; }
        public DateTime RegistrationDate { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public List<int> PurchaseHistory { get; set; } = new List<int>();
        public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();
        
        public bool IsVip => Segment == CustomerSegment.VIP;
        public int DaysSinceLastPurchase => (DateTime.Now - LastPurchaseDate).Days;
    }

    public enum CustomerSegment
    {
        New,
        Regular,
        VIP,
        Premium,
        Churned
    }
}