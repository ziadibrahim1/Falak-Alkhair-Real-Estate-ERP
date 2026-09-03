using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;

namespace FalakAlkhair.Application.Auctions.DTOs;

public class AuctionDto
{
    public Guid Id { get; set; }
    public string AuctionNumber { get; set; } = default!;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = default!;
    public Guid? UnitId { get; set; }
    public string? UnitNumber { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;
    public Guid? AgentId { get; set; }
    public string? AgentNameAr { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal CommissionPercentage { get; set; }
    public AuctionStatus Status { get; set; }
    public Guid? WinnerBuyerId { get; set; }
    public string? WinnerBuyerNameAr { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? ExternalAuctionId { get; set; }
    public decimal? CurrentBidAmount { get; set; }
    public int BidsCount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AuctionDto FromEntity(Auction auction) => new()
    {
        Id = auction.Id,
        AuctionNumber = auction.AuctionNumber,
        PropertyId = auction.PropertyId,
        PropertyName = auction.Property?.PropertyName ?? string.Empty,
        UnitId = auction.UnitId,
        UnitNumber = auction.Unit?.UnitNumber,
        OwnerId = auction.OwnerId,
        OwnerNameAr = auction.Owner?.NameAr ?? string.Empty,
        AgentId = auction.AgentId,
        AgentNameAr = auction.Agent?.NameAr,
        StartDate = auction.StartDate,
        EndDate = auction.EndDate,
        StartingPrice = auction.StartingPrice,
        ReservePrice = auction.ReservePrice,
        DepositAmount = auction.DepositAmount,
        CommissionPercentage = auction.CommissionPercentage,
        Status = auction.Status,
        WinnerBuyerId = auction.WinnerBuyerId,
        WinnerBuyerNameAr = auction.WinnerBuyer?.NameAr,
        FinalPrice = auction.FinalPrice,
        ExternalAuctionId = auction.ExternalAuctionId,
        CurrentBidAmount = auction.CurrentBidAmount,
        BidsCount = auction.BidsCount,
        Notes = auction.Notes,
        CreatedAt = auction.CreatedAt
    };
}

public class AuctionAuditLogDto
{
    public Guid Id { get; set; }
    public AuctionEventType EventType { get; set; }
    public string? Notes { get; set; }
    public string? SourceIp { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? CreatedBy { get; set; }

    public static AuctionAuditLogDto FromEntity(AuctionAuditLog log) => new()
    {
        Id = log.Id,
        EventType = log.EventType,
        Notes = log.Notes,
        SourceIp = log.SourceIp,
        OccurredAt = log.OccurredAt,
        CreatedBy = log.CreatedBy
    };
}
