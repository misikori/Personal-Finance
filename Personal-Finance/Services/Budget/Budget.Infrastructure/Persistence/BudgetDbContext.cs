using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) 
        { 
        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure relationships

            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId);
        }
    }
}
