using IdentityServer.Entities;
using IdentityServer.EntityTypeConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IdentityServer.Data;

public class ApplicationContext : IdentityDbContext<User>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    
    public ApplicationContext(DbContextOptions options) 
        : base(options)
    {
    }

   protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new RoleConfiguration());
        builder.ApplyConfiguration(new UserConfiguration());
    }
   
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Suppress PendingModelChangesWarning
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

        base.OnConfiguring(optionsBuilder);
    }

}