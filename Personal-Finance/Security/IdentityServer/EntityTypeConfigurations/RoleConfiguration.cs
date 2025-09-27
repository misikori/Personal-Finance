using IdentityServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityServer.EntityTypeConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Name = Roles.User,
                NormalizedName = Roles.User.ToUpper(),
            },
            new IdentityRole
            {
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpper(),
            }
        );
    }   
}