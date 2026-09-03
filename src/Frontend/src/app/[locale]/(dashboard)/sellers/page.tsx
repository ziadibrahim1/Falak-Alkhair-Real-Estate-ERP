"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getSellers } from "@/lib/api/sellers";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { SellerDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Draft", "Active", "Expired", "Cancelled", "Completed"];

export default function SellersPage() {
  const t = useTranslations("sellers");
  const query = usePaginatedQuery<SellerDto>(getSellers);

  const columns: Column<SellerDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.sellerCode },
    { key: "owner", header: t("owner"), render: (r) => r.ownerNameAr },
    { key: "property", header: t("property"), render: (r) => r.propertyName ?? "-" },
    { key: "askingPrice", header: t("askingPrice"), render: (r) => r.askingPrice.toLocaleString() },
    { key: "commission", header: t("commission"), render: (r) => `${r.commissionPercentage}%` },
    { key: "agent", header: t("assignedAgent"), render: (r) => r.assignedAgentNameAr ?? "-" },
    { key: "status", header: t("status"), render: (r) => STATUS_LABEL_INDEX[r.mandateStatus] ?? r.mandateStatus },
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
