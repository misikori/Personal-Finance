using Microsoft.EntityFrameworkCore;

namespace MarketGateway.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options) { }

        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<PriceBar> PriceBars { get; set; }
        public DbSet<ApiUsage> ApiUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Symbol>().ToTable("Symbols");
            modelBuilder.Entity<PriceBar>().ToTable("PriceBars");
            
            modelBuilder.Entity<ApiUsage>()
                .HasIndex(x => new { x.Vendor, x.Date })
                .IsUnique();  // Replaces the ON CONFLICT from SQLite
        }
    }

    public class Symbol
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PriceBar
    {
        public Guid Id { get; set; }
        public Guid SymbolId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
    public class ApiUsage
    {
        public int Id { get; set; }
        public string Vendor { get; set; }
        public DateTime Date { get; set; } // Stored as UTC
        public int CallsMade { get; set; }
    }
    
}