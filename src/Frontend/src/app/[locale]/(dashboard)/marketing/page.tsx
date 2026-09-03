"use client";

import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getCampaigns } from "@/lib/api/marketing";
import { usePaginatedQuery } from "@/lib/hooks/usePaginatedQuery";
import type { MarketingCampaignDto } from "@/lib/types";

const CHANNEL_LABEL_INDEX: Record<number, string> = {
  1: "Website", 2: "Google", 3: "Instagram", 4: "Snapchat", 5: "TikTok",
  6: "Facebook", 7: "WhatsApp", 8: "Portals", 9: "Offline", 99: "Other",
};

export default function MarketingPage() {
  const t = useTranslations("marketing");
  const tCommon = useTranslations("common");
  const query = usePaginatedQuery<MarketingCampaignDto>(getCampaigns);

  const columns: Column<MarketingCampaignDto>[] = [
    { key: "code", header: t("code"), render: (r) => r.campaignCode },
    { key: "name", header: t("name"), render: (r) => r.name },
    { key: "channel", header: t("channel"), render: (r) => CHANNEL_LABEL_INDEX[r.channel] ?? r.channel },
    { key: "budget", header: t("budget"), render: (r) => r.budget.toLocaleString() },
    { key: "cost", header: t("actualCost"), render: (r) => r.actualCost.toLocaleString() },
    { key: "leads", header: t("leadsCount"), render: (r) => r.leadsCount },
    { key: "conversions", header: t("conversionsCount"), render: (r) => r.conversionsCount },
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
