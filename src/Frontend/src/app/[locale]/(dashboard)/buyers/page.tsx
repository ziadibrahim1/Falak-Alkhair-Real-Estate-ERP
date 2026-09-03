"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getBuyers } from "@/lib/api/buyers";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { BuyerDto } from "@/lib/types";

export default function BuyersPage() {
  const t = useTranslations("buyers");
  const tCommon = useTranslations("common");
  const query = usePaginatedQuery<BuyerDto>(getBuyers);

  const columns: Column<BuyerDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.buyerCode },
    { key: "name", header: t("name"), render: (r) => r.nameAr },
    { key: "mobile", header: t("mobile"), render: (r) => r.mobile },
    { key: "budget", header: t("budget"), render: (r) => (r.budget ? r.budget.toLocaleString() : "-") },
    { key: "city", header: t("preferredCity"), render: (r) => r.preferredCity ?? "-" },
    { key: "agent", header: t("assignedAgent"), render: (r) => r.assignedAgentNameAr ?? "-" },
    {
      key: "status",
      header: tCommon("status"),
      render: (r) => (
        <span
          className={`rounded-full px-2 py-0.5 text-xs ${
            r.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"
          }`}
        >
          {r.isActive ? "●" : "○"}
        </span>
      ),
    },
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
