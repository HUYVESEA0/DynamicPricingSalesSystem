using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.Data
{
    public class JsonDataStorage
    {
        private readonly string _dataDirectory;

        public JsonDataStorage(string dataDirectory = "Data")
        {
            _dataDirectory = dataDirectory;
            EnsureDataDirectoryExists();
        }

        private void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public async Task<List<T>> LoadDataAsync<T>(string fileName) where T : class
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                return JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data from {fileName}: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task SaveDataAsync<T>(List<T> data, string fileName) where T : class
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(data, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to {fileName}: {ex.Message}");
                throw;
            }
        }

        public async Task<T?> LoadSingleAsync<T>(string fileName) where T : class
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading single data from {fileName}: {ex.Message}");
                return null;
            }
        }

        public async Task SaveSingleAsync<T>(T data, string fileName) where T : class
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                var json = JsonSerializer.Serialize(data, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving single data to {fileName}: {ex.Message}");
                throw;
            }
        }

        public async Task BackupDataAsync()
        {
            var backupDirectory = Path.Combine(_dataDirectory, "Backups", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(backupDirectory);

            var dataFiles = Directory.GetFiles(_dataDirectory, "*.json");
            
            foreach (var file in dataFiles)
            {
                var fileName = Path.GetFileName(file);
                var backupPath = Path.Combine(backupDirectory, fileName);
                File.Copy(file, backupPath);
            }

            Console.WriteLine($"Data backed up to: {backupDirectory}");
        }

        public List<string> GetAvailableBackups()
        {
            var backupDirectory = Path.Combine(_dataDirectory, "Backups");
            if (!Directory.Exists(backupDirectory))
            {
                return new List<string>();
            }

            return new List<string>(Directory.GetDirectories(backupDirectory));
        }
    }
}