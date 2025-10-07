using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;

namespace Portfolio.Data;

/// <summary>
/// Database context for Portfolio service
/// Manages positions and transactions in SQL Server
/// NOTE: User budgets are managed by the separate Budget microservice
/// </summary>
public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// User stock positions
    /// </summary>
    public DbSet<PortfolioPosition> Positions { get; set; }
    
    /// <summary>
    /// Buy/Sell transaction history
    /// </summary>
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PortfolioPosition configuration
        modelBuilder.Entity<PortfolioPosition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.AveragePurchasePrice).HasPrecision(18, 4);
            
            // Create unique index on Username + Symbol (one position per symbol per user)
            entity.HasIndex(e => new { e.Username, e.Symbol }).IsUnique();
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.PricePerShare).HasPrecision(18, 4);
            
            // Index for querying user transactions
            entity.HasIndex(e => e.Username);
            entity.HasIndex(e => e.TransactionDate);
        });
    }
}


