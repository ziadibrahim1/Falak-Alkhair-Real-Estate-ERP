using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class MaintenanceEmployeeConfiguration : IEntityTypeConfiguration<MaintenanceEmployee>
{
    public void Configure(EntityTypeBuilder<MaintenanceEmployee> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.EmployeeCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Skills).HasMaxLength(500);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VendorCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.VendorCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CommercialRegistrationNumber).HasMaxLength(20);
        builder.Property(x => x.VatNumber).HasMaxLength(20);
        builder.Property(x => x.Rating).HasPrecision(3, 2);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.RequestNumber }).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.EstimatedCost).HasPrecision(18, 2);
        builder.Property(x => x.ActualCost).HasPrecision(18, 2);

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedEmployee).WithMany(e => e.AssignedRequests).HasForeignKey(x => x.AssignedEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedVendor).WithMany(v => v.AssignedRequests).HasForeignKey(x => x.AssignedVendorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class MaintenanceQuotationConfiguration : IEntityTypeConfiguration<MaintenanceQuotation>
{
    public void Configure(EntityTypeBuilder<MaintenanceQuotation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuotationNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.QuotationNumber }).IsUnique();
        builder.Property(x => x.VatPercentage).HasPrecision(5, 2);
        builder.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.VatAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor).WithMany(v => v.Quotations).HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MaintenanceRequest).WithMany(r => r.Quotations).HasForeignKey(x => x.MaintenanceRequestId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.MaintenanceRequestId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class MaintenanceQuotationItemConfiguration : IEntityTypeConfiguration<MaintenanceQuotationItem>
{
    public void Configure(EntityTypeBuilder<MaintenanceQuotationItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        builder.HasOne(x => x.Quotation).WithMany(q => q.Items).HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.QuotationId);
    }
}
