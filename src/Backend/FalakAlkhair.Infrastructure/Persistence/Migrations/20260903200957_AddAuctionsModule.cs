using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FalakAlkhair.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuctionsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AuctionId",
                table: "Commissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Auctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuctionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReservePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VatPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WinnerBuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExternalAuctionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalPlatformUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentBidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BidsCount = table.Column<int>(type: "int", nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auctions_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auctions_Buyers_WinnerBuyerId",
                        column: x => x.WinnerBuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auctions_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auctions_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auctions_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auctions_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuctionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuctionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionAuditLogs_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commissions_AuctionId",
                table: "Commissions",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionAuditLogs_AuctionId",
                table: "AuctionAuditLogs",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionAuditLogs_CompanyId",
                table: "AuctionAuditLogs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionAuditLogs_EventType",
                table: "AuctionAuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_AgentId",
                table: "Auctions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_CompanyId",
                table: "Auctions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_CompanyId_AuctionNumber",
                table: "Auctions",
                columns: new[] { "CompanyId", "AuctionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_ExternalAuctionId",
                table: "Auctions",
                column: "ExternalAuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_OwnerId",
                table: "Auctions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_PropertyId",
                table: "Auctions",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_SellerId",
                table: "Auctions",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_Status",
                table: "Auctions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_UnitId",
                table: "Auctions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_WinnerBuyerId",
                table: "Auctions",
                column: "WinnerBuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commissions_Auctions_AuctionId",
                table: "Commissions",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commissions_Auctions_AuctionId",
                table: "Commissions");

            migrationBuilder.DropTable(
                name: "AuctionAuditLogs");

            migrationBuilder.DropTable(
                name: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Commissions_AuctionId",
                table: "Commissions");

            migrationBuilder.DropColumn(
                name: "AuctionId",
                table: "Commissions");
        }
    }
}
