"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getOwners } from "@/lib/api/owners";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { OwnerDto } from "@/lib/types";

export default function OwnersPage() {
  const t = useTranslations("owners");
  const tCommon = useTranslations("common");
  const query = usePaginatedQuery<OwnerDto>(getOwners);

  const columns: Column<OwnerDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.ownerCode },
    { key: "name", header: t("name"), render: (r) => r.nameAr },
    { key: "mobile", header: t("mobile"), render: (r) => r.mobile },
    { key: "properties", header: t("propertiesCount"), render: (r) => r.propertiesCount },
    {
      key: "status",
      header: tCommon("status"),
      render: (r) => (
        <span
          className={`rounded-full px-2 py-0.5 text-xs ${
            r.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"
          }`}
        >
          {r.isActive ? "●" : "○"}
        </span>
      ),
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
