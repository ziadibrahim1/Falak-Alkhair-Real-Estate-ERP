"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getMaintenanceQuotations } from "@/lib/api/maintenanceQuotations";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { MaintenanceQuotationDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Pending", "Approved", "Rejected"];

export default function QuotationsPage() {
  const t = useTranslations("quotations");
  const query = usePaginatedQuery<MaintenanceQuotationDto>(getMaintenanceQuotations);

  const columns: Column<MaintenanceQuotationDto>[] = [
    { key: "number", header: t("number"), render: (r) => r.quotationNumber },
    { key: "request", header: t("request"), render: (r) => r.maintenanceRequestNumber },
    { key: "vendor", header: t("vendor"), render: (r) => r.vendorNameAr },
    { key: "total", header: t("total"), render: (r) => r.totalAmount.toLocaleString() },
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
