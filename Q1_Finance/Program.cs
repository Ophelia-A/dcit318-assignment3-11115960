using System;
using System.Collections.Generic;

namespace FinanceManagementSystem
{
    // Record for financial data
    public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

    // Interface for processing transactions
    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    // Concrete implementations of ITransactionProcessor
    public sealed class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction) =>
            Console.WriteLine($"[Bank Transfer] Processing GHS {transaction.Amount:N2} for {transaction.Category} (Tx #{transaction.Id}) on {transaction.Date:d}");
    }

    public sealed class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction) =>
            Console.WriteLine($"[Mobile Money] Paid GHS {transaction.Amount:N2} towards {transaction.Category} (Tx #{transaction.Id}) on {transaction.Date:d}");
    }

    public sealed class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction) =>
            Console.WriteLine($"[Crypto Wallet] Sent ≈GHS {transaction.Amount:N2} for {transaction.Category} (Tx #{transaction.Id}) on {transaction.Date:d}");
    }

    // Base Account class
    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number is required.", nameof(accountNumber));
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be negative.");

            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            if (transaction is null) throw new ArgumentNullException(nameof(transaction));
            Balance -= transaction.Amount;
        }
    }

    // Sealed specialized SavingsAccount
    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance) : base(accountNumber, initialBalance) { }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction.Amount > Balance)
            {
                Console.WriteLine("Insufficient funds");
                return;
            }

            Balance -= transaction.Amount;
            Console.WriteLine($"New balance: GHS {Balance:N2}");
        }
    }

    // FinanceApp to integrate and simulate
    public class FinanceApp
    {
        private readonly List<Transaction> _transactions = new();

        public void Run()
        {
            Console.WriteLine("=== Finance App ===");

            var account = new SavingsAccount("SA-001", 1000m);

            var t1 = new Transaction(1, DateTime.Today, 120m, "Groceries");
            var t2 = new Transaction(2, DateTime.Today, 250m, "Utilities");
            var t3 = new Transaction(3, DateTime.Today, 800m, "Entertainment");

            ITransactionProcessor p1 = new MobileMoneyProcessor();
            ITransactionProcessor p2 = new BankTransferProcessor();
            ITransactionProcessor p3 = new CryptoWalletProcessor();

            // Process and apply transactions
            p1.Process(t1);
            account.ApplyTransaction(t1);
            _transactions.Add(t1);

            p2.Process(t2);
            account.ApplyTransaction(t2);
            _transactions.Add(t2);

            p3.Process(t3);
            account.ApplyTransaction(t3);
            _transactions.Add(t3);

            Console.WriteLine($"Stored {_transactions.Count} transactions for account {account.AccountNumber}.");
        }
    }

    // Entry point
    public static class Program
    {
        public static void Main()
        {
            new FinanceApp().Run();
        }
    }
}
