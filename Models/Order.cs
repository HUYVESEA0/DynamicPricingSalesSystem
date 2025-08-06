using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicPricingSalesSystem.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public void CalculateTotals()
        {
            TotalAmount = Items.Sum(item => item.TotalPrice);
            FinalAmount = TotalAmount - DiscountAmount;
        }

        public int TotalItems => Items.Sum(item => item.Quantity);

        public void AddItem(OrderItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                existingItem.CalculateTotal();
            }
            else
            {
                Items.Add(item);
            }
            CalculateTotals();
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
            CalculateTotals();
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            
            switch (newStatus)
            {
                case OrderStatus.Shipped:
                    ShippedDate = DateTime.Now;
                    break;
                case OrderStatus.Delivered:
                    DeliveredDate = DateTime.Now;
                    break;
            }
        }
    }
}