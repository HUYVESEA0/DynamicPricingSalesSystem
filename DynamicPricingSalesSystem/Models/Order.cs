using System;
using System.Collections.Generic;

namespace DynamicPricingSalesSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public PaymentInfo Payment { get; set; } = new PaymentInfo();
        public DateTime? ShippingDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        public void CalculateTotals()
        {
            SubTotal = 0;
            foreach (var item in Items)
            {
                SubTotal += item.LineTotal;
            }
            TotalAmount = SubTotal + TaxAmount + ShippingCost - DiscountAmount;
        }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal LineTotal => Quantity * UnitPrice * (1 - DiscountPercent / 100);
    }

    public class PaymentInfo
    {
        public string Method { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }
}