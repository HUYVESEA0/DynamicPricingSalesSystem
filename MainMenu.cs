using System;
using System.Linq;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Engines;
using DynamicPricingSalesSystem.Models;
using DynamicPricingSalesSystem.Services;
using DynamicPricingSalesSystem.Utils;

namespace DynamicPricingSalesSystem
{
    public class MainMenu
    {
        private readonly JsonDataManager _dataManager;
        private readonly ProductService _productService;
        private readonly CustomerService _customerService;
        private readonly OrderService _orderService;
        private readonly DynamicPricingEngine _pricingEngine;
        private readonly SampleDataGenerator _sampleDataGenerator;

        public MainMenu()
        {
            _dataManager = new JsonDataManager();
            _productService = new ProductService(_dataManager);
            _customerService = new CustomerService(_dataManager);
            _orderService = new OrderService(_dataManager, _productService, _customerService);
            _pricingEngine = new DynamicPricingEngine(_productService, _orderService);
            _sampleDataGenerator = new SampleDataGenerator();
            
            InitializeSampleData();
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    ShowMainMenu();
                    var choice = ConsoleHelper.GetInput("Enter your choice");
                    
                    switch (choice.ToLower())
                    {
                        case "1":
                            ShowProductMenu();
                            break;
                        case "2":
                            ShowCustomerMenu();
                            break;
                        case "3":
                            ShowOrderMenu();
                            break;
                        case "4":
                            ShowPricingMenu();
                            break;
                        case "5":
                            ShowAnalyticsMenu();
                            break;
                        case "6":
                            ShowDataMenu();
                            break;
                        case "7":
                            ShowDashboard();
                            break;
                        case "q":
                        case "quit":
                        case "exit":
                            ConsoleHelper.WriteSuccess("Thank you for using Dynamic Pricing Sales System!");
                            return;
                        default:
                            ConsoleHelper.WriteError("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
                    ConsoleHelper.PressAnyKeyToContinue();
                }
            }
        }

        private void ShowMainMenu()
        {
            ConsoleHelper.WriteTitle("Dynamic Pricing Sales System");
            
            Console.WriteLine("1. Product Management");
            Console.WriteLine("2. Customer Management");
            Console.WriteLine("3. Order Management");
            Console.WriteLine("4. Dynamic Pricing");
            Console.WriteLine("5. Analytics & Reports");
            Console.WriteLine("6. Data Management");
            Console.WriteLine("7. Dashboard");
            Console.WriteLine("Q. Quit");
            Console.WriteLine();
        }

        private void ShowProductMenu()
        {
            while (true)
            {
                ConsoleHelper.WriteTitle("Product Management");
                
                Console.WriteLine("1. View All Products");
                Console.WriteLine("2. Search Products");
                Console.WriteLine("3. Add Product");
                Console.WriteLine("4. Update Product");
                Console.WriteLine("5. Delete Product");
                Console.WriteLine("6. Update Stock");
                Console.WriteLine("7. Low Stock Report");
                Console.WriteLine("8. Top Selling Products");
                Console.WriteLine("9. Back to Main Menu");
                Console.WriteLine();

                var choice = ConsoleHelper.GetInput("Enter your choice");
                
                switch (choice)
                {
                    case "1":
                        ViewAllProducts();
                        break;
                    case "2":
                        SearchProducts();
                        break;
                    case "3":
                        AddProduct();
                        break;
                    case "4":
                        UpdateProduct();
                        break;
                    case "5":
                        DeleteProduct();
                        break;
                    case "6":
                        UpdateStock();
                        break;
                    case "7":
                        ShowLowStockReport();
                        break;
                    case "8":
                        ShowTopSellingProducts();
                        break;
                    case "9":
                        return;
                    default:
                        ConsoleHelper.WriteError("Invalid choice. Please try again.");
                        break;
                }
                
                if (choice != "9")
                    ConsoleHelper.PressAnyKeyToContinue();
            }
        }

