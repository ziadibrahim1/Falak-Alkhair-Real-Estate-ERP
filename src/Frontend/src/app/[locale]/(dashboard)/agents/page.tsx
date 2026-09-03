"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getAgents } from "@/lib/api/agents";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { AgentDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Active", "Suspended", "Inactive"];

export default function AgentsPage() {
  const t = useTranslations("agents");
  const tCommon = useTranslations("common");
  const query = usePaginatedQuery<AgentDto>(getAgents);

  const columns: Column<AgentDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.agentCode },
    { key: "name", header: t("name"), render: (r) => r.nameAr },
    { key: "mobile", header: t("mobile"), render: (r) => r.mobile },
    { key: "falLicense", header: t("falLicense"), render: (r) => r.falLicenseNumber ?? "-" },
    { key: "commission", header: t("defaultCommission"), render: (r) => `${r.defaultCommissionPercentage}%` },
    { key: "commissionsCount", header: t("commissionsCount"), render: (r) => r.commissionsCount },
    {
      key: "status",
      header: tCommon("status"),
      render: (r) => STATUS_LABEL_INDEX[r.status] ?? r.status,
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
