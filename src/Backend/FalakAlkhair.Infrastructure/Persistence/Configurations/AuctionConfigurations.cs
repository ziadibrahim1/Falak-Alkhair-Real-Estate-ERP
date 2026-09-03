using FalakAlkhair.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FalakAlkhair.Infrastructure.Persistence.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AuctionNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => new { x.CompanyId, x.AuctionNumber }).IsUnique();
        builder.Property(x => x.ExternalAuctionId).HasMaxLength(100);
        builder.Property(x => x.ExternalPlatformUrl).HasMaxLength(500);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);

        builder.Property(x => x.StartingPrice).HasPrecision(18, 2);
        builder.Property(x => x.ReservePrice).HasPrecision(18, 2);
        builder.Property(x => x.DepositAmount).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasPrecision(5, 2);
        builder.Property(x => x.VatPercentage).HasPrecision(5, 2);
        builder.Property(x => x.FinalPrice).HasPrecision(18, 2);
        builder.Property(x => x.CurrentBidAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.WinnerBuyer).WithMany().HasForeignKey(x => x.WinnerBuyerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ExternalAuctionId);
        builder.HasIndex(x => x.CompanyId);
    }
}

public class AuctionAuditLogConfiguration : IEntityTypeConfiguration<AuctionAuditLog>
{
    public void Configure(EntityTypeBuilder<AuctionAuditLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Payload).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SourceIp).HasMaxLength(50);

        builder.HasOne(x => x.Auction).WithMany(a => a.AuditLogs).HasForeignKey(x => x.AuctionId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.AuctionId);
        builder.HasIndex(x => x.CompanyId);
    }
}
