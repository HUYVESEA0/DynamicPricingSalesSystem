using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPricingSalesSystem.Data;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Services
{
    public class ProductService
    {
        private readonly JsonDataManager _dataManager;
        private List<Product> _products;

        public ProductService(JsonDataManager dataManager)
        {
            _dataManager = dataManager;
            _products = _dataManager.LoadData<Product>("products");
        }

        public List<Product> GetAllProducts() => _products.ToList();

        public Product? GetProductById(int id) => _products.FirstOrDefault(p => p.Id == id);

        public List<Product> GetProductsByCategory(string category) =>
            _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Product> GetLowStockProducts(int threshold = 10) =>
            _products.Where(p => p.IsLowStock(threshold)).ToList();

        public Product CreateProduct(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            product.CurrentPrice = product.BasePrice;
            product.LastUpdated = DateTime.Now;
            
            _products.Add(product);
            _dataManager.SaveData(_products, "products");
            
            return product;
        }

        public bool UpdateProduct(Product product)
        {
            var existingProduct = GetProductById(product.Id);
            if (existingProduct == null)
                return false;

            var index = _products.IndexOf(existingProduct);
            product.LastUpdated = DateTime.Now;
            _products[index] = product;
            
            _dataManager.SaveData(_products, "products");
            return true;
        }

        public bool DeleteProduct(int id)
        {
            var product = GetProductById(id);
            if (product == null)
                return false;

            _products.Remove(product);
            _dataManager.SaveData(_products, "products");
            return true;
        }

        public bool UpdateProductPrice(int productId, decimal newPrice, string reason = "Manual Update")
        {
            var product = GetProductById(productId);
            if (product == null)
                return false;

            product.UpdatePrice(newPrice, reason);
            _dataManager.SaveData(_products, "products");
            return true;
        }

        public bool UpdateStock(int productId, int quantity, bool isAddition = true)
        {
            var product = GetProductById(productId);
            if (product == null)
                return false;

            if (isAddition)
            {
                product.Stock += quantity;
            }
            else
            {
                product.Stock = Math.Max(0, product.Stock - quantity);
            }

            product.LastUpdated = DateTime.Now;
            _dataManager.SaveData(_products, "products");
            return true;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _products.ToList();

            return _products.Where(p => 
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Supplier.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        public Dictionary<string, int> GetCategoryStats()
        {
            return _products.GroupBy(p => p.Category)
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        public List<Product> GetTopSellingProducts(int count = 10)
        {
            return _products.OrderByDescending(p => p.SalesCount)
                          .Take(count)
                          .ToList();
        }

        public List<Product> GetMostProfitableProducts(int count = 10)
        {
            return _products.OrderByDescending(p => p.CalculateProfitMargin())
                          .Take(count)
                          .ToList();
        }

        public decimal GetTotalInventoryValue()
        {
            return _products.Sum(p => p.CurrentPrice * p.Stock);
        }

        public List<Product> GetProductsNeedingReorder(int minStock = 10)
        {
            return _products.Where(p => p.Stock <= minStock).ToList();
        }

        public void UpdateDemandScores(List<Order> recentOrders)
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var productSales = recentOrders
                .Where(o => o.OrderDate >= cutoffDate)
                .SelectMany(o => o.Items)
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            foreach (var product in _products)
            {
                var salesCount = productSales.GetValueOrDefault(product.Id, 0);
                product.DemandScore = Math.Max(0.1, Math.Min(3.0, salesCount / 10.0));
                product.SalesCount = salesCount;
            }

            _dataManager.SaveData(_products, "products");
        }

        public void BulkUpdatePrices(List<(int ProductId, decimal NewPrice, string Reason)> priceUpdates)
        {
            foreach (var (productId, newPrice, reason) in priceUpdates)
            {
                UpdateProductPrice(productId, newPrice, reason);
            }
        }

        public List<Product> GetProductsInPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _products.Where(p => p.CurrentPrice >= minPrice && p.CurrentPrice <= maxPrice).ToList();
        }

        public void RefreshData()
        {
            _products = _dataManager.LoadData<Product>("products");
        }
    }
}