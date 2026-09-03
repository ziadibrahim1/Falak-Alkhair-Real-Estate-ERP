"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getVendors } from "@/lib/api/vendors";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { VendorDto } from "@/lib/types";

export default function VendorsPage() {
  const t = useTranslations("vendors");
  const query = usePaginatedQuery<VendorDto>(getVendors);

  const columns: Column<VendorDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.vendorCode },
    { key: "name", header: t("name"), render: (r) => r.nameAr },
    { key: "contact", header: t("contactPerson"), render: (r) => r.contactPerson ?? "-" },
    { key: "mobile", header: t("mobile"), render: (r) => r.mobile },
    { key: "rating", header: t("rating"), render: (r) => (r.rating ? `${r.rating}/5` : "-") },
    { key: "assigned", header: t("assignedRequestsCount"), render: (r) => r.assignedRequestsCount },
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
