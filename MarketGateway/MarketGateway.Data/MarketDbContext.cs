using MarketGateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.Data
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options) { }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<ApiUsage> ApiUsages => Set<ApiUsage>();
        public DbSet<ParseFailure> ParseFailures => Set<ParseFailure>();
        public DbSet<ApiCall> ApiCalls => Set<ApiCall>();

        public DbSet<QuoteEntity> Quotes => Set<QuoteEntity>();
        public DbSet<OhlcvSeriesEntity> OhlcvSeries => Set<OhlcvSeriesEntity>();
        public DbSet<OhlcvDailyEntity>  OhlcvDaily  => Set<OhlcvDailyEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketDbContext).Assembly);
        }
    }
}