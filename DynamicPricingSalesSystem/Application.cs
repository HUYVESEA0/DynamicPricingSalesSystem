using DynamicPricingSalesSystem.Core.Pricing;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Data.Repositories;
using DynamicPricingSalesSystem.Models;
using DynamicPricingSalesSystem.Services.Analytics;
using DynamicPricingSalesSystem.Services.Sales;
using DynamicPricingSalesSystem.UI.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem
{
    public class Application
    {
        private readonly JsonDataStorage _storage;
        private readonly CustomerRepository _customerRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;
        private readonly SalesService _salesService;
        private readonly AnalyticsService _analyticsService;
        private readonly PricingEngine _pricingEngine;
        private readonly SampleDataGenerator _sampleDataGenerator;

        public Application()
        {
            _storage = new JsonDataStorage("Data");
            _customerRepository = new CustomerRepository(_storage);
            _productRepository = new ProductRepository(_storage);
            _orderRepository = new OrderRepository(_storage);
            _salesService = new SalesService(_customerRepository, _productRepository, _orderRepository);
            _analyticsService = new AnalyticsService(_customerRepository, _productRepository, _orderRepository);
            _pricingEngine = new PricingEngine();
            _sampleDataGenerator = new SampleDataGenerator(_customerRepository, _productRepository, _orderRepository);
        }

        public async Task RunAsync()
        {
            await LoadDataAsync();
            
            while (true)
            {
                ConsoleUI.DisplayHeader();
                ConsoleUI.DisplayMainMenu();
                
                var choice = ConsoleUI.GetUserChoice(8);
                
                try
                {
                    switch (choice)
                    {
                        case 1:
                            await ShowAnalyticsDashboardAsync();
                            break;
                        case 2:
                            await ShowPricingEngineAsync();
                            break;
                        case 3:
                            await ShowSalesManagementAsync();
                            break;
                        case 4:
                            await ShowCustomerManagementAsync();
                            break;
                        case 5:
                            await ShowProductManagementAsync();
                            break;
                        case 6:
                            await ShowReportsAsync();
                            break;
                        case 7:
                            await ShowSystemSettingsAsync();
                            break;
                        case 8:
                            await GenerateSampleDataAsync();
                            break;
                        case 0:
                            ConsoleUI.DisplayInfoMessage("Thank you for using Dynamic Pricing + Sales Management System!");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUI.DisplayErrorMessage($"An error occurred: {ex.Message}");
                    ConsoleUI.WaitForKeyPress();
                }
            }
        }

        private async Task LoadDataAsync()
        {
            ConsoleUI.DisplayLoadingMessage("Loading data");
            
            await _customerRepository.LoadAsync();
            await _productRepository.LoadAsync();
            await _orderRepository.LoadAsync();
            
            ConsoleUI.DisplaySuccessMessage("Data loaded successfully");
        }

        private async Task ShowAnalyticsDashboardAsync()
        {
            ConsoleUI.DisplayHeader();
            ConsoleUI.DisplayLoadingMessage("Loading dashboard metrics");
            
            var metrics = await _analyticsService.GetDashboardMetricsAsync();
            
            System.Console.Clear();
            ConsoleUI.DisplayHeader();
            
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("═══════════════════════ ANALYTICS DASHBOARD ═══════════════════════");
            System.Console.ResetColor();
            System.Console.WriteLine();

            // Display key metrics in cards
            ConsoleUI.DisplayMetricCard("TOTAL REVENUE", $"${metrics.ThisMonthRevenue:N2}", 
                $"Growth: {metrics.RevenueGrowth:F1}%", ConsoleColor.Green);
            
            ConsoleUI.DisplayMetricCard("ORDERS TODAY", metrics.TodaysOrders.ToString(), 
                $"Revenue: ${metrics.TodaysRevenue:N2}", ConsoleColor.Cyan);
            
            ConsoleUI.DisplayMetricCard("TOTAL CUSTOMERS", metrics.TotalCustomers.ToString(), 
                $"Conversion: {metrics.ConversionRate:F1}%", ConsoleColor.Magenta);
            
            ConsoleUI.DisplayMetricCard("AVG ORDER VALUE", $"${metrics.AverageOrderValue:N2}", 
                $"Pending Orders: {metrics.PendingOrders}", ConsoleColor.Blue);

            // Display alerts
            if (metrics.LowStockAlerts > 0)
            {
                ConsoleUI.DisplayWarningMessage($"⚠️ {metrics.LowStockAlerts} products need restocking!");
            }

            // Display customer segments
            System.Console.WriteLine("\n📊 Customer Segment Distribution:");
            foreach (var segment in metrics.CustomerSegmentDistribution)
            {
                System.Console.WriteLine($"   {segment.Key}: {segment.Value} customers");
            }

            // Display top selling products
            if (metrics.TopSellingProducts.Any())
            {
                System.Console.WriteLine("\n🏆 Top Selling Products:");
                var headers = new List<string> { "Product", "Revenue", "Qty Sold" };
                var selectors = new List<Func<ProductSalesInfo, string>>
                {
                    p => p.ProductName.Length > 30 ? p.ProductName.Substring(0, 27) + "..." : p.ProductName,
                    p => $"${p.Revenue:N2}",
                    p => p.QuantitySold.ToString()
                };
                ConsoleUI.DisplayTable(metrics.TopSellingProducts.Take(5).ToList(), headers, selectors);
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowPricingEngineAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "🔍 Analyze Product Pricing",
                    "⚙️ Run Price Optimization",
                    "🧪 A/B Testing",
                    "📈 Pricing Strategy Configuration",
                    "📊 Price Performance Report"
                };
                
                ConsoleUI.DisplaySubMenu("Dynamic Pricing Engine", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await AnalyzeProductPricingAsync();
                        break;
                    case 2:
                        await RunPriceOptimizationAsync();
                        break;
                    case 3:
                        await RunABTestingAsync();
                        break;
                    case 4:
                        await ConfigurePricingStrategyAsync();
                        break;
                    case 5:
                        await ShowPricePerformanceReportAsync();
                        break;
                }
            }
        }

        private async Task AnalyzeProductPricingAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🔍 Product Pricing Analysis\n");
            
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
            {
                ConsoleUI.DisplayWarningMessage("No products found. Please add products first.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            System.Console.WriteLine("Select a product to analyze:");
            for (int i = 0; i < Math.Min(products.Count, 10); i++)
            {
                System.Console.WriteLine($"{i + 1}. {products[i].Name} - Current Price: ${products[i].CurrentPrice:F2}");
            }

            var productChoice = ConsoleUI.GetUserChoice(Math.Min(products.Count, 10));
            if (productChoice == 0) return;

            var selectedProduct = products[productChoice - 1];
            
            // Create pricing context
            var context = new PricingContext
            {
                DemandFactor = 1.2m, // Simulate high demand
                InventoryLevel = (decimal)selectedProduct.Stock / 100, // Simulate inventory level
                IsSeasonalPeriod = DateTime.Now.Month == 12, // Christmas season
                Season = DateTime.Now.Month == 12 ? "christmas" : "regular"
            };

            var result = _pricingEngine.CalculateOptimalPrice(selectedProduct, context);

            System.Console.WriteLine($"\n📊 Pricing Analysis for: {selectedProduct.Name}");
            System.Console.WriteLine($"Current Price: ${selectedProduct.CurrentPrice:F2}");
            System.Console.WriteLine($"Optimal Price: ${result.OptimalPrice:F2}");
            System.Console.WriteLine($"Price Change: ${result.PriceChange:F2} ({result.PriceChangePercent:F1}%)");
            System.Console.WriteLine($"Profit Margin: {selectedProduct.ProfitMargin:F1}%");
            
            System.Console.WriteLine("\n🎯 Strategy Breakdown:");
            foreach (var strategy in result.StrategyResults)
            {
                System.Console.WriteLine($"  {strategy.StrategyName}: ${strategy.Price:F2} (Weight: {strategy.Weight:P0})");
                System.Console.WriteLine($"    Reason: {strategy.Reason}");
            }

            if (ConsoleUI.ConfirmAction($"\nApply the optimal price of ${result.OptimalPrice:F2}?"))
            {
                await _productRepository.UpdatePriceAsync(selectedProduct.Id, result.OptimalPrice, "AI-optimized pricing");
                ConsoleUI.DisplaySuccessMessage("Price updated successfully!");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task RunPriceOptimizationAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("⚙️ Bulk Price Optimization\n");
            
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
            {
                ConsoleUI.DisplayWarningMessage("No products found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var category = ConsoleUI.GetUserInput("Enter category to optimize (or 'all' for all products)", false);
            var productsToOptimize = string.IsNullOrEmpty(category) || category.ToLower() == "all" 
                ? products 
                : products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!productsToOptimize.Any())
            {
                ConsoleUI.DisplayWarningMessage("No products found in the specified category.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            ConsoleUI.DisplayLoadingMessage($"Optimizing prices for {productsToOptimize.Count} products");

            var optimized = 0;
            foreach (var product in productsToOptimize)
            {
                var context = new PricingContext
                {
                    DemandFactor = (decimal)(0.8 + new Random().NextDouble() * 0.4), // Random demand factor
                    InventoryLevel = (decimal)product.Stock / 100
                };

                var result = _pricingEngine.CalculateOptimalPrice(product, context);
                
                if (Math.Abs(result.PriceChange) > 0.50m) // Only update if change is significant
                {
                    await _productRepository.UpdatePriceAsync(product.Id, result.OptimalPrice, "Bulk optimization");
                    optimized++;
                }
            }

            ConsoleUI.DisplaySuccessMessage($"Optimization completed! {optimized} prices updated.");
            ConsoleUI.WaitForKeyPress();
        }

        private async Task RunABTestingAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🧪 A/B Testing Framework\n");
            
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
            {
                ConsoleUI.DisplayWarningMessage("No products found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            System.Console.WriteLine("Select a product for A/B testing:");
            for (int i = 0; i < Math.Min(products.Count, 10); i++)
            {
                System.Console.WriteLine($"{i + 1}. {products[i].Name} - ${products[i].CurrentPrice:F2}");
            }

            var choice = ConsoleUI.GetUserChoice(Math.Min(products.Count, 10));
            if (choice == 0) return;

            var product = products[choice - 1];
            var priceA = ConsoleUI.GetDecimalInput($"Enter Price A (current: ${product.CurrentPrice:F2})", 0.01m);
            var priceB = ConsoleUI.GetDecimalInput("Enter Price B", 0.01m);
            var days = ConsoleUI.GetIntInput("Test duration in days", 1, 30);

            ConsoleUI.DisplayLoadingMessage("Running A/B test simulation");

            var context = new PricingContext();
            var testResult = _pricingEngine.RunABTest(product, context, priceA, priceB, days);

            System.Console.WriteLine($"\n🧪 A/B Test Results for: {product.Name}");
            System.Console.WriteLine($"Test Duration: {days} days");
            System.Console.WriteLine($"\nVariant A (${testResult.VariantA.Price:F2}):");
            System.Console.WriteLine($"  Views: {testResult.VariantA.Views:N0}");
            System.Console.WriteLine($"  Conversions: {testResult.VariantA.Conversions:N0}");
            System.Console.WriteLine($"  Conversion Rate: {testResult.VariantA.ConversionRate:F2}%");
            System.Console.WriteLine($"  Revenue: ${testResult.VariantA.Revenue:N2}");

            System.Console.WriteLine($"\nVariant B (${testResult.VariantB.Price:F2}):");
            System.Console.WriteLine($"  Views: {testResult.VariantB.Views:N0}");
            System.Console.WriteLine($"  Conversions: {testResult.VariantB.Conversions:N0}");
            System.Console.WriteLine($"  Conversion Rate: {testResult.VariantB.ConversionRate:F2}%");
            System.Console.WriteLine($"  Revenue: ${testResult.VariantB.Revenue:N2}");

            System.Console.WriteLine($"\n🏆 Winner: Variant {testResult.Winner} (Confidence: {testResult.Confidence:F1}%)");
            
            System.Console.WriteLine("\n💡 Recommendations:");
            foreach (var recommendation in testResult.Recommendations)
            {
                System.Console.WriteLine($"  • {recommendation}");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ConfigurePricingStrategyAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📈 Pricing Strategy Configuration\n");
            
            var strategies = _pricingEngine.GetAvailableStrategies();
            
            System.Console.WriteLine("Available Pricing Strategies:");
            for (int i = 0; i < strategies.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {strategies[i]}");
            }

            System.Console.WriteLine("\nCurrent strategy weights are balanced at 25% each.");
            System.Console.WriteLine("This configuration can be extended to allow custom weight adjustments.");
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowPricePerformanceReportAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 Price Performance Report\n");
            
            var products = await _productRepository.GetAllAsync();
            var topPerformers = products.OrderByDescending(p => p.Metrics.TotalRevenue).Take(10).ToList();

            if (topPerformers.Any())
            {
                var headers = new List<string> { "Product", "Current Price", "Revenue", "Margin %", "Sales" };
                var selectors = new List<Func<Product, string>>
                {
                    p => p.Name.Length > 25 ? p.Name.Substring(0, 22) + "..." : p.Name,
                    p => $"${p.CurrentPrice:F2}",
                    p => $"${p.Metrics.TotalRevenue:N0}",
                    p => $"{p.ProfitMargin:F1}%",
                    p => p.Metrics.TotalSold.ToString()
                };
                
                ConsoleUI.DisplayTable(topPerformers, headers, selectors);
            }
            else
            {
                ConsoleUI.DisplayWarningMessage("No performance data available.");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowSalesManagementAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "📋 Create New Order",
                    "📦 Process Pending Orders",
                    "🚚 Ship Orders",
                    "❌ Cancel Order",
                    "📊 View Order History",
                    "💰 Calculate Customer CLV"
                };
                
                ConsoleUI.DisplaySubMenu("Sales Management", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await CreateNewOrderAsync();
                        break;
                    case 2:
                        await ProcessPendingOrdersAsync();
                        break;
                    case 3:
                        await ShipOrdersAsync();
                        break;
                    case 4:
                        await CancelOrderAsync();
                        break;
                    case 5:
                        await ViewOrderHistoryAsync();
                        break;
                    case 6:
                        await CalculateCustomerCLVAsync();
                        break;
                }
            }
        }

        private async Task CreateNewOrderAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📋 Create New Order\n");
            
            var customers = await _customerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            
            if (!customers.Any() || !products.Any())
            {
                ConsoleUI.DisplayWarningMessage("Need customers and products to create orders.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            // Select customer
            System.Console.WriteLine("Select customer:");
            for (int i = 0; i < Math.Min(customers.Count, 10); i++)
            {
                System.Console.WriteLine($"{i + 1}. {customers[i].Name} ({customers[i].Segment})");
            }
            
            var customerChoice = ConsoleUI.GetUserChoice(Math.Min(customers.Count, 10));
            if (customerChoice == 0) return;
            
            var selectedCustomer = customers[customerChoice - 1];
            var orderItems = new List<OrderItem>();

            // Add products to order
            while (true)
            {
                System.Console.WriteLine("\nSelect product to add (or 0 to finish):");
                for (int i = 0; i < Math.Min(products.Count, 10); i++)
                {
                    System.Console.WriteLine($"{i + 1}. {products[i].Name} - ${products[i].CurrentPrice:F2} (Stock: {products[i].Stock})");
                }
                
                var productChoice = ConsoleUI.GetUserChoice(Math.Min(products.Count, 10));
                if (productChoice == 0) break;
                
                var selectedProduct = products[productChoice - 1];
                var quantity = ConsoleUI.GetIntInput($"Quantity (max {selectedProduct.Stock})", 1, selectedProduct.Stock);
                
                orderItems.Add(new OrderItem
                {
                    ProductId = selectedProduct.Id,
                    ProductName = selectedProduct.Name,
                    Quantity = quantity,
                    UnitPrice = selectedProduct.CurrentPrice
                });
                
                System.Console.WriteLine($"Added {quantity}x {selectedProduct.Name} to order");
            }

            if (!orderItems.Any())
            {
                ConsoleUI.DisplayWarningMessage("No items added to order.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            try
            {
                var order = await _salesService.CreateOrderAsync(selectedCustomer.Id, orderItems);
                ConsoleUI.DisplaySuccessMessage($"Order #{order.Id} created successfully! Total: ${order.TotalAmount:F2}");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Failed to create order: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ProcessPendingOrdersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📦 Process Pending Orders\n");
            
            var pendingOrders = await _orderRepository.GetPendingOrdersAsync();
            
            if (!pendingOrders.Any())
            {
                ConsoleUI.DisplayInfoMessage("No pending orders to process.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            System.Console.WriteLine($"Found {pendingOrders.Count} pending orders:");
            
            var headers = new List<string> { "Order ID", "Customer", "Total", "Items", "Date" };
            var selectors = new List<Func<Order, string>>
            {
                o => o.Id.ToString(),
                o => o.CustomerId.ToString(),
                o => $"${o.TotalAmount:F2}",
                o => o.Items.Count.ToString(),
                o => o.OrderDate.ToString("MM/dd/yyyy")
            };
            
            ConsoleUI.DisplayTable(pendingOrders.Take(10).ToList(), headers, selectors);

            if (ConsoleUI.ConfirmAction("Process all pending orders?"))
            {
                var processed = 0;
                foreach (var order in pendingOrders)
                {
                    if (await _salesService.ProcessOrderAsync(order.Id))
                    {
                        processed++;
                    }
                }
                
                ConsoleUI.DisplaySuccessMessage($"Processed {processed} orders successfully!");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShipOrdersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🚚 Ship Orders\n");
            
            var confirmedOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Confirmed);
            
            if (!confirmedOrders.Any())
            {
                ConsoleUI.DisplayInfoMessage("No confirmed orders ready for shipping.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "Order ID", "Customer", "Total", "Date" };
            var selectors = new List<Func<Order, string>>
            {
                o => o.Id.ToString(),
                o => o.CustomerId.ToString(),
                o => $"${o.TotalAmount:F2}",
                o => o.OrderDate.ToString("MM/dd/yyyy")
            };
            
            ConsoleUI.DisplayTable(confirmedOrders.Take(10).ToList(), headers, selectors);

            var orderId = ConsoleUI.GetIntInput("Enter Order ID to ship (0 to cancel)");
            if (orderId == 0) return;

            if (await _salesService.ShipOrderAsync(orderId))
            {
                ConsoleUI.DisplaySuccessMessage($"Order #{orderId} shipped successfully!");
            }
            else
            {
                ConsoleUI.DisplayErrorMessage("Failed to ship order. Please check the order status.");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task CancelOrderAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("❌ Cancel Order\n");
            
            var orderId = ConsoleUI.GetIntInput("Enter Order ID to cancel");
            var reason = ConsoleUI.GetUserInput("Cancellation reason (optional)", false);

            if (await _salesService.CancelOrderAsync(orderId, reason))
            {
                ConsoleUI.DisplaySuccessMessage($"Order #{orderId} cancelled successfully!");
            }
            else
            {
                ConsoleUI.DisplayErrorMessage("Failed to cancel order. Order may not exist or cannot be cancelled.");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ViewOrderHistoryAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 Order History\n");
            
            var orders = await _orderRepository.GetRecentOrdersAsync(20);
            
            if (!orders.Any())
            {
                ConsoleUI.DisplayInfoMessage("No orders found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "ID", "Customer", "Status", "Total", "Date" };
            var selectors = new List<Func<Order, string>>
            {
                o => o.Id.ToString(),
                o => o.CustomerId.ToString(),
                o => o.Status.ToString(),
                o => $"${o.TotalAmount:F2}",
                o => o.OrderDate.ToString("MM/dd/yyyy")
            };
            
            ConsoleUI.DisplayTable(orders, headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task CalculateCustomerCLVAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("💰 Calculate Customer Lifetime Value\n");
            
            var customerId = ConsoleUI.GetIntInput("Enter Customer ID");
            
            try
            {
                var clv = await _salesService.CalculateCustomerLifetimeValueAsync(customerId);
                
                System.Console.WriteLine($"\n💰 Customer Lifetime Value Analysis:");
                System.Console.WriteLine($"Customer ID: {clv.CustomerId}");
                System.Console.WriteLine($"Lifetime Value: ${clv.Value:N2}");
                System.Console.WriteLine($"Average Order Value: ${clv.AverageOrderValue:N2}");
                System.Console.WriteLine($"Order Frequency: {clv.OrderFrequency:F2} orders/year");
                System.Console.WriteLine($"Customer Lifespan: {clv.CustomerLifespan:F1} years");
                System.Console.WriteLine($"Total Orders: {clv.TotalOrders}");
                System.Console.WriteLine($"Total Spent: ${clv.TotalSpent:N2}");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Error calculating CLV: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowCustomerManagementAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "👥 View All Customers",
                    "➕ Add New Customer",
                    "🔍 Search Customers",
                    "📊 Customer Analytics",
                    "🏆 Top Customers"
                };
                
                ConsoleUI.DisplaySubMenu("Customer Management", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await ViewAllCustomersAsync();
                        break;
                    case 2:
                        await AddNewCustomerAsync();
                        break;
                    case 3:
                        await SearchCustomersAsync();
                        break;
                    case 4:
                        await ShowCustomerAnalyticsAsync();
                        break;
                    case 5:
                        await ShowTopCustomersAsync();
                        break;
                }
            }
        }

        private async Task ViewAllCustomersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("👥 All Customers\n");
            
            var customers = await _customerRepository.GetAllAsync();
            
            if (!customers.Any())
            {
                ConsoleUI.DisplayInfoMessage("No customers found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "ID", "Name", "Email", "Segment", "Total Spent", "Orders" };
            var selectors = new List<Func<Customer, string>>
            {
                c => c.Id.ToString(),
                c => c.Name.Length > 20 ? c.Name.Substring(0, 17) + "..." : c.Name,
                c => c.Email.Length > 25 ? c.Email.Substring(0, 22) + "..." : c.Email,
                c => c.Segment.ToString(),
                c => $"${c.TotalSpent:N0}",
                c => c.TotalOrders.ToString()
            };
            
            ConsoleUI.DisplayTable(customers.Take(20).ToList(), headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task AddNewCustomerAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("➕ Add New Customer\n");
            
            var name = ConsoleUI.GetUserInput("Customer Name");
            var email = ConsoleUI.GetUserInput("Email Address");
            var phone = ConsoleUI.GetUserInput("Phone Number", false);
            
            var customer = new Customer
            {
                Name = name,
                Email = email,
                Phone = phone,
                Segment = CustomerSegment.New,
                RegistrationDate = DateTime.Now,
                LastPurchaseDate = DateTime.Now
            };

            try
            {
                await _customerRepository.AddAsync(customer);
                ConsoleUI.DisplaySuccessMessage($"Customer '{name}' added successfully!");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Failed to add customer: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task SearchCustomersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🔍 Search Customers\n");
            
            var searchTerm = ConsoleUI.GetUserInput("Enter search term (name, email, or phone)");
            var customers = await _customerRepository.SearchAsync(searchTerm);
            
            if (!customers.Any())
            {
                ConsoleUI.DisplayInfoMessage("No customers found matching the search criteria.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "ID", "Name", "Email", "Segment", "Total Spent" };
            var selectors = new List<Func<Customer, string>>
            {
                c => c.Id.ToString(),
                c => c.Name,
                c => c.Email,
                c => c.Segment.ToString(),
                c => $"${c.TotalSpent:N0}"
            };
            
            ConsoleUI.DisplayTable(customers, headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowCustomerAnalyticsAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 Customer Analytics\n");
            
            ConsoleUI.DisplayLoadingMessage("Analyzing customer data");
            var analytics = await _analyticsService.GetCustomerAnalyticsAsync();
            
            ConsoleUI.DisplayMetricCard("Total Customers", analytics.TotalCustomers.ToString());
            ConsoleUI.DisplayMetricCard("Active Customers", analytics.ActiveCustomers.ToString(),
                $"Retention Rate: {analytics.CustomerRetentionRate:F1}%");
            ConsoleUI.DisplayMetricCard("New This Month", analytics.NewCustomersThisMonth.ToString());
            ConsoleUI.DisplayMetricCard("Average CLV", $"${analytics.AverageLifetimeValue:N2}");

            System.Console.WriteLine("📈 Customer Acquisition Trend (Last 12 Months):");
            foreach (var trend in analytics.CustomerAcquisitionTrend.TakeLast(6))
            {
                System.Console.WriteLine($"   {trend.Key}: {trend.Value} new customers");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowTopCustomersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🏆 Top Customers\n");
            
            var topCustomers = await _customerRepository.GetTopCustomersAsync(10);
            
            if (!topCustomers.Any())
            {
                ConsoleUI.DisplayInfoMessage("No customer data available.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "Rank", "Name", "Segment", "Total Spent", "Orders", "CLV" };
            var selectors = new List<Func<Customer, string>>
            {
                c => (topCustomers.IndexOf(c) + 1).ToString(),
                c => c.Name.Length > 20 ? c.Name.Substring(0, 17) + "..." : c.Name,
                c => c.Segment.ToString(),
                c => $"${c.TotalSpent:N0}",
                c => c.TotalOrders.ToString(),
                c => $"${c.LifetimeValue:N0}"
            };
            
            ConsoleUI.DisplayTable(topCustomers, headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowProductManagementAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "📦 View All Products",
                    "➕ Add New Product",
                    "🔍 Search Products",
                    "💰 Update Prices",
                    "📊 Inventory Status",
                    "🏆 Top Performers"
                };
                
                ConsoleUI.DisplaySubMenu("Product Management", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await ViewAllProductsAsync();
                        break;
                    case 2:
                        await AddNewProductAsync();
                        break;
                    case 3:
                        await SearchProductsAsync();
                        break;
                    case 4:
                        await UpdateProductPricesAsync();
                        break;
                    case 5:
                        await ShowInventoryStatusAsync();
                        break;
                    case 6:
                        await ShowTopPerformersAsync();
                        break;
                }
            }
        }

        private async Task ViewAllProductsAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📦 All Products\n");
            
            var products = await _productRepository.GetAllAsync();
            
            if (!products.Any())
            {
                ConsoleUI.DisplayInfoMessage("No products found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "ID", "Name", "Category", "Price", "Stock", "Sold" };
            var selectors = new List<Func<Product, string>>
            {
                p => p.Id.ToString(),
                p => p.Name.Length > 25 ? p.Name.Substring(0, 22) + "..." : p.Name,
                p => p.Category,
                p => $"${p.CurrentPrice:F2}",
                p => p.Stock.ToString(),
                p => p.Metrics.TotalSold.ToString()
            };
            
            ConsoleUI.DisplayTable(products.Take(20).ToList(), headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task AddNewProductAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("➕ Add New Product\n");
            
            var name = ConsoleUI.GetUserInput("Product Name");
            var description = ConsoleUI.GetUserInput("Description", false);
            var category = ConsoleUI.GetUserInput("Category");
            var brand = ConsoleUI.GetUserInput("Brand");
            var cost = ConsoleUI.GetDecimalInput("Cost", 0.01m);
            var price = ConsoleUI.GetDecimalInput("Selling Price", cost);
            var stock = ConsoleUI.GetIntInput("Initial Stock", 0);
            
            var product = new Product
            {
                Name = name,
                Description = description,
                Category = category,
                Brand = brand,
                SKU = $"{category.Substring(0, Math.Min(3, category.Length)).ToUpper()}{DateTime.Now.Ticks % 10000:D4}",
                BasePrice = price,
                CurrentPrice = price,
                Cost = cost,
                Stock = stock,
                ReorderLevel = Math.Max(10, stock / 10),
                IsActive = true
            };

            try
            {
                await _productRepository.AddAsync(product);
                ConsoleUI.DisplaySuccessMessage($"Product '{name}' added successfully!");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Failed to add product: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task SearchProductsAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🔍 Search Products\n");
            
            var searchTerm = ConsoleUI.GetUserInput("Enter search term (name, category, brand, or SKU)");
            var products = await _productRepository.SearchAsync(searchTerm);
            
            if (!products.Any())
            {
                ConsoleUI.DisplayInfoMessage("No products found matching the search criteria.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "ID", "Name", "Category", "Price", "Stock" };
            var selectors = new List<Func<Product, string>>
            {
                p => p.Id.ToString(),
                p => p.Name.Length > 30 ? p.Name.Substring(0, 27) + "..." : p.Name,
                p => p.Category,
                p => $"${p.CurrentPrice:F2}",
                p => p.Stock.ToString()
            };
            
            ConsoleUI.DisplayTable(products, headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task UpdateProductPricesAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("💰 Update Product Prices\n");
            
            var productId = ConsoleUI.GetIntInput("Enter Product ID");
            var product = await _productRepository.GetByIdAsync(productId);
            
            if (product == null)
            {
                ConsoleUI.DisplayErrorMessage("Product not found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            System.Console.WriteLine($"Current product: {product.Name}");
            System.Console.WriteLine($"Current price: ${product.CurrentPrice:F2}");
            System.Console.WriteLine($"Cost: ${product.Cost:F2}");
            System.Console.WriteLine($"Current margin: {product.ProfitMargin:F1}%");
            
            var newPrice = ConsoleUI.GetDecimalInput($"Enter new price (min ${product.Cost * 1.01m:F2})", product.Cost * 1.01m);
            var reason = ConsoleUI.GetUserInput("Reason for price change", false);
            
            if (await _productRepository.UpdatePriceAsync(productId, newPrice, reason))
            {
                var newMargin = ((newPrice - product.Cost) / newPrice) * 100;
                ConsoleUI.DisplaySuccessMessage($"Price updated! New margin: {newMargin:F1}%");
            }
            else
            {
                ConsoleUI.DisplayErrorMessage("Failed to update price.");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowInventoryStatusAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 Inventory Status\n");
            
            var allProducts = await _productRepository.GetAllAsync();
            var lowStockProducts = await _productRepository.GetLowStockProductsAsync();
            
            ConsoleUI.DisplayMetricCard("Total Products", allProducts.Count.ToString());
            ConsoleUI.DisplayMetricCard("Low Stock Alerts", lowStockProducts.Count.ToString(), 
                lowStockProducts.Count > 0 ? "⚠️ Needs Attention" : "✅ All Good");
            
            var totalValue = allProducts.Sum(p => p.InventoryValue);
            ConsoleUI.DisplayMetricCard("Total Inventory Value", $"${totalValue:N2}");

            if (lowStockProducts.Any())
            {
                System.Console.WriteLine("\n⚠️ Low Stock Products:");
                var headers = new List<string> { "Name", "Current Stock", "Reorder Level", "Value" };
                var selectors = new List<Func<Product, string>>
                {
                    p => p.Name.Length > 30 ? p.Name.Substring(0, 27) + "..." : p.Name,
                    p => p.Stock.ToString(),
                    p => p.ReorderLevel.ToString(),
                    p => $"${p.InventoryValue:N0}"
                };
                
                ConsoleUI.DisplayTable(lowStockProducts.Take(10).ToList(), headers, selectors);
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowTopPerformersAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🏆 Top Performing Products\n");
            
            var topProducts = await _productRepository.GetTopSellingProductsAsync(15);
            
            if (!topProducts.Any())
            {
                ConsoleUI.DisplayInfoMessage("No sales data available.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            var headers = new List<string> { "Rank", "Name", "Revenue", "Units Sold", "Margin %" };
            var selectors = new List<Func<Product, string>>
            {
                p => (topProducts.IndexOf(p) + 1).ToString(),
                p => p.Name.Length > 25 ? p.Name.Substring(0, 22) + "..." : p.Name,
                p => $"${p.Metrics.TotalRevenue:N0}",
                p => p.Metrics.TotalSold.ToString(),
                p => $"{p.ProfitMargin:F1}%"
            };
            
            ConsoleUI.DisplayTable(topProducts, headers, selectors);
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowReportsAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "📈 Sales Report",
                    "👥 Customer Analytics Report",
                    "📦 Product Analytics Report",
                    "🏢 Competitive Analysis",
                    "📊 Custom Date Range Report"
                };
                
                ConsoleUI.DisplaySubMenu("Reports & Analytics", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await ShowSalesReportAsync();
                        break;
                    case 2:
                        await ShowCustomerAnalyticsReportAsync();
                        break;
                    case 3:
                        await ShowProductAnalyticsReportAsync();
                        break;
                    case 4:
                        await ShowCompetitiveAnalysisAsync();
                        break;
                    case 5:
                        await ShowCustomDateRangeReportAsync();
                        break;
                }
            }
        }

        private async Task ShowSalesReportAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📈 Sales Report\n");
            
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            
            ConsoleUI.DisplayLoadingMessage("Generating sales report");
            var report = await _salesService.GenerateSalesReportAsync(startDate, endDate);
            
            System.Console.WriteLine($"📊 Sales Report ({startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy})");
            System.Console.WriteLine($"Total Orders: {report.TotalOrders}");
            System.Console.WriteLine($"Completed Orders: {report.CompletedOrders}");
            System.Console.WriteLine($"Total Revenue: ${report.TotalRevenue:N2}");
            System.Console.WriteLine($"Average Order Value: ${report.AverageOrderValue:N2}");
            System.Console.WriteLine($"Completion Rate: {(report.TotalOrders > 0 ? (double)report.CompletedOrders / report.TotalOrders * 100 : 0):F1}%");

            if (report.TopProducts.Any())
            {
                System.Console.WriteLine("\n🏆 Top Products This Period:");
                var headers = new List<string> { "Product", "Revenue", "Qty Sold" };
                var selectors = new List<Func<ProductSalesInfo, string>>
                {
                    p => p.ProductName.Length > 30 ? p.ProductName.Substring(0, 27) + "..." : p.ProductName,
                    p => $"${p.Revenue:N2}",
                    p => p.QuantitySold.ToString()
                };
                
                ConsoleUI.DisplayTable(report.TopProducts, headers, selectors);
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowCustomerAnalyticsReportAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("👥 Customer Analytics Report\n");
            
            ConsoleUI.DisplayLoadingMessage("Analyzing customer data");
            var analytics = await _analyticsService.GetCustomerAnalyticsAsync();
            
            System.Console.WriteLine("📊 Customer Overview:");
            System.Console.WriteLine($"Total Customers: {analytics.TotalCustomers:N0}");
            System.Console.WriteLine($"Active Customers: {analytics.ActiveCustomers:N0}");
            System.Console.WriteLine($"New This Month: {analytics.NewCustomersThisMonth:N0}");
            System.Console.WriteLine($"Churned Customers: {analytics.ChurnedCustomers:N0}");
            System.Console.WriteLine($"Retention Rate: {analytics.CustomerRetentionRate:F1}%");
            System.Console.WriteLine($"Average CLV: ${analytics.AverageLifetimeValue:N2}");

            System.Console.WriteLine("\n📈 Segment Performance:");
            foreach (var segment in analytics.SegmentPerformance)
            {
                var data = (dynamic)segment.Value;
                System.Console.WriteLine($"{segment.Key}: {data.CustomerCount} customers, ${data.Revenue:N0} revenue");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowProductAnalyticsReportAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📦 Product Analytics Report\n");
            
            ConsoleUI.DisplayLoadingMessage("Analyzing product data");
            var analytics = await _analyticsService.GetProductAnalyticsAsync();
            
            System.Console.WriteLine("📊 Product Overview:");
            System.Console.WriteLine($"Total Products: {analytics.TotalProducts:N0}");
            System.Console.WriteLine($"Active Products: {analytics.ActiveProducts:N0}");
            System.Console.WriteLine($"Out of Stock: {analytics.OutOfStockProducts:N0}");
            System.Console.WriteLine($"Low Stock: {analytics.LowStockProducts:N0}");
            System.Console.WriteLine($"Inventory Value: ${analytics.TotalInventoryValue:N2}");

            System.Console.WriteLine("\n📈 Category Performance:");
            foreach (var category in analytics.CategoryPerformance.Take(5))
            {
                var data = (dynamic)category.Value;
                System.Console.WriteLine($"{category.Key}: {data.ProductCount} products, ${data.Revenue:N0} revenue");
            }

            if (analytics.PriceAnalysis.ContainsKey("AveragePrice"))
            {
                System.Console.WriteLine($"\n💰 Price Analysis:");
                System.Console.WriteLine($"Average Price: ${analytics.PriceAnalysis["AveragePrice"]}");
                System.Console.WriteLine($"Price Range: {analytics.PriceAnalysis["PriceRange"]}");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowCompetitiveAnalysisAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🏢 Competitive Analysis\n");
            
            ConsoleUI.DisplayLoadingMessage("Analyzing competitive landscape");
            var analysis = await _analyticsService.GetCompetitiveAnalysisAsync();
            
            System.Console.WriteLine("🎯 Competitive Intelligence:");
            System.Console.WriteLine($"Products Tracked: {analysis.TotalProductsTracked:N0}");
            System.Console.WriteLine($"Price Advantage: {analysis.CompetitivePriceAdvantage:F1}%");
            System.Console.WriteLine($"Market Share Estimate: {analysis.MarketShareEstimate:F1}%");

            System.Console.WriteLine("\n📊 Price Positioning:");
            foreach (var position in analysis.PricePositioning)
            {
                System.Console.WriteLine($"{position.Key}: {position.Value} of products");
            }

            System.Console.WriteLine("\n💡 Market Insights:");
            foreach (var insight in analysis.CompetitorInsights)
            {
                System.Console.WriteLine($"• {insight}");
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowCustomDateRangeReportAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 Custom Date Range Report\n");
            
            System.Console.WriteLine("Enter date range for the report:");
            
            // For simplicity, using predefined ranges
            System.Console.WriteLine("1. Last 7 days");
            System.Console.WriteLine("2. Last 30 days");
            System.Console.WriteLine("3. Last 90 days");
            System.Console.WriteLine("4. Last 365 days");
            
            var choice = ConsoleUI.GetUserChoice(4);
            if (choice == 0) return;
            
            var endDate = DateTime.Now;
            var startDate = choice switch
            {
                1 => endDate.AddDays(-7),
                2 => endDate.AddDays(-30),
                3 => endDate.AddDays(-90),
                4 => endDate.AddDays(-365),
                _ => endDate.AddDays(-30)
            };

            ConsoleUI.DisplayLoadingMessage("Generating custom report");
            var report = await _salesService.GenerateSalesReportAsync(startDate, endDate);
            
            System.Console.WriteLine($"\n📊 Custom Report ({startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy})");
            System.Console.WriteLine($"Period: {(endDate - startDate).Days} days");
            System.Console.WriteLine($"Total Orders: {report.TotalOrders}");
            System.Console.WriteLine($"Revenue: ${report.TotalRevenue:N2}");
            System.Console.WriteLine($"Daily Average: ${report.TotalRevenue / (endDate - startDate).Days:N2}");
            System.Console.WriteLine($"Average Order Value: ${report.AverageOrderValue:N2}");

            if (report.DailySales.Any())
            {
                System.Console.WriteLine("\n📈 Daily Sales Trend:");
                foreach (var day in report.DailySales.TakeLast(7))
                {
                    System.Console.WriteLine($"  {day.Key}: ${day.Value:N2}");
                }
            }

            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowSystemSettingsAsync()
        {
            while (true)
            {
                ConsoleUI.DisplayHeader();
                var options = new List<string>
                {
                    "💾 Backup Data",
                    "📂 View Backup History",
                    "🔄 Reset Sample Data",
                    "📊 System Statistics",
                    "⚙️ Configuration"
                };
                
                ConsoleUI.DisplaySubMenu("System Settings", options);
                var choice = ConsoleUI.GetUserChoice(options.Count);
                
                if (choice == 0) break;
                
                switch (choice)
                {
                    case 1:
                        await BackupDataAsync();
                        break;
                    case 2:
                        await ViewBackupHistoryAsync();
                        break;
                    case 3:
                        await ResetSampleDataAsync();
                        break;
                    case 4:
                        await ShowSystemStatisticsAsync();
                        break;
                    case 5:
                        await ShowConfigurationAsync();
                        break;
                }
            }
        }

        private async Task BackupDataAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("💾 Backup Data\n");
            
            try
            {
                ConsoleUI.DisplayLoadingMessage("Creating backup");
                await _storage.BackupDataAsync();
                ConsoleUI.DisplaySuccessMessage("Data backup completed successfully!");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Backup failed: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ViewBackupHistoryAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📂 Backup History\n");
            
            var backups = _storage.GetAvailableBackups();
            
            if (!backups.Any())
            {
                ConsoleUI.DisplayInfoMessage("No backups found.");
                ConsoleUI.WaitForKeyPress();
                return;
            }

            System.Console.WriteLine("Available backups:");
            foreach (var backup in backups)
            {
                var backupName = System.IO.Path.GetFileName(backup);
                System.Console.WriteLine($"• {backupName}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ResetSampleDataAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🔄 Reset Sample Data\n");
            
            if (!ConsoleUI.ConfirmAction("This will delete all existing data and generate new sample data. Continue?"))
            {
                return;
            }

            try
            {
                ConsoleUI.DisplayLoadingMessage("Clearing existing data");
                await _sampleDataGenerator.ClearAllDataAsync();
                
                ConsoleUI.DisplayLoadingMessage("Generating new sample data");
                await _sampleDataGenerator.GenerateAllSampleDataAsync();
                
                // Reload data
                await LoadDataAsync();
                
                ConsoleUI.DisplaySuccessMessage("Sample data reset completed!");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Reset failed: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task ShowSystemStatisticsAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("📊 System Statistics\n");
            
            var customers = await _customerRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var orders = await _orderRepository.GetAllAsync();
            
            ConsoleUI.DisplayMetricCard("Database Statistics", "");
            System.Console.WriteLine($"Customers: {customers.Count:N0}");
            System.Console.WriteLine($"Products: {products.Count:N0}");
            System.Console.WriteLine($"Orders: {orders.Count:N0}");
            System.Console.WriteLine($"Total Revenue: ${orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount):N2}");
            
            var dataSize = CalculateDataSize();
            System.Console.WriteLine($"Estimated Data Size: {dataSize:F2} MB");
            
            ConsoleUI.WaitForKeyPress();
        }

        private decimal CalculateDataSize()
        {
            // Simplified data size calculation
            return 2.5m; // Placeholder for actual calculation
        }

        private async Task ShowConfigurationAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("⚙️ System Configuration\n");
            
            System.Console.WriteLine("Current Configuration:");
            System.Console.WriteLine($"Data Directory: Data/");
            System.Console.WriteLine($"Backup Directory: Data/Backups/");
            System.Console.WriteLine($"Tax Rate: 8.0%");
            System.Console.WriteLine($"Free Shipping Threshold: $100.00");
            System.Console.WriteLine($"VIP Free Shipping: Enabled");
            System.Console.WriteLine($"Pricing Engine: Multi-Strategy Weighted");
            System.Console.WriteLine($"A/B Testing: Enabled");
            System.Console.WriteLine($"Analytics: Real-time");
            
            System.Console.WriteLine("\nThis is a read-only view. Configuration changes would be implemented in a full system.");
            
            ConsoleUI.WaitForKeyPress();
        }

        private async Task GenerateSampleDataAsync()
        {
            ConsoleUI.DisplayHeader();
            System.Console.WriteLine("🔄 Generate Sample Data\n");
            
            var hasSampleData = await _sampleDataGenerator.HasSampleDataAsync();
            
            if (hasSampleData)
            {
                ConsoleUI.DisplayWarningMessage("Sample data already exists.");
                if (!ConsoleUI.ConfirmAction("Regenerate sample data? This will replace existing data."))
                {
                    return;
                }
            }

            try
            {
                await _sampleDataGenerator.GenerateAllSampleDataAsync();
                await LoadDataAsync(); // Reload data after generation
                ConsoleUI.DisplaySuccessMessage("Sample data generated successfully!");
            }
            catch (Exception ex)
            {
                ConsoleUI.DisplayErrorMessage($"Failed to generate sample data: {ex.Message}");
            }
            
            ConsoleUI.WaitForKeyPress();
        }
    }
}