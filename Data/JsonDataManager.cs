using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DynamicPricingSalesSystem.Models;

namespace DynamicPricingSalesSystem.Data
{
    public class JsonDataManager
    {
        private readonly string _dataDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataManager(string dataDirectory = "Data/Json")
        {
            _dataDirectory = dataDirectory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public List<T> LoadData<T>(string fileName) where T : class
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {fileName}: {ex.Message}");
                return new List<T>();
            }
        }

        public void SaveData<T>(List<T> data, string fileName) where T : class
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {fileName}: {ex.Message}");
            }
        }

        public T LoadSingleData<T>(string fileName) where T : class, new()
        {
            var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
            
            if (!File.Exists(filePath))
            {
                return new T();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? new T();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {fileName}: {ex.Message}");
                return new T();
            }
        }

        public void SaveSingleData<T>(T data, string fileName) where T : class
        {
            try
            {
                var filePath = Path.Combine(_dataDirectory, $"{fileName}.json");
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {fileName}: {ex.Message}");
            }
        }

        public void BackupData()
        {
            var backupDir = Path.Combine(_dataDirectory, "Backups", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(backupDir);

            foreach (var file in Directory.GetFiles(_dataDirectory, "*.json"))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(backupDir, fileName));
            }
        }

        public List<string> GetAvailableBackups()
        {
            var backupDir = Path.Combine(_dataDirectory, "Backups");
            if (!Directory.Exists(backupDir))
                return new List<string>();

            return new List<string>(Directory.GetDirectories(backupDir));
        }
    }
}