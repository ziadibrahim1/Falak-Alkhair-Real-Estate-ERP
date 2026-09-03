"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getViewings } from "@/lib/api/viewings";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { ViewingDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Scheduled", "Completed", "Cancelled", "NoShow"];

export default function ViewingsPage() {
  const t = useTranslations("viewings");
  const query = usePaginatedQuery<ViewingDto>(getViewings);

  const columns: Column<ViewingDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.viewingCode },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "with", header: t("withParty"), render: (r) => r.buyerNameAr ?? r.tenantNameAr ?? "-" },
    { key: "agent", header: t("agent"), render: (r) => r.agentNameAr ?? "-" },
    { key: "scheduledAt", header: t("scheduledAt"), render: (r) => new Date(r.scheduledAt).toLocaleString() },
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
