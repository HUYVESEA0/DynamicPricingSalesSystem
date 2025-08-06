using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Data
{
    public class SampleDataGenerator
    {
        private readonly Random _random;
        private readonly string[] _categories = { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Toys", "Health", "Beauty" };
        private readonly string[] _firstNames = { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna" };
        private readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
        private readonly string[] _competitors = { "CompetitorA", "CompetitorB", "CompetitorC", "MarketLeader", "Discount Store" };

        public SampleDataGenerator()
        {
            _random = new Random();
        }

        public List<Product> GenerateProducts(int count = 50)
        {
            var products = new List<Product>();
            var productNames = new Dictionary<string, string[]>
            {
                ["Electronics"] = new[] { "Smartphone", "Laptop", "Tablet", "Headphones", "Camera", "TV", "Speaker", "Watch" },
                ["Clothing"] = new[] { "T-Shirt", "Jeans", "Dress", "Jacket", "Shoes", "Hat", "Sweater", "Shorts" },
                ["Books"] = new[] { "Novel", "Textbook", "Biography", "Cookbook", "Manual", "Guide", "Dictionary", "Atlas" },
                ["Home & Garden"] = new[] { "Chair", "Table", "Lamp", "Plant", "Tool", "Vase", "Mirror", "Cushion" },
                ["Sports"] = new[] { "Ball", "Racket", "Bike", "Weights", "Mat", "Shoes", "Bottle", "Bag" },
                ["Toys"] = new[] { "Doll", "Car", "Puzzle", "Game", "Block", "Bear", "Robot", "Kite" },
                ["Health"] = new[] { "Vitamins", "Supplement", "Monitor", "Scale", "Thermometer", "Bandage", "Cream", "Pills" },
                ["Beauty"] = new[] { "Lipstick", "Foundation", "Perfume", "Shampoo", "Lotion", "Brush", "Mirror", "Serum" }
            };

            for (int i = 1; i <= count; i++)
            {
                var category = _categories[_random.Next(_categories.Length)];
                var nameOptions = productNames[category];
                var baseName = nameOptions[_random.Next(nameOptions.Length)];
                var brand = $"Brand{_random.Next(1, 20)}";
                
                var cost = (decimal)(_random.NextDouble() * 80 + 10); // $10-$90
                var basePrice = cost * (decimal)(_random.NextDouble() * 1.5 + 1.5); // 1.5x to 3x markup

                var product = new Product
                {
                    Id = i,
                    Name = $"{brand} {baseName} {_random.Next(100, 999)}",
                    Category = category,
                    BasePrice = Math.Round(basePrice, 2),
                    CurrentPrice = Math.Round(basePrice, 2),
                    Cost = Math.Round(cost, 2),
                    Stock = _random.Next(0, 200),
                    MinPrice = Math.Round(cost * 1.1m, 2), // 10% above cost
                    MaxPrice = Math.Round(basePrice * 2m, 2), // 2x base price
                    LastUpdated = DateTime.Now.AddDays(-_random.Next(0, 30)),
                    SalesCount = _random.Next(0, 100),
                    DemandScore = _random.NextDouble() * 1.5 + 0.5, // 0.5 to 2.0
                    Supplier = $"Supplier{_random.Next(1, 10)}",
                    SeasonalityFactor = _random.NextDouble() * 0.4 + 0.8 // 0.8 to 1.2
                };

                products.Add(product);
            }

            return products;
        }

        public List<Customer> GenerateCustomers(int count = 100)
        {
            var customers = new List<Customer>();

            for (int i = 1; i <= count; i++)
            {
                var registrationDate = DateTime.Now.AddDays(-_random.Next(1, 730)); // Up to 2 years ago
                var lastPurchaseDate = registrationDate.AddDays(_random.Next(0, (DateTime.Now - registrationDate).Days));
                
                var customer = new Customer
                {
                    Id = i,
                    FirstName = _firstNames[_random.Next(_firstNames.Length)],
                    LastName = _lastNames[_random.Next(_lastNames.Length)],
                    Email = $"customer{i}@email.com",
                    Phone = $"555-{_random.Next(1000, 9999)}",
                    RegistrationDate = registrationDate,
                    LastPurchaseDate = lastPurchaseDate,
                    TotalSpent = (decimal)(_random.NextDouble() * 2000),
                    TotalOrders = _random.Next(1, 20),
                    PriceSensitivity = _random.NextDouble() * 1.5 + 0.5, // 0.5 to 2.0
                    LoyaltyScore = _random.NextDouble(),
                    IsActive = _random.NextDouble() > 0.1, // 90% active
                    PreferredCategories = _categories.OrderBy(x => Guid.NewGuid()).Take(_random.Next(1, 4)).ToList()
                };

                customer.UpdateSegment();
                customers.Add(customer);
            }

            return customers;
        }

        public List<Order> GenerateOrders(List<Product> products, List<Customer> customers, int count = 200)
        {
            var orders = new List<Order>();

            for (int i = 1; i <= count; i++)
            {
                var customer = customers[_random.Next(customers.Count)];
                var orderDate = DateTime.Now.AddDays(-_random.Next(0, 90));
                
                var order = new Order
                {
                    Id = i,
                    CustomerId = customer.Id,
                    OrderDate = orderDate,
                    Status = (OrderStatus)_random.Next(0, 5)
                };

                // Add 1-5 items to the order
                var itemCount = _random.Next(1, 6);
                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 4);
                    
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = quantity,
                        UnitPrice = product.CurrentPrice,
                        DiscountApplied = product.CurrentPrice * customer.GetDiscountRate() * quantity
                    };
                    
                    orderItem.CalculateTotal();
                    order.AddItem(orderItem);
                }

                // Apply customer discount
                order.DiscountAmount = order.TotalAmount * customer.GetDiscountRate();
                order.CalculateTotals();

                if (order.Status >= OrderStatus.Shipped)
                {
                    order.ShippedDate = orderDate.AddDays(_random.Next(1, 3));
                }
                if (order.Status == OrderStatus.Delivered)
                {
                    order.DeliveredDate = order.ShippedDate?.AddDays(_random.Next(1, 7));
                }

                orders.Add(order);
            }

            return orders;
        }

        public List<PricingRule> GeneratePricingRules()
        {
            return new List<PricingRule>
            {
                new PricingRule
                {
                    Id = 1,
                    Name = "Low Stock Premium",
                    RuleType = PricingRuleType.InventoryBased,
                    IsActive = true,
                    Priority = 1,
                    Conditions = "Stock < 10",
                    MinPriceMultiplier = 1.0m,
                    MaxPriceMultiplier = 1.3m,
                    Description = "Increase price when stock is low"
                },
                new PricingRule
                {
                    Id = 2,
                    Name = "High Demand Surge",
                    RuleType = PricingRuleType.DemandBased,
                    IsActive = true,
                    Priority = 2,
                    Conditions = "DemandScore > 1.5",
                    MinPriceMultiplier = 1.0m,
                    MaxPriceMultiplier = 1.4m,
                    Description = "Increase price during high demand"
                },
                new PricingRule
                {
                    Id = 3,
                    Name = "Competitive Pricing",
                    RuleType = PricingRuleType.CompetitorBased,
                    IsActive = true,
                    Priority = 3,
                    Conditions = "CompetitorPrice < CurrentPrice",
                    MinPriceMultiplier = 0.9m,
                    MaxPriceMultiplier = 1.1m,
                    Description = "Match competitor prices"
                },
                new PricingRule
                {
                    Id = 4,
                    Name = "VIP Customer Discount",
                    RuleType = PricingRuleType.CustomerSegmentBased,
                    IsActive = true,
                    Priority = 4,
                    Conditions = "CustomerSegment = VIP",
                    MinPriceMultiplier = 0.8m,
                    MaxPriceMultiplier = 0.95m,
                    Description = "Special pricing for VIP customers"
                },
                new PricingRule
                {
                    Id = 5,
                    Name = "Weekend Premium",
                    RuleType = PricingRuleType.TimeBased,
                    IsActive = true,
                    Priority = 5,
                    Conditions = "DayOfWeek = Saturday OR Sunday",
                    MinPriceMultiplier = 1.0m,
                    MaxPriceMultiplier = 1.2m,
                    Description = "Weekend pricing premium"
                }
            };
        }

        public List<Competitor> GenerateCompetitors()
        {
            return _competitors.Select((name, index) => new Competitor
            {
                Id = index + 1,
                Name = name,
                Website = $"www.{name.ToLower()}.com",
                IsActive = true,
                MarketShare = _random.NextDouble() * 0.3 + 0.05, // 5% to 35%
                PricingStrategy = _random.Next(3) switch
                {
                    0 => "Premium",
                    1 => "Competitive",
                    _ => "Discount"
                },
                LastUpdated = DateTime.Now.AddHours(-_random.Next(1, 24))
            }).ToList();
        }

        public List<CompetitorPrice> GenerateCompetitorPrices(List<Competitor> competitors, List<Product> products, int pricesPerCompetitor = 20)
        {
            var competitorPrices = new List<CompetitorPrice>();
            int id = 1;

            foreach (var competitor in competitors)
            {
                var selectedProducts = products.OrderBy(x => Guid.NewGuid()).Take(pricesPerCompetitor);
                
                foreach (var product in selectedProducts)
                {
                    var priceVariation = _random.NextDouble() * 0.4 - 0.2; // -20% to +20%
                    var competitorPrice = product.CurrentPrice * (decimal)(1 + priceVariation);
                    
                    competitorPrices.Add(new CompetitorPrice
                    {
                        Id = id++,
                        CompetitorId = competitor.Id,
                        ProductName = product.Name,
                        Category = product.Category,
                        Price = Math.Round(competitorPrice, 2),
                        DateRecorded = DateTime.Now.AddHours(-_random.Next(1, 48)),
                        Source = "Web Scraping",
                        IsAvailable = _random.NextDouble() > 0.05 // 95% available
                    });
                }
            }

            return competitorPrices;
        }
    }
}