        private void ShowPricingMenu()
        {
            while (true)
            {
                ConsoleHelper.WriteTitle("Dynamic Pricing Engine");
                
                Console.WriteLine("1. Analyze All Product Prices");
                Console.WriteLine("2. Get Pricing Recommendation");
                Console.WriteLine("3. Update All Prices (Preview)");
                Console.WriteLine("4. Update All Prices (Apply)");
                Console.WriteLine("5. Manual Price Update");
                Console.WriteLine("6. View Price History");
                Console.WriteLine("7. Pricing Rules");
                Console.WriteLine("8. Back to Main Menu");
                Console.WriteLine();

                var choice = ConsoleHelper.GetInput("Enter your choice");
                
                switch (choice)
                {
                    case "1":
                        AnalyzeAllPrices();
                        break;
                    case "2":
                        GetPricingRecommendation();
                        break;
                    case "3":
                        UpdateAllPrices(false);
                        break;
                    case "4":
                        UpdateAllPrices(true);
                        break;
                    case "5":
                        ManualPriceUpdate();
                        break;
                    case "6":
                        ViewPriceHistory();
                        break;
                    case "7":
                        ShowPricingRules();
                        break;
                    case "8":
                        return;
                    default:
                        ConsoleHelper.WriteError("Invalid choice. Please try again.");
                        break;
                }
                
                if (choice != "8")
                    ConsoleHelper.PressAnyKeyToContinue();
            }
        }

        private void ViewAllProducts()
        {
            ConsoleHelper.WriteHeader("All Products");
            var products = _productService.GetAllProducts();
            
            ConsoleHelper.DisplayTable(products,
                ("ID", p => p.Id),
                ("Name", p => p.Name),
                ("Category", p => p.Category),
                ("Price", p => $"${p.CurrentPrice:F2}"),
                ("Stock", p => p.Stock),
                ("Demand", p => $"{p.DemandScore:F1}"));
        }

        private void SearchProducts()
        {
            var searchTerm = ConsoleHelper.GetInput("Enter search term");
            var products = _productService.SearchProducts(searchTerm);
            
            if (products.Any())
            {
                ConsoleHelper.WriteHeader($"Search Results for '{searchTerm}'");
                ConsoleHelper.DisplayTable(products,
                    ("ID", p => p.Id),
                    ("Name", p => p.Name),
                    ("Category", p => p.Category),
                    ("Price", p => $"${p.CurrentPrice:F2}"),
                    ("Stock", p => p.Stock));
            }
            else
            {
                ConsoleHelper.WriteWarning("No products found matching your search.");
            }
        }

        private void AddProduct()
        {
            ConsoleHelper.WriteHeader("Add New Product");
            
            var product = new Product
            {
                Name = ConsoleHelper.GetInput("Product Name"),
                Category = ConsoleHelper.GetInput("Category"),
                BasePrice = ConsoleHelper.GetDecimalInput("Base Price"),
                Cost = ConsoleHelper.GetDecimalInput("Cost"),
                Stock = ConsoleHelper.GetIntInput("Initial Stock"),
                Supplier = ConsoleHelper.GetInput("Supplier")
            };

            product.MinPrice = product.Cost * 1.1m; // 10% above cost
            product.MaxPrice = product.BasePrice * 2m; // 2x base price

            var created = _productService.CreateProduct(product);
            ConsoleHelper.WriteSuccess($"Product created with ID: {created.Id}");
        }

        private void UpdateProduct()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID to update");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            ConsoleHelper.WriteHeader($"Updating Product: {product.Name}");
            
            product.Name = ConsoleHelper.GetInput("Name", product.Name);
            product.Category = ConsoleHelper.GetInput("Category", product.Category);
            product.BasePrice = ConsoleHelper.GetDecimalInput("Base Price", product.BasePrice);
            product.Cost = ConsoleHelper.GetDecimalInput("Cost", product.Cost);
            product.Stock = ConsoleHelper.GetIntInput("Stock", product.Stock);

            if (_productService.UpdateProduct(product))
            {
                ConsoleHelper.WriteSuccess("Product updated successfully.");
            }
            else
            {
                ConsoleHelper.WriteError("Failed to update product.");
            }
        }

