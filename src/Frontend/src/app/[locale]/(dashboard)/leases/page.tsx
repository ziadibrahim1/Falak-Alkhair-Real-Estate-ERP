"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { activateLease, getLeases, terminateLease } from "@/lib/api/leases";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { LeaseDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Draft", "PendingApproval", "Active", "Terminated", "Cancelled"];

export default function LeasesPage() {
  const t = useTranslations("leases");
  const query = usePaginatedQuery<LeaseDto>(getLeases);

  const handleActivate = async (id: string) => {
    await activateLease(id);
    query.setPageNumber(query.pageNumber);
  };

  const handleTerminate = async (id: string) => {
    await terminateLease(id);
    query.setPageNumber(query.pageNumber);
  };

  const columns: Column<LeaseDto>[] = [
    { key: "leaseNumber", header: t("leaseNumber"), render: (r) => r.leaseNumber },
    { key: "tenant", header: t("tenant"), render: (r) => r.tenantNameAr },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "annualRent", header: t("annualRent"), render: (r) => r.annualRentAmount.toLocaleString() },
    { key: "endDate", header: t("endDate"), render: (r) => new Date(r.endDate).toLocaleDateString() },
    { key: "status", header: t("status"), render: (r) => STATUS_LABEL_INDEX[r.status] ?? r.status },
    {
      key: "actions",
      header: "",
      render: (r) => {
        if (r.status === 1 || r.status === 2) {
          return (
            <button
              onClick={() => handleActivate(r.id)}
              className="rounded-md bg-brand px-3 py-1 text-xs font-medium text-white hover:bg-brand-dark"
            >
              {t("activate")}
            </button>
          );
        }
        if (r.status === 3) {
          return (
            <button
              onClick={() => handleTerminate(r.id)}
              className="rounded-md bg-red-600 px-3 py-1 text-xs font-medium text-white hover:bg-red-700"
            >
              {t("terminate")}
            </button>
          );
        }
        return null;
      },
    },
  ];

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("title")}</h1>
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
