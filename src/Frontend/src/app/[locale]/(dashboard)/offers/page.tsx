"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getOffers } from "@/lib/api/offers";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { OfferDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Pending", "Accepted", "Rejected", "Expired", "Withdrawn"];

export default function OffersPage() {
  const t = useTranslations("offers");
  const query = usePaginatedQuery<OfferDto>(getOffers);

  const columns: Column<OfferDto>[] = [
    { key: "number", header: t("number"), render: (r) => r.offerNumber },
    { key: "buyer", header: t("buyer"), render: (r) => r.buyerNameAr },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "amount", header: t("amount"), render: (r) => r.amount.toLocaleString() },
    { key: "offerDate", header: t("offerDate"), render: (r) => new Date(r.offerDate).toLocaleDateString() },
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