        private void DeleteProduct()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID to delete");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            var confirm = ConsoleHelper.GetBoolInput($"Are you sure you want to delete '{product.Name}'?");
            if (confirm && _productService.DeleteProduct(productId))
            {
                ConsoleHelper.WriteSuccess("Product deleted successfully.");
            }
            else
            {
                ConsoleHelper.WriteWarning("Product deletion cancelled or failed.");
            }
        }

        private void UpdateStock()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            ConsoleHelper.WriteInfo($"Current stock for '{product.Name}': {product.Stock}");
            var quantity = ConsoleHelper.GetIntInput("Enter quantity change (+/-)");
            var isAddition = quantity >= 0;
            
            if (_productService.UpdateStock(productId, Math.Abs(quantity), isAddition))
            {
                ConsoleHelper.WriteSuccess($"Stock updated. New stock: {_productService.GetProductById(productId)?.Stock}");
            }
            else
            {
                ConsoleHelper.WriteError("Failed to update stock.");
            }
        }

        private void ShowLowStockReport()
        {
            ConsoleHelper.WriteHeader("Low Stock Report");
            var lowStockProducts = _productService.GetLowStockProducts();
            
            if (lowStockProducts.Any())
            {
                ConsoleHelper.DisplayTable(lowStockProducts,
                    ("ID", p => p.Id),
                    ("Name", p => p.Name),
                    ("Category", p => p.Category),
                    ("Stock", p => p.Stock),
                    ("Status", p => p.Stock == 0 ? "OUT OF STOCK" : "LOW STOCK"));
            }
            else
            {
                ConsoleHelper.WriteSuccess("All products have adequate stock levels.");
            }
        }

        private void ShowTopSellingProducts()
        {
            ConsoleHelper.WriteHeader("Top Selling Products");
            var topProducts = _productService.GetTopSellingProducts();
            
            ConsoleHelper.DisplayTable(topProducts,
                ("ID", p => p.Id),
                ("Name", p => p.Name),
                ("Category", p => p.Category),
                ("Sales Count", p => p.SalesCount),
                ("Revenue", p => $"${(p.SalesCount * p.CurrentPrice):F2}"));
        }

        private void AnalyzeAllPrices()
        {
            ConsoleHelper.WriteHeader("Price Analysis");
            ConsoleHelper.WriteInfo("Analyzing optimal prices for all products...");
            
            var analysis = _pricingEngine.AnalyzeAllProducts();
            
            ConsoleHelper.DisplayTable(analysis.Take(20).ToList(),
                ("Product ID", a => a.Item1),
                ("Current", a => $"${a.Item2:F2}"),
                ("Optimal", a => $"${a.Item3:F2}"),
                ("Difference", a => $"${a.Item4:F2}"),
                ("Change %", a => $"{(a.Item4 / a.Item2 * 100):F1}%"));
                
            if (analysis.Count > 20)
            {
                ConsoleHelper.WriteInfo($"Showing top 20 of {analysis.Count} products. Use Update All Prices for complete analysis.");
            }
        }

        private void GetPricingRecommendation()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID for recommendation");
            var recommendation = _pricingEngine.GetPricingRecommendation(productId);
            
            if (recommendation.ProductId == 0)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            ConsoleHelper.WriteHeader($"Pricing Recommendation for {recommendation.ProductName}");
            Console.WriteLine($"Current Price: ${recommendation.CurrentPrice:F2}");
            Console.WriteLine($"Recommended Price: ${recommendation.RecommendedPrice:F2}");
            Console.WriteLine($"Price Difference: ${recommendation.PriceDifference:F2} ({recommendation.PercentageChange:F1}%)");
            Console.WriteLine($"Confidence: {recommendation.Confidence:F1}%");
            Console.WriteLine($"Expected Impact: {recommendation.ExpectedImpact}");
            
            if (recommendation.Reasons.Any())
            {
                Console.WriteLine("\nReasons:");
                foreach (var reason in recommendation.Reasons)
                {
                    Console.WriteLine($"• {reason}");
                }
            }
        }

        private void UpdateAllPrices(bool applyChanges)
        {
            ConsoleHelper.WriteHeader(applyChanges ? "Applying Price Updates" : "Price Update Preview");
            
            if (applyChanges)
            {
                var confirm = ConsoleHelper.GetBoolInput("This will update all product prices. Continue?");
                if (!confirm)
                {
                    ConsoleHelper.WriteWarning("Price update cancelled.");
                    return;
                }
            }

            _pricingEngine.UpdateAllPrices(applyChanges);
            
            if (applyChanges)
            {
                ConsoleHelper.WriteSuccess("All prices have been updated based on dynamic pricing analysis.");
            }
            else
            {
                ConsoleHelper.WriteInfo("This was a preview. Use 'Apply' option to make actual changes.");
            }
        }

        private void ManualPriceUpdate()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            ConsoleHelper.WriteInfo($"Current price for '{product.Name}': ${product.CurrentPrice:F2}");
            ConsoleHelper.WriteInfo($"Price range: ${product.MinPrice:F2} - ${product.MaxPrice:F2}");
            
            var newPrice = ConsoleHelper.GetDecimalInput("Enter new price");
            var reason = ConsoleHelper.GetInput("Enter reason for price change");
            
            if (_productService.UpdateProductPrice(productId, newPrice, reason))
            {
                ConsoleHelper.WriteSuccess("Price updated successfully.");
            }
            else
            {
                ConsoleHelper.WriteError("Failed to update price. Check if price is within allowed range.");
            }
        }

        private void ViewPriceHistory()
        {
            var productId = ConsoleHelper.GetIntInput("Enter Product ID");
            var product = _productService.GetProductById(productId);
            
            if (product == null)
            {
                ConsoleHelper.WriteError("Product not found.");
                return;
            }

            var history = product.GetPriceHistory();
            
            if (history.Any())
            {
                ConsoleHelper.WriteHeader($"Price History for {product.Name}");
                ConsoleHelper.DisplayTable(history,
                    ("Date", h => h.ChangeDate.ToString("MM/dd/yyyy HH:mm")),
                    ("Old Price", h => $"${h.OldPrice:F2}"),
                    ("New Price", h => $"${h.NewPrice:F2}"),
                    ("Change", h => $"${h.PriceChange:F2}"),
                    ("Reason", h => h.Reason));
            }
            else
            {
                ConsoleHelper.WriteWarning("No price history available for this product.");
            }
        }

        private void ShowPricingRules()
        {
            ConsoleHelper.WriteHeader("Pricing Rules");
            var rules = _sampleDataGenerator.GeneratePricingRules();
            
            ConsoleHelper.DisplayTable(rules,
                ("ID", r => r.Id),
                ("Name", r => r.Name),
                ("Type", r => r.RuleType),
                ("Active", r => r.IsActive ? "Yes" : "No"),
                ("Priority", r => r.Priority),
                ("Min Mult.", r => r.MinPriceMultiplier),
                ("Max Mult.", r => r.MaxPriceMultiplier));
        }

        private void ShowDashboard()
        {
            ConsoleHelper.WriteTitle("Sales Dashboard");
            
            var products = _productService.GetAllProducts();
            var customers = _customerService.GetAllCustomers();
            var orders = _orderService.GetRecentOrders(30);
            
            var totalRevenue = _orderService.GetTotalRevenue(DateTime.Now.AddDays(-30));
            var totalOrders = orders.Count;
            var activeCustomers = customers.Count(c => c.IsActive);
            var lowStockProducts = _productService.GetLowStockProducts().Count;
            var avgOrderValue = _orderService.GetAverageOrderValue();
            
            ConsoleHelper.WriteHeader("Key Metrics (Last 30 Days)");
            Console.WriteLine($"Total Revenue: ${totalRevenue:F2}");
            Console.WriteLine($"Total Orders: {totalOrders}");
            Console.WriteLine($"Active Customers: {activeCustomers}");
            Console.WriteLine($"Average Order Value: ${avgOrderValue:F2}");
            Console.WriteLine($"Low Stock Products: {lowStockProducts}");
            Console.WriteLine($"Total Products: {products.Count}");
            
            ConsoleHelper.WriteHeader("Category Performance");
            var categoryStats = _productService.GetCategoryStats();
            foreach (var category in categoryStats)
            {
                Console.WriteLine($"{category.Key}: {category.Value} products");
            }
            
            ConsoleHelper.WriteHeader("Customer Segments");
            var segmentStats = _customerService.GetSegmentDistribution();
            foreach (var segment in segmentStats)
            {
                Console.WriteLine($"{segment.Key}: {segment.Value} customers");
            }
        }

        private void ShowDataMenu()
        {
            while (true)
            {
                ConsoleHelper.WriteTitle("Data Management");
                
                Console.WriteLine("1. Generate Sample Data");
                Console.WriteLine("2. Backup Data");
                Console.WriteLine("3. Export Reports");
                Console.WriteLine("4. Data Statistics");
                Console.WriteLine("5. Back to Main Menu");
                Console.WriteLine();

                var choice = ConsoleHelper.GetInput("Enter your choice");
                
                switch (choice)
                {
                    case "1":
                        GenerateSampleData();
                        break;
                    case "2":
                        BackupData();
                        break;
                    case "3":
                        ConsoleHelper.WriteInfo("Export functionality would be implemented here.");
                        break;
                    case "4":
                        ShowDataStatistics();
                        break;
                    case "5":
                        return;
                    default:
                        ConsoleHelper.WriteError("Invalid choice. Please try again.");
                        break;
                }
                
                if (choice != "5")
                    ConsoleHelper.PressAnyKeyToContinue();
            }
        }

        private void ShowCustomerMenu()
        {
            ConsoleHelper.WriteInfo("Customer management functionality available. Implementation similar to Product menu.");
            // Implementation would follow similar pattern to Product menu
        }

        private void ShowOrderMenu()
        {
            ConsoleHelper.WriteInfo("Order management functionality available. Implementation similar to Product menu.");
            // Implementation would follow similar pattern to Product menu
        }

        private void ShowAnalyticsMenu()
        {
            ConsoleHelper.WriteInfo("Analytics and reporting functionality available. Implementation similar to other menus.");
            // Implementation would include various analytics and reports
        }

        private void GenerateSampleData()
        {
            var confirm = ConsoleHelper.GetBoolInput("This will replace existing data. Continue?");
            if (!confirm)
                return;

            ConsoleHelper.WriteInfo("Generating sample data...");
            
            var products = _sampleDataGenerator.GenerateProducts(50);
            var customers = _sampleDataGenerator.GenerateCustomers(100);
            var orders = _sampleDataGenerator.GenerateOrders(products, customers, 200);
            var rules = _sampleDataGenerator.GeneratePricingRules();
            var competitors = _sampleDataGenerator.GenerateCompetitors();
            var competitorPrices = _sampleDataGenerator.GenerateCompetitorPrices(competitors, products);
            
            _dataManager.SaveData(products, "products");
            _dataManager.SaveData(customers, "customers");
            _dataManager.SaveData(orders, "orders");
            _dataManager.SaveData(rules, "pricing-rules");
            _dataManager.SaveData(competitors, "competitors");
            _dataManager.SaveData(competitorPrices, "competitor-prices");
            
            // Load the pricing rules and competitor prices into the engine
            _pricingEngine.LoadPricingRules(rules);
            _pricingEngine.LoadCompetitorPrices(competitorPrices);
            
            // Refresh services with new data
            _productService.RefreshData();
            _customerService.RefreshData();
            _orderService.RefreshData();
            
            ConsoleHelper.WriteSuccess("Sample data generated successfully!");
        }

        private void BackupData()
        {
            _dataManager.BackupData();
            ConsoleHelper.WriteSuccess("Data backed up successfully!");
        }

        private void ShowDataStatistics()
        {
            var products = _productService.GetAllProducts();
            var customers = _customerService.GetAllCustomers();
            var orders = _orderService.GetAllOrders();
            
            ConsoleHelper.WriteHeader("Data Statistics");
            Console.WriteLine($"Products: {products.Count}");
            Console.WriteLine($"Customers: {customers.Count}");
            Console.WriteLine($"Orders: {orders.Count}");
            Console.WriteLine($"Total Inventory Value: ${_productService.GetTotalInventoryValue():F2}");
            Console.WriteLine($"Total Revenue: ${_orderService.GetTotalRevenue():F2}");
        }

        private void InitializeSampleData()
        {
            // Check if data already exists
            var products = _productService.GetAllProducts();
            if (products.Any())
                return;

            // Generate initial sample data silently
            try
            {
                var sampleProducts = _sampleDataGenerator.GenerateProducts(20);
                var sampleCustomers = _sampleDataGenerator.GenerateCustomers(30);
                var sampleOrders = _sampleDataGenerator.GenerateOrders(sampleProducts, sampleCustomers, 50);
                var rules = _sampleDataGenerator.GeneratePricingRules();
                var competitors = _sampleDataGenerator.GenerateCompetitors();
                var competitorPrices = _sampleDataGenerator.GenerateCompetitorPrices(competitors, sampleProducts, 10);
                
                _dataManager.SaveData(sampleProducts, "products");
                _dataManager.SaveData(sampleCustomers, "customers");
                _dataManager.SaveData(sampleOrders, "orders");
                _dataManager.SaveData(rules, "pricing-rules");
                _dataManager.SaveData(competitors, "competitors");
                _dataManager.SaveData(competitorPrices, "competitor-prices");
                
                _pricingEngine.LoadPricingRules(rules);
                _pricingEngine.LoadCompetitorPrices(competitorPrices);
                
                _productService.RefreshData();
                _customerService.RefreshData();
                _orderService.RefreshData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not initialize sample data: {ex.Message}");
            }
        }
    }
}