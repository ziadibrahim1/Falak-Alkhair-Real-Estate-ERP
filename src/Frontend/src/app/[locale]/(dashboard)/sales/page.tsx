"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getSales } from "@/lib/api/sales";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { SaleDto } from "@/lib/types";

const STAGE_LABEL_INDEX = [
  "", "Lead", "Qualified", "Viewing", "Offer", "Negotiation", "Reserved", "Contract", "Payment", "Completed", "Cancelled",
];

export default function SalesPage() {
  const t = useTranslations("sales");
  const query = usePaginatedQuery<SaleDto>(getSales);

  const columns: Column<SaleDto>[] = [
    { key: "number", header: t("number"), render: (r) => r.saleNumber },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "buyer", header: t("buyer"), render: (r) => r.buyerNameAr },
    { key: "agent", header: t("agent"), render: (r) => r.agentNameAr ?? "-" },
    { key: "finalPrice", header: t("finalPrice"), render: (r) => r.finalPrice.toLocaleString() },
    { key: "stage", header: t("stage"), render: (r) => STAGE_LABEL_INDEX[r.stage] ?? r.stage },
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
