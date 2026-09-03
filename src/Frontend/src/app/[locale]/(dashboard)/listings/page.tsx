"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getListings, publishListing } from "@/lib/api/listings";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { ListingDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Draft", "PendingReview", "Published", "Paused", "Expired", "Sold", "Rented"];
const TYPE_LABEL_INDEX = ["", "ForSale", "ForRent"];

export default function ListingsPage() {
  const t = useTranslations("listings");
  const query = usePaginatedQuery<ListingDto>(getListings);

  const handlePublish = async (id: string) => {
    await publishListing(id);
    query.setPageNumber(query.pageNumber);
  };

  const columns: Column<ListingDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.listingCode },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "unit", header: t("unit"), render: (r) => r.unitNumber },
    { key: "type", header: t("type"), render: (r) => TYPE_LABEL_INDEX[r.listingType] ?? r.listingType },
    { key: "price", header: t("price"), render: (r) => r.price.toLocaleString() },
    { key: "agent", header: t("agent"), render: (r) => r.agentNameAr ?? "-" },
    { key: "status", header: t("status"), render: (r) => STATUS_LABEL_INDEX[r.status] ?? r.status },
    {
      key: "actions",
      header: "",
      render: (r) =>
        r.status === 1 || r.status === 2 || r.status === 4 ? (
          <button
            onClick={() => handlePublish(r.id)}
            className="rounded-md bg-brand px-3 py-1 text-xs font-medium text-white hover:bg-brand-dark"
          >
            {t("publish")}
          </button>
        ) : null,
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
