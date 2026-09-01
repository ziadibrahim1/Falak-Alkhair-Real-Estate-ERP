"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getProperties } from "@/lib/api/properties";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { PropertyDto } from "@/lib/types";

export default function PropertiesPage() {
  const t = useTranslations("properties");
  const query = usePaginatedQuery<PropertyDto>(getProperties);

  const columns: Column<PropertyDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.propertyCode },
    { key: "name", header: t("name"), render: (r) => r.propertyName },
    { key: "owner", header: t("owner"), render: (r) => r.ownerNameAr },
    { key: "city", header: t("city"), render: (r) => r.city ?? "-" },
    { key: "units", header: t("unitsCount"), render: (r) => `${r.availableUnitsCount}/${r.unitsCount}` },
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
