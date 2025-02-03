using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

namespace MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Add index for better query performance
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_ApplicationUser_TenantId");
        
        builder.Property(x => x.TenantId)
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(x => x.DisplayName)
            .IsRequired();
    }
}