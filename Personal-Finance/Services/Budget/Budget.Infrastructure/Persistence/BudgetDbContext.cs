using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence
{
    public class BudgetDbContext(DbContextOptions<BudgetDbContext> options) : DbContext(options)
    {
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<SpendingLimit> SpendingLimits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure relationships

            _ = modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId);

            // Use fixed GUIDs instead of Guid.NewGuid()
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var categoryGroceriesId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var categoryBillsId     = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var categorySalaryId    = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var walletUsdId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var walletEurId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            modelBuilder.Entity<Wallet>().HasData(
                new Wallet { Id = walletUsdId, Currency = "USD", CurrentBalance = 2500, Name = "Main funds", UserId = userId },
                new Wallet { Id = walletEurId, Currency = "EUR", CurrentBalance = 800, Name = "Vacation funds", UserId = userId }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = categoryBillsId, UserId = userId, Name = "Bills" },
                new Category { Id = categoryGroceriesId, UserId = userId, Name = "Groceries" },
                new Category { Id = categorySalaryId, UserId = userId, Name = "Salary" }
            );

            modelBuilder.Entity<Transaction>().HasData(
                new Transaction {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    UserId = userId, WalletId = walletUsdId,
                    CategoryName = "Groceries", Amount = 125, TransactionType = TransactionType.Expense,
                    Description = "Weekly grocery run", Date = new DateTime(2025, 10, 1), Currency = "USD"
                },
                new Transaction {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    UserId = userId, WalletId = walletUsdId,
                    CategoryName = "Bills", Amount = 50, TransactionType = TransactionType.Expense,
                    Description = "Internet", Date = new DateTime(2025, 10, 2), Currency = "USD"
                }
            );

        }
    }
}
