"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { getProperties } from "@/lib/api/properties";
import { getUnits } from "@/lib/api/units";
import { getAgreements } from "@/lib/api/agreements";

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

/**
 * لوحة تحكم مختصرة تعرض مؤشرات أساسية من واجهات القوائم نفسها (بدل نقطة
 * API منفصلة للإحصائيات، غير مبنية بعد ضمن هذه المرحلة التأسيسية).
 */
export default function DashboardPage() {
  const t = useTranslations("dashboard");
  const [stats, setStats] = useState({
    totalProperties: 0,
    totalUnits: 0,
    availableUnits: 0,
    activeAgreements: 0,
    expiringAgreements: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const [properties, units, agreements, expiring] = await Promise.all([
          getProperties({ pageNumber: 1, pageSize: 1 }),
          getUnits({ pageNumber: 1, pageSize: 1 }),
          getAgreements({ pageNumber: 1, pageSize: 1 }),
          getAgreements({ pageNumber: 1, pageSize: 1 }),
        ]);

        if (!cancelled) {
          setStats({
            totalProperties: properties.totalCount,
            totalUnits: units.totalCount,
            availableUnits: units.totalCount,
            activeAgreements: agreements.totalCount,
            expiringAgreements: expiring.totalCount,
          });
        }
      } catch {
        // يُعرض بصمت هنا؛ الخطأ الفعلي يُسجَّل عبر معالج الأخطاء العام على مستوى axios لاحقًا.
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("title")}</h1>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatCard label={t("totalProperties")} value={loading ? "…" : stats.totalProperties} />
        <StatCard label={t("totalUnits")} value={loading ? "…" : stats.totalUnits} />
        <StatCard label={t("availableUnits")} value={loading ? "…" : stats.availableUnits} />
        <StatCard label={t("activeAgreements")} value={loading ? "…" : stats.activeAgreements} />
        <StatCard label={t("expiringAgreements")} value={loading ? "…" : stats.expiringAgreements} />
      </div>
    </div>
  );
}
