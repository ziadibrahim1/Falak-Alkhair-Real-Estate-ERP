using FalakAlkhair.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FullNameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FullNameEn).HasMaxLength(200);
        builder.Property(x => x.EmployeeNumber).HasMaxLength(30);

        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.EmployeeNumber);
    }
}

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(x => x.NameAr).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
    }
}
