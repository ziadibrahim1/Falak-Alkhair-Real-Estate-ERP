"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getAuctions } from "@/lib/api/auctions";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { AuctionDto } from "@/lib/types";

const STATUS_LABEL_INDEX = [
  "", "Draft", "PendingApproval", "Scheduled", "Published", "Live", "Ended", "Awarded", "Cancelled", "Settled",
];

export default function AuctionsPage() {
  const t = useTranslations("auctions");
  const query = usePaginatedQuery<AuctionDto>(getAuctions);

  const columns: Column<AuctionDto>[] = [
    { key: "number", header: t("number"), render: (r) => r.auctionNumber },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber ?? "-" },
    { key: "agent", header: t("agent"), render: (r) => r.agentNameAr ?? "-" },
    { key: "startingPrice", header: t("startingPrice"), render: (r) => r.startingPrice.toLocaleString() },
    { key: "currentBid", header: t("currentBid"), render: (r) => (r.currentBidAmount ? r.currentBidAmount.toLocaleString() : "-") },
    { key: "finalPrice", header: t("finalPrice"), render: (r) => (r.finalPrice ? r.finalPrice.toLocaleString() : "-") },
    { key: "status", header: t("status"), render: (r) => STATUS_LABEL_INDEX[r.status] ?? r.status },
  ];

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("title")}</h1>
        <button className="rounded-lg bg-brand px-4 py-2 text-sm font-medium text-white hover:bg-brand-dark">
          {t("addNew")}
        </button>
      </div>

      <DataTable
        columns={columns}
        rows={query.rows}
        rowKey={(r) => r.id}
        loading={query.loading}
        pageNumber={query.pageNumber}
        pageSize={query.pageSize}
        totalCount={query.totalCount}
        totalPages={query.totalPages}
        onPageChange={query.setPageNumber}
        searchTerm={query.searchTerm}
        onSearchChange={query.setSearchTerm}
      />
    </div>
  );
}
