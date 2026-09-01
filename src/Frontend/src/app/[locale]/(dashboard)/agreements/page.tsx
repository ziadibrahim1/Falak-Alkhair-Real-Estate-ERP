"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { approveAgreement, getAgreements } from "@/lib/api/agreements";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { AgreementDto } from "@/lib/types";

const STATUS_LABEL_INDEX = ["", "Draft", "PendingApproval", "Active", "Expiring", "Expired", "Terminated"];

export default function AgreementsPage() {
  const t = useTranslations("agreements");
  const query = usePaginatedQuery<AgreementDto>(getAgreements);

  const handleApprove = async (id: string) => {
    await approveAgreement(id);
    query.setPageNumber(query.pageNumber);
  };

  const columns: Column<AgreementDto>[] = [
    { key: "contractNumber", header: t("contractNumber"), render: (r) => r.contractNumber },
    { key: "owner", header: t("owner"), render: (r) => r.ownerNameAr },
    { key: "property", header: t("property"), render: (r) => r.propertyName },
    { key: "endDate", header: t("endDate"), render: (r) => new Date(r.endDate).toLocaleDateString() },
    { key: "status", header: t("status"), render: (r) => STATUS_LABEL_INDEX[r.status] ?? r.status },
    {
      key: "actions",
      header: "",
      render: (r) =>
        r.status === 1 || r.status === 2 ? (
          <button
            onClick={() => handleApprove(r.id)}
            className="rounded-md bg-brand px-3 py-1 text-xs font-medium text-white hover:bg-brand-dark"
          >
            {t("approve")}
          </button>
        ) : null,
    },
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
