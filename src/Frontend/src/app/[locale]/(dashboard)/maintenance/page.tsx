"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getMaintenanceRequests } from "@/lib/api/maintenanceRequests";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { MaintenanceRequestDto } from "@/lib/types";

const STATUS_LABEL_INDEX = [
  "", "New", "Assigned", "Inspection", "Quotation", "WaitingApproval", "Approved", "InProgress", "WaitingParts", "Completed", "Cancelled",
];
const PRIORITY_LABEL_INDEX = ["", "Low", "Medium", "High", "Critical"];

export default function MaintenancePage() {
  const t = useTranslations("maintenance");
  const query = usePaginatedQuery<MaintenanceRequestDto>(getMaintenanceRequests);

  const columns: Column<MaintenanceRequestDto>[] = [
    { key: "number", header: t("number"), render: (r) => r.requestNumber },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "priority", header: t("priority"), render: (r) => PRIORITY_LABEL_INDEX[r.priority] ?? r.priority },
    { key: "employee", header: t("employee"), render: (r) => r.assignedEmployeeNameAr ?? "-" },
    { key: "vendor", header: t("vendor"), render: (r) => r.assignedVendorNameAr ?? "-" },
    { key: "cost", header: t("estimatedCost"), render: (r) => (r.estimatedCost ? r.estimatedCost.toLocaleString() : "-") },
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
