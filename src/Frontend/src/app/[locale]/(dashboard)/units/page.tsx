"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getUnits } from "@/lib/api/units";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { UnitDto } from "@/lib/types";

export default function UnitsPage() {
  const t = useTranslations("units");
  const query = usePaginatedQuery<UnitDto>(getUnits);

  const columns: Column<UnitDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.unitCode },
    { key: "number", header: t("number"), render: (r) => r.unitNumber },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "area", header: t("area"), render: (r) => r.area ?? "-" },
    { key: "rentalPrice", header: t("rentalPrice"), render: (r) => r.rentalPrice ?? "-" },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("title")}</h1>

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
