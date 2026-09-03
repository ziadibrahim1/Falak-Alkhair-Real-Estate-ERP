"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getLeads } from "@/lib/api/leads";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { LeadDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "New", "Contacted", "Qualified", "Converted", "Lost"];
const PRIORITY_LABEL_INDEX = ["", "Low", "Medium", "High"];

export default function LeadsPage() {
  const t = useTranslations("leads");
  const query = usePaginatedQuery<LeadDto>(getLeads);

  const columns: Column<LeadDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.leadCode },
    { key: "name", header: t("name"), render: (r) => r.nameAr },
    { key: "mobile", header: t("mobile"), render: (r) => r.mobile },
    { key: "agent", header: t("assignedAgent"), render: (r) => r.assignedAgentNameAr ?? "-" },
    { key: "priority", header: t("priority"), render: (r) => PRIORITY_LABEL_INDEX[r.priority] ?? r.priority },
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
