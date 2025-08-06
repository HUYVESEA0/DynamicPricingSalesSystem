using DynamicPricingSalesSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Data.Repositories
{
    public class ProductRepository
    {
        private readonly JsonDataStorage _storage;
        private List<Product> _products;

        public ProductRepository(JsonDataStorage storage)
        {
            _storage = storage;
            _products = new List<Product>();
        }

        public async Task LoadAsync()
        {
            _products = await _storage.LoadDataAsync<Product>("products");
        }

        public async Task SaveAsync()
        {
            await _storage.SaveDataAsync(_products, "products");
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return _products.Where(p => p.IsActive).ToList();
        }

        public async Task<Product> AddAsync(Product product)
        {
            if (product.Id == 0)
            {
                product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            }
            
            product.CreatedDate = DateTime.Now;
            product.LastUpdated = DateTime.Now;
            product.CurrentPrice = product.BasePrice;
            
            _products.Add(product);
            await SaveAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(Product product)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct == null) return null;

            product.LastUpdated = DateTime.Now;
            var index = _products.IndexOf(existingProduct);
            _products[index] = product;
            await SaveAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return false;

            product.IsActive = false; // Soft delete
            await SaveAsync();
            return true;
        }

        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            return _products.Where(p => p.IsActive && 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            return _products.Where(p => p.IsActive && (
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Category.ToLower().Contains(searchTerm) ||
                p.Brand.ToLower().Contains(searchTerm) ||
                p.SKU.ToLower().Contains(searchTerm)
            )).ToList();
        }

        public async Task<bool> UpdatePriceAsync(int productId, decimal newPrice, string reason = "Manual update")
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;

            // Add to price history
            product.PriceHistory.Add(new PriceHistory
            {
                Date = DateTime.Now,
                Price = product.CurrentPrice,
                Reason = reason,
                UpdatedBy = "System"
            });

            product.CurrentPrice = newPrice;
            product.LastUpdated = DateTime.Now;
            
            await UpdateAsync(product);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;

            product.Stock = newStock;
            product.LastUpdated = DateTime.Now;
            
            await UpdateAsync(product);
            return true;
        }

        public async Task<List<Product>> GetLowStockProductsAsync()
        {
            return _products.Where(p => p.IsActive && p.NeedsReorder).ToList();
        }

        public async Task<List<Product>> GetTopSellingProductsAsync(int count = 10)
        {
            return _products.Where(p => p.IsActive)
                           .OrderByDescending(p => p.Metrics.TotalSold)
                           .Take(count)
                           .ToList();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return _products.Where(p => p.IsActive)
                           .Select(p => p.Category)
                           .Distinct()
                           .OrderBy(c => c)
                           .ToList();
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            return _products.Where(p => p.IsActive)
                           .Select(p => p.Brand)
                           .Distinct()
                           .OrderBy(b => b)
                           .ToList();
        }

        public async Task UpdateProductMetricsAsync(int productId, int quantitySold, decimal revenue)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return;

            product.Metrics.TotalSold += quantitySold;
            product.Metrics.TotalRevenue += revenue;
            product.Metrics.LastSold = DateTime.Now;
            product.Stock -= quantitySold;
            
            // Update demand score based on sales velocity
            var daysSinceCreated = (DateTime.Now - product.CreatedDate).Days;
            if (daysSinceCreated > 0)
            {
                product.Metrics.DemandScore = (decimal)product.Metrics.TotalSold / daysSinceCreated;
            }

            await UpdateAsync(product);
        }

        public async Task<Dictionary<string, object>> GetProductAnalyticsAsync()
        {
            var analytics = new Dictionary<string, object>
            {
                ["TotalProducts"] = _products.Count(p => p.IsActive),
                ["TotalValue"] = _products.Where(p => p.IsActive).Sum(p => p.InventoryValue),
                ["AveragePrice"] = _products.Where(p => p.IsActive).Average(p => p.CurrentPrice),
                ["LowStockCount"] = _products.Count(p => p.IsActive && p.NeedsReorder),
                ["CategoryDistribution"] = _products.Where(p => p.IsActive)
                    .GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["TopCategories"] = _products.Where(p => p.IsActive)
                    .GroupBy(p => p.Category)
                    .OrderByDescending(g => g.Sum(p => p.Metrics.TotalRevenue))
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Metrics.TotalRevenue))
            };

            return analytics;
        }
    }
}