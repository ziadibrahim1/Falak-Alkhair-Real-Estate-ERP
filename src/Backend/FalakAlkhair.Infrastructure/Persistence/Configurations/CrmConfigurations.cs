using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AgentCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.AgentCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FalLicenseNumber).HasMaxLength(50);
        builder.Property(x => x.Specialization).HasMaxLength(200);

        builder.Property(x => x.DefaultCommissionPercentage).HasPrecision(5, 2);
        builder.Property(x => x.DefaultCommissionFixedAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.FalLicenseNumber);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BuyerCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.BuyerCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PreferredCity).HasMaxLength(100);
        builder.Property(x => x.PreferredDistrict).HasMaxLength(100);

        builder.Property(x => x.Budget).HasPrecision(18, 2);
        builder.Property(x => x.MinArea).HasPrecision(18, 2);
        builder.Property(x => x.MaxArea).HasPrecision(18, 2);

        builder.HasOne(x => x.AssignedAgent).WithMany().HasForeignKey(x => x.AssignedAgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.PreferredCity);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SellerCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.SellerCode }).IsUnique();

        builder.Property(x => x.AskingPrice).HasPrecision(18, 2);
        builder.Property(x => x.MinimumPrice).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);

        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedAgent).WithMany().HasForeignKey(x => x.AssignedAgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.MandateStatus);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeadCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.LeadCode }).IsUnique();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.InterestedProperty).WithMany().HasForeignKey(x => x.InterestedPropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedAgent).WithMany().HasForeignKey(x => x.AssignedAgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Campaign).WithMany(c => c.Leads).HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LeadType);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CommissionNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.CommissionNumber }).IsUnique();

        builder.Property(x => x.BaseAmount).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);
        builder.Property(x => x.CommissionAmount).HasPrecision(18, 2);
        builder.Property(x => x.VatPercentage).HasPrecision(5, 2);
        builder.Property(x => x.VatAmount).HasPrecision(18, 2);
        builder.Property(x => x.NetCommissionAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Agent).WithMany(a => a.Commissions).HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Lease).WithMany(l => l.Commissions).HasForeignKey(x => x.LeaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sale).WithMany(s => s.Commissions).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Auction).WithMany(a => a.Commissions).HasForeignKey(x => x.AuctionId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AgentId);
        builder.HasIndex(x => x.CompanyId);
    }
}
