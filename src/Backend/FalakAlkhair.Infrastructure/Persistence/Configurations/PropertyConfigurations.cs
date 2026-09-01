using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OwnerCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.OwnerCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.CommercialRegistrationNumber).HasMaxLength(20);
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Iban).HasMaxLength(24);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PropertyCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.PropertyCode }).IsUnique();
        builder.Property(x => x.PropertyName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.DeedNumber).HasMaxLength(50);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.NationalAddressShortCode).HasMaxLength(10);

        builder.Property(x => x.TotalArea).HasPrecision(18, 2);
        builder.Property(x => x.BuildingArea).HasPrecision(18, 2);
        builder.Property(x => x.Latitude).HasPrecision(10, 7);
        builder.Property(x => x.Longitude).HasPrecision(10, 7);

        builder.HasOne(x => x.Owner).WithMany(o => o.Properties).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.DeedNumber);
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.UnitCode }).IsUnique();
        builder.Property(x => x.UnitNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Floor).HasMaxLength(20);
        builder.Property(x => x.ElectricityMeterNumber).HasMaxLength(50);
        builder.Property(x => x.WaterMeterNumber).HasMaxLength(50);

        builder.Property(x => x.Area).HasPrecision(18, 2);
        builder.Property(x => x.RentalPrice).HasPrecision(18, 2);
        builder.Property(x => x.SalePrice).HasPrecision(18, 2);
        builder.Property(x => x.DepositAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Property).WithMany(p => p.Units).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CurrentStatus);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class PropertyManagementAgreementConfiguration : IEntityTypeConfiguration<PropertyManagementAgreement>
{
    public void Configure(EntityTypeBuilder<PropertyManagementAgreement> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContractNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.ContractNumber }).IsUnique();

        builder.Property(x => x.ManagementFee).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);

        builder.HasOne(x => x.Owner).WithMany(o => o.ManagementAgreements).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Property).WithMany(p => p.ManagementAgreements).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EndDate);
        builder.HasIndex(x => x.CompanyId);
    }
}
