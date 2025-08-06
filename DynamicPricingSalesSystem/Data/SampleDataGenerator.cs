using DynamicPricingSalesSystem.Data.Repositories;
using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Data
{
    public class SampleDataGenerator
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;
        private readonly Random _random;

        public SampleDataGenerator(CustomerRepository customerRepository,
                                 ProductRepository productRepository,
                                 OrderRepository orderRepository)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _random = new Random();
        }

        public async Task GenerateAllSampleDataAsync()
        {
            Console.WriteLine("Generating sample data...");
            
            await GenerateCustomersAsync(100);
            await GenerateProductsAsync(50);
            await GenerateHistoricalOrdersAsync(500);
            
            Console.WriteLine("Sample data generation completed!");
        }

        private async Task GenerateCustomersAsync(int count)
        {
            Console.WriteLine($"Generating {count} customers...");
            
            var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa", "Tom", "Anna",
                                   "James", "Mary", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth",
                                   "Richard", "Barbara", "Joseph", "Susan", "Thomas", "Jessica", "Charles", "Karen" };
            
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
                                  "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
                                  "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White" };

            for (int i = 1; i <= count; i++)
            {
                var firstName = firstNames[_random.Next(firstNames.Length)];
                var lastName = lastNames[_random.Next(lastNames.Length)];
                var registrationDate = DateTime.Now.AddDays(-_random.Next(1, 365 * 2));
                
                var customer = new Customer
                {
                    Name = $"{firstName} {lastName}",
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}@email.com",
                    Phone = $"+1-555-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}",
                    RegistrationDate = registrationDate,
                    Segment = GenerateCustomerSegment(),
                    LastPurchaseDate = registrationDate.AddDays(_random.Next(0, 30))
                };

                // Set spending based on segment
                customer.TotalSpent = customer.Segment switch
                {
                    CustomerSegment.Premium => _random.Next(10000, 25000),
                    CustomerSegment.VIP => _random.Next(5000, 15000),
                    CustomerSegment.Regular => _random.Next(1000, 5000),
                    CustomerSegment.New => _random.Next(0, 500),
                    CustomerSegment.Churned => _random.Next(500, 2000),
                    _ => _random.Next(100, 1000)
                };

                customer.TotalOrders = _random.Next(1, customer.Segment == CustomerSegment.Premium ? 50 : 20);
                customer.LifetimeValue = customer.TotalSpent * (_random.Next(120, 200) / 100m);

                await _customerRepository.AddAsync(customer);
            }
        }

        private async Task GenerateProductsAsync(int count)
        {
            Console.WriteLine($"Generating {count} products...");
            
            var categories = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Sports", "Toys", "Beauty", "Automotive" };
            var brands = new[] { "Samsung", "Apple", "Nike", "Adidas", "Sony", "Dell", "HP", "Canon", "Amazon", "Generic" };
            
            var productNames = new Dictionary<string, string[]>
            {
                ["Electronics"] = new[] { "Smartphone", "Laptop", "Tablet", "Headphones", "Smart Watch", "Camera", "TV", "Speaker" },
                ["Clothing"] = new[] { "T-Shirt", "Jeans", "Dress", "Jacket", "Shoes", "Hat", "Sweater", "Shorts" },
                ["Books"] = new[] { "Novel", "Textbook", "Biography", "Cookbook", "Self-Help", "History", "Science", "Fiction" },
                ["Home & Garden"] = new[] { "Plant Pot", "Garden Tool", "Furniture", "Decor", "Kitchen Appliance", "Bedding", "Lamp", "Rug" },
                ["Sports"] = new[] { "Running Shoes", "Yoga Mat", "Dumbbells", "Tennis Racket", "Basketball", "Bike", "Helmet", "Water Bottle" },
                ["Toys"] = new[] { "Action Figure", "Doll", "Board Game", "Puzzle", "Building Blocks", "Car", "Robot", "Ball" },
                ["Beauty"] = new[] { "Moisturizer", "Shampoo", "Lipstick", "Foundation", "Perfume", "Face Mask", "Serum", "Nail Polish" },
                ["Automotive"] = new[] { "Car Parts", "Oil", "Tire", "Battery", "GPS", "Car Cover", "Tools", "Air Freshener" }
            };

            for (int i = 1; i <= count; i++)
            {
                var category = categories[_random.Next(categories.Length)];
                var brand = brands[_random.Next(brands.Length)];
                var productType = productNames[category][_random.Next(productNames[category].Length)];
                
                var cost = _random.Next(10, 500);
                var markup = _random.Next(20, 100) / 100m;
                var basePrice = cost * (1 + markup);

                var product = new Product
                {
                    Name = $"{brand} {productType}",
                    Description = $"High-quality {productType.ToLower()} from {brand}",
                    Category = category,
                    Brand = brand,
                    SKU = $"{category.Substring(0, 3).ToUpper()}{i:D4}",
                    BasePrice = basePrice,
                    CurrentPrice = basePrice,
                    Cost = cost,
                    Stock = _random.Next(0, 200),
                    ReorderLevel = _random.Next(10, 30),
                    CreatedDate = DateTime.Now.AddDays(-_random.Next(30, 365)),
                    Weight = _random.Next(1, 50) / 10m,
                    Dimensions = $"{_random.Next(5, 30)}x{_random.Next(5, 30)}x{_random.Next(5, 30)} cm",
                    IsActive = true
                };

                // Generate some metrics
                product.Metrics.TotalSold = _random.Next(0, 1000);
                product.Metrics.TotalRevenue = product.Metrics.TotalSold * product.CurrentPrice * (_random.Next(80, 120) / 100m);
                product.Metrics.ViewCount = _random.Next(product.Metrics.TotalSold * 5, product.Metrics.TotalSold * 20);
                product.Metrics.ConversionRate = product.Metrics.ViewCount > 0 ? 
                    (decimal)product.Metrics.TotalSold / product.Metrics.ViewCount * 100 : 0;
                product.Metrics.AverageRating = _random.Next(25, 50) / 10m;
                product.Metrics.ReviewCount = _random.Next(0, product.Metrics.TotalSold / 5);
                product.Metrics.DemandScore = _random.Next(1, 100) / 10m;
                product.Metrics.CompetitorPrice = product.CurrentPrice * (_random.Next(90, 130) / 100m);
                product.Metrics.PriceElasticity = _random.Next(5, 25) / 10m;

                if (product.Metrics.TotalSold > 0)
                {
                    product.Metrics.LastSold = DateTime.Now.AddDays(-_random.Next(1, 30));
                }

                await _productRepository.AddAsync(product);
            }
        }

        private async Task GenerateHistoricalOrdersAsync(int count)
        {
            Console.WriteLine($"Generating {count} historical orders...");
            
            var customers = await _customerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();

            if (!customers.Any() || !products.Any())
            {
                Console.WriteLine("No customers or products found. Cannot generate orders.");
                return;
            }

            for (int i = 1; i <= count; i++)
            {
                var customer = customers[_random.Next(customers.Count)];
                var orderDate = DateTime.Now.AddDays(-_random.Next(1, 365));
                
                var itemCount = _random.Next(1, 5);
                var items = new List<OrderItem>();
                
                for (int j = 0; j < itemCount; j++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 4);
                    
                    // Apply some random discount
                    var discountPercent = customer.Segment switch
                    {
                        CustomerSegment.VIP => _random.Next(5, 15),
                        CustomerSegment.Premium => _random.Next(3, 10),
                        CustomerSegment.Regular => _random.Next(0, 5),
                        _ => 0
                    };

                    items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = quantity,
                        UnitPrice = product.CurrentPrice,
                        DiscountPercent = discountPercent
                    });
                }

                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = orderDate,
                    Items = items,
                    Status = GenerateOrderStatus(),
                    TaxAmount = items.Sum(i => i.LineTotal) * 0.08m,
                    ShippingCost = customer.IsVip ? 0 : (items.Sum(i => i.LineTotal) > 100 ? 0 : 5.99m),
                    DiscountAmount = items.Sum(i => i.LineTotal * i.DiscountPercent / 100),
                    ShippingAddress = $"{_random.Next(100, 9999)} Main St, City, State {_random.Next(10000, 99999)}",
                    BillingAddress = $"{_random.Next(100, 9999)} Main St, City, State {_random.Next(10000, 99999)}"
                };

                order.CalculateTotals();

                if (order.Status == OrderStatus.Shipped)
                {
                    order.ShippingDate = orderDate.AddDays(_random.Next(1, 3));
                }
                else if (order.Status == OrderStatus.Delivered)
                {
                    order.ShippingDate = orderDate.AddDays(_random.Next(1, 3));
                    order.DeliveryDate = order.ShippingDate.Value.AddDays(_random.Next(1, 7));
                }

                order.Payment = new PaymentInfo
                {
                    Method = new[] { "Credit Card", "PayPal", "Bank Transfer" }[_random.Next(3)],
                    TransactionId = Guid.NewGuid().ToString("N")[..10].ToUpper(),
                    PaymentDate = orderDate.AddHours(_random.Next(1, 24)),
                    Status = order.Status == OrderStatus.Cancelled ? PaymentStatus.Cancelled :
                            order.Status == OrderStatus.Returned ? PaymentStatus.Refunded : PaymentStatus.Completed
                };

                await _orderRepository.AddAsync(order);
            }
        }

        private CustomerSegment GenerateCustomerSegment()
        {
            var segments = Enum.GetValues<CustomerSegment>();
            var weights = new[] { 30, 40, 15, 10, 5 }; // New, Regular, VIP, Premium, Churned
            
            var totalWeight = weights.Sum();
            var randomValue = _random.Next(totalWeight);
            
            int currentWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue < currentWeight)
                {
                    return segments[i];
                }
            }
            
            return CustomerSegment.Regular;
        }

        private OrderStatus GenerateOrderStatus()
        {
            var statuses = Enum.GetValues<OrderStatus>();
            var weights = new[] { 5, 10, 5, 10, 60, 5, 5 }; // Distribution of order statuses
            
            var totalWeight = weights.Sum();
            var randomValue = _random.Next(totalWeight);
            
            int currentWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                currentWeight += weights[i];
                if (randomValue < currentWeight)
                {
                    return statuses[i];
                }
            }
            
            return OrderStatus.Delivered;
        }

        public async Task<bool> HasSampleDataAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            
            return customers.Any() && products.Any() && orders.Any();
        }

        public async Task ClearAllDataAsync()
        {
            Console.WriteLine("Clearing all data...");
            
            await _customerRepository.SaveAsync();
            await _productRepository.SaveAsync();
            await _orderRepository.SaveAsync();
            
            Console.WriteLine("All data cleared!");
        }
    }
}