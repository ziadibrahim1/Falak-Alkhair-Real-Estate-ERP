"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { getDashboardStats } from "@/lib/api/dashboard";
import type { DashboardStatsDto } from "@/lib/types";

interface StatCardProps {
  label: string;
  value: number | string;
}

function StatCard({ label, value }: StatCardProps) {
  return (
    <div className="rounded-2xl border border-border bg-surface p-5 shadow-sm">
      <p className="text-sm text-gray-500">{label}</p>
      <p className="mt-2 text-3xl font-bold text-brand-dark">{value}</p>
    </div>
  );
}

function StatSection({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="mb-8">
      <h2 className="mb-3 text-lg font-semibold text-gray-700">{title}</h2>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">{children}</div>
    </section>
  );
}

/** لوحة تحكم تعرض إحصائيات حقيقية مجمَّعة على الخادم بأمر واحد (GET /api/dashboard/stats). */
export default function DashboardPage() {
  const t = useTranslations("dashboard");
  const [stats, setStats] = useState<DashboardStatsDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const data = await getDashboardStats();
        if (!cancelled) setStats(data);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  const v = (value: number | undefined) => (loading || value === undefined ? "…" : value.toLocaleString());
  const money = (value: number | undefined) => (loading || value === undefined ? "…" : `${value.toLocaleString()} ر.س`);

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("title")}</h1>

      <StatSection title={t("sectionProperties")}>
        <StatCard label={t("totalProperties")} value={v(stats?.totalProperties)} />
        <StatCard label={t("totalUnits")} value={v(stats?.totalUnits)} />
        <StatCard label={t("availableUnits")} value={v(stats?.availableUnits)} />
        <StatCard label={t("rentedUnits")} value={v(stats?.rentedUnits)} />
      </StatSection>

      <StatSection title={t("sectionLeasing")}>
        <StatCard label={t("activeLeases")} value={v(stats?.activeLeases)} />
        <StatCard label={t("activeLeasesAnnualRentValue")} value={money(stats?.activeLeasesAnnualRentValue)} />
        <StatCard label={t("overduePaymentsCount")} value={v(stats?.overduePaymentsCount)} />
        <StatCard label={t("overduePaymentsAmount")} value={money(stats?.overduePaymentsAmount)} />
      </StatSection>

      <StatSection title={t("sectionSales")}>
        <StatCard label={t("salesPipelineCount")} value={v(stats?.salesPipelineCount)} />
        <StatCard label={t("salesPipelineValue")} value={money(stats?.salesPipelineValue)} />
        <StatCard label={t("salesCompletedThisMonth")} value={v(stats?.salesCompletedThisMonth)} />
        <StatCard label={t("pendingCommissionsAmount")} value={money(stats?.pendingCommissionsAmount)} />
      </StatSection>

      <StatSection title={t("sectionOperations")}>
        <StatCard label={t("openMaintenanceRequests")} value={v(stats?.openMaintenanceRequests)} />
        <StatCard label={t("urgentMaintenanceRequests")} value={v(stats?.urgentMaintenanceRequests)} />
        <StatCard label={t("upcomingAuctions")} value={v(stats?.upcomingAuctions)} />
        <StatCard label={t("liveAuctions")} value={v(stats?.liveAuctions)} />
      </StatSection>

      <StatSection title={t("sectionCrm")}>
        <StatCard label={t("totalLeads")} value={v(stats?.totalLeads)} />
        <StatCard label={t("newLeadsThisMonth")} value={v(stats?.newLeadsThisMonth)} />
      </StatSection>
    </div>
  );
}
