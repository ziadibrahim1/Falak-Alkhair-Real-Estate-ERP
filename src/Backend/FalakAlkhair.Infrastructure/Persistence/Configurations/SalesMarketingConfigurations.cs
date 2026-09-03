using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ListingCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.ListingCode }).IsUnique();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Features).HasMaxLength(1000);

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class MarketingCampaignConfiguration : IEntityTypeConfiguration<MarketingCampaign>
{
    public void Configure(EntityTypeBuilder<MarketingCampaign> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CampaignCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.CampaignCode }).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Budget).HasPrecision(18, 2);
        builder.Property(x => x.ActualCost).HasPrecision(18, 2);

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class ViewingConfiguration : IEntityTypeConfiguration<Viewing>
{
    public void Configure(EntityTypeBuilder<Viewing> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ViewingCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.ViewingCode }).IsUnique();

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Listing).WithMany(l => l.Viewings).HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ScheduledAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OfferNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.OfferNumber }).IsUnique();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Conditions).HasMaxLength(1000);

        builder.HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SaleNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.SaleNumber }).IsUnique();
        builder.Property(x => x.AskingPrice).HasPrecision(18, 2);
        builder.Property(x => x.FinalPrice).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);
        builder.Property(x => x.VatPercentage).HasPrecision(5, 2);

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Buyer).WithMany().HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Offer).WithMany().HasForeignKey(x => x.OfferId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Stage);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.CompanyId);
    }
}
