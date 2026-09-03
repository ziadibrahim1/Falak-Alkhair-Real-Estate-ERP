using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.TenantCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.CommercialRegistrationNumber).HasMaxLength(20);
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Employer).HasMaxLength(200);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.NationalId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeaseNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.LeaseNumber }).IsUnique();

        builder.Property(x => x.AnnualRentAmount).HasPrecision(18, 2);
        builder.Property(x => x.SecurityDeposit).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);
        builder.Property(x => x.VatPercentage).HasPrecision(5, 2);
        builder.Property(x => x.TerminationReason).HasMaxLength(500);

        builder.HasOne(x => x.Tenant).WithMany(t => t.Leases).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EndDate);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class LeasePaymentConfiguration : IEntityTypeConfiguration<LeasePayment>
{
    public void Configure(EntityTypeBuilder<LeasePayment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Lease).WithMany(l => l.Payments).HasForeignKey(x => x.LeaseId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LeaseId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.PaymentNumber }).IsUnique();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(100);

        builder.HasOne(x => x.Lease).WithMany().HasForeignKey(x => x.LeaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeasePayment).WithMany(p => p.PaymentTransactions).HasForeignKey(x => x.LeasePaymentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentDate);
        builder.HasIndex(x => x.LeaseId);
        builder.HasIndex(x => x.CompanyId);
    }
}
