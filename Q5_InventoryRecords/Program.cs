using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace InventoryRecords
{
    // b) Marker interface
    public interface IInventoryEntity
    {
        int Id { get; }
    }

    // a) Immutable Inventory Record (record type implements marker)
    public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

    // c) Generic Inventory Logger
    public class InventoryLogger<T> where T : IInventoryEntity
    {
        private readonly List<T> _log = new();
        private readonly string _filePath;

        public InventoryLogger(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));
            _filePath = filePath;
        }

        public void Add(T item) => _log.Add(item);

        public List<T> GetAll() => new(_log);

        public void SaveToFile()
        {
            try
            {
                // d) using + e) exception handling
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                JsonSerializer.Serialize(fs, _log, options);
                Console.WriteLine($"Saved {_log.Count} item(s) to: {_filePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Permission error while saving: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error while saving: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while saving: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                    throw new FileNotFoundException("Inventory file not found.", _filePath);

                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var data = JsonSerializer.Deserialize<List<T>>(fs) ?? new List<T>();
                _log.Clear();
                _log.AddRange(data);
                Console.WriteLine($"Loaded {_log.Count} item(s) from: {_filePath}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Load error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid file format (JSON): {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"I/O error while loading: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while loading: {ex.Message}");
            }
        }
    }

    // f) Integration Layer – InventoryApp
    public class InventoryApp
    {
        private readonly InventoryLogger<InventoryItem> _logger;

        public InventoryApp(string filePath)
        {
            _logger = new InventoryLogger<InventoryItem>(filePath);
        }

        public void SeedSampleData()
        {
            _logger.Add(new InventoryItem(1, "Dish Soap 750ml", 24, DateTime.Today));
            _logger.Add(new InventoryItem(2, "Floor Cleaner 1L", 12, DateTime.Today.AddDays(-1)));
            _logger.Add(new InventoryItem(3, "Bleach 500ml", 30, DateTime.Today.AddDays(-3)));
            _logger.Add(new InventoryItem(4, "Sponge Pack (5x)", 18, DateTime.Today.AddDays(-7)));
            _logger.Add(new InventoryItem(5, "Air Freshener 300ml", 10, DateTime.Today.AddDays(-10)));
        }

        public void SaveData() => _logger.SaveToFile();

        public void LoadData() => _logger.LoadFromFile();

        public void PrintAllItems()
        {
            Console.WriteLine("\n=== Inventory Items ===");
            foreach (var item in _logger.GetAll())
            {
                Console.WriteLine($"Id={item.Id}, Name={item.Name}, Qty={item.Quantity}, Added={item.DateAdded:d}");
            }
        }
    }

    public static class Program
    {
        public static void Main()
        {
            // g) Main application flow
            // Save and load from the app's output directory
            string dataPath = Path.Combine(AppContext.BaseDirectory, "inventory.json");

            // 1) Start a session: seed + save
            var app = new InventoryApp(dataPath);
            app.SeedSampleData();
            app.SaveData();

            // Simulate a "new session": clear from memory by creating a new app instance
            app = new InventoryApp(dataPath);
            app.LoadData();
            app.PrintAllItems();
        }
    }
}