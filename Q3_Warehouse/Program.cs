using System;
using System.Collections.Generic;

namespace WarehouseSystem
{
    // a) Marker interface for inventory items
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }

    // b) ElectronicItem
    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public string Brand { get; }
        public int WarrantyMonths { get; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            Brand = brand;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString()
            => $"ElectronicItem {{ Id={Id}, Name={Name}, Qty={Quantity}, Brand={Brand}, Warranty={WarrantyMonths}m }}";
    }

    // c) GroceryItem
    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString()
            => $"GroceryItem {{ Id={Id}, Name={Name}, Qty={Quantity}, Expiry={ExpiryDate:d} }}";
    }

    // e) Custom exceptions
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }

    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }

    // d) Generic Inventory Repository
    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new(); // key = item.Id

        public void AddItem(T item)
        {
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with Id {item.Id} already exists.");
            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Item with Id {id} not found.");
            return item;
        }

        public void RemoveItem(int id)
        {
            if (!_items.Remove(id))
                throw new ItemNotFoundException($"Cannot remove: item with Id {id} not found.");
        }

        public List<T> GetAllItems() => new(_items.Values);

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative.");
            var item = GetItemById(id); // may throw ItemNotFoundException
            item.Quantity = newQuantity;
        }
    }

    // f) WareHouseManager
    public class WareHouseManager
    {
        private readonly InventoryRepository<ElectronicItem> _electronics = new();
        private readonly InventoryRepository<GroceryItem> _groceries = new();

        public void SeedData()
        {
            // 2–3 items each
            _electronics.AddItem(new ElectronicItem(101, "Smartphone", 20, "TechOne", 24));
            _electronics.AddItem(new ElectronicItem(102, "Laptop", 10, "NovaBook", 12));
            _electronics.AddItem(new ElectronicItem(103, "Headphones", 35, "SoundMax", 6));

            _groceries.AddItem(new GroceryItem(201, "Rice (5kg)", 50, DateTime.Today.AddMonths(8)));
            _groceries.AddItem(new GroceryItem(202, "Milk (1L)", 80, DateTime.Today.AddDays(30)));
            _groceries.AddItem(new GroceryItem(203, "Eggs (Tray)", 25, DateTime.Today.AddDays(14)));
        }

        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            foreach (var item in repo.GetAllItems())
                Console.WriteLine(item);
        }

        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var current = repo.GetItemById(id);
                var newQty = checked(current.Quantity + quantity); // protect against overflow
                repo.UpdateQuantity(id, newQty);
                Console.WriteLine($"Stock updated: Id={id}, {current.Name} now Qty={newQty}");
            }
            catch (ItemNotFoundException ex)
            {
                Console.WriteLine($"[WARN] {ex.Message}");
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine($"[WARN] {ex.Message}");
            }
            catch (OverflowException)
            {
                Console.WriteLine("[WARN] Quantity overflow while updating stock.");
            }
        }

        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id);
                Console.WriteLine($"Removed item with Id={id}");
            }
            catch (ItemNotFoundException ex)
            {
                Console.WriteLine($"[WARN] {ex.Message}");
            }
        }

        public void DemoErrorCases()
        {
            // Try to add a duplicate item
            try
            {
                _groceries.AddItem(new GroceryItem(201, "Rice (5kg) - Duplicate", 10, DateTime.Today.AddMonths(6)));
            }
            catch (DuplicateItemException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }

            // Try to remove a non-existent item
            RemoveItemById(_electronics, 999);

            // Try invalid quantity update (negative)
            try
            {
                _groceries.UpdateQuantity(202, -5);
            }
            catch (InvalidQuantityException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
            catch (ItemNotFoundException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        public void Run()
        {
            SeedData();

            Console.WriteLine("=== Grocery Items ===");
            PrintAllItems(_groceries);

            Console.WriteLine("\n=== Electronic Items ===");
            PrintAllItems(_electronics);

            Console.WriteLine("\n=== Stock Operations ===");
            IncreaseStock(_groceries, 202, 15);   // Milk +15
            IncreaseStock(_electronics, 103, 7);  // Headphones +7

            Console.WriteLine("\n=== Error Demonstrations ===");
            DemoErrorCases();
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var manager = new WareHouseManager();
            manager.Run();
        }
    }
}
