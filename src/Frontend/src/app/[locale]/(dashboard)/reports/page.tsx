"use client";

import { useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import {
  exportReport,
  getReport,
  type CommissionSummaryLineDto,
  type MaintenanceSummaryLineDto,
  type OccupancyLineDto,
  type ReportKey,
  type RentRollLineDto,
  type SalesPipelineStageDto,
} from "@/lib/api/reports";

const REPORT_KEYS: ReportKey[] = ["rent-roll", "sales-pipeline", "commission-summary", "maintenance-summary", "occupancy"];

const REPORT_LABEL_KEY: Record<ReportKey, string> = {
  "rent-roll": "rentRoll",
  "sales-pipeline": "salesPipeline",
  "commission-summary": "commissionSummary",
  "maintenance-summary": "maintenanceSummary",
  occupancy: "occupancy",
};

export default function ReportsPage() {
  const t = useTranslations("reports");
  const [active, setActive] = useState<ReportKey>("rent-roll");
  const [rows, setRows] = useState<unknown[]>([]);
  // يتتبّع التقرير الذي تخصّه rows فعليًا — لتفادي عرض بيانات تقرير سابق (شكل DTO مختلف)
  // للحظة واحدة تحت جدول تقرير جديد بعد تبديل التبويب وقبل اكتمال الجلب الجديد.
  const [loadedFor, setLoadedFor] = useState<ReportKey | null>(null);
  const loading = loadedFor !== active;

  useEffect(() => {
    let cancelled = false;
    getReport(active)
      .then((result) => {
        if (!cancelled) {
          setRows(result);
          setLoadedFor(active);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [active]);

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("title")}</h1>
        <button
          onClick={() => exportReport(active)}
          className="rounded-lg bg-brand px-4 py-2 text-sm font-medium text-white hover:bg-brand-dark"
        >
          {t("export")}
        </button>
      </div>

      <div className="mb-4 flex flex-wrap gap-2">
        {REPORT_KEYS.map((key) => (
          <button
            key={key}
            onClick={() => setActive(key)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium ${
              active === key ? "bg-brand text-white" : "border border-border text-gray-600 hover:bg-gray-50"
            }`}
          >
            {t(REPORT_LABEL_KEY[key])}
          </button>
        ))}
      </div>

      <div className="overflow-x-auto rounded-2xl border border-border bg-surface shadow-sm">
        <ReportTable reportKey={active} rows={rows} loading={loading} noDataLabel={t("noData")} col={(key) => t(`col.${key}`)} />
      </div>
    </div>
  );
}

type ColLabel = (key: string) => string;

function ReportTable({
  reportKey,
  rows,
  loading,
  noDataLabel,
  col,
}: {
  reportKey: ReportKey;
  rows: unknown[];
  loading: boolean;
  noDataLabel: string;
  col: ColLabel;
}) {
  if (loading) {
    return <div className="p-8 text-center text-gray-500">…</div>;
  }
  if (rows.length === 0) {
    return <div className="p-8 text-center text-gray-500">{noDataLabel}</div>;
  }

  switch (reportKey) {
    case "rent-roll":
      return <RentRollTable rows={rows as RentRollLineDto[]} col={col} />;
    case "sales-pipeline":
      return <SalesPipelineTable rows={rows as SalesPipelineStageDto[]} col={col} />;
    case "commission-summary":
      return <CommissionSummaryTable rows={rows as CommissionSummaryLineDto[]} col={col} />;
    case "maintenance-summary":
      return <MaintenanceSummaryTable rows={rows as MaintenanceSummaryLineDto[]} col={col} />;
    case "occupancy":
      return <OccupancyTable rows={rows as OccupancyLineDto[]} col={col} />;
  }
}

const th = "px-4 py-3 text-start text-xs font-medium text-gray-600";
const td = "px-4 py-3";
const trBase = "border-b border-border last:border-0";

function RentRollTable({ rows, col }: { rows: RentRollLineDto[]; col: ColLabel }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="bg-gray-50">
          <th className={th}>{col("lease")}</th><th className={th}>{col("property")}</th><th className={th}>{col("unit")}</th><th className={th}>{col("tenant")}</th>
          <th className={th}>{col("startDate")}</th><th className={th}>{col("endDate")}</th><th className={th}>{col("annualRent")}</th><th className={th}>{col("nextDue")}</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.leaseNumber} className={trBase}>
            <td className={td}>{r.leaseNumber}</td><td className={td}>{r.propertyName}</td><td className={td}>{r.unitNumber}</td>
            <td className={td}>{r.tenantNameAr}</td><td className={td}>{new Date(r.startDate).toLocaleDateString()}</td>
            <td className={td}>{new Date(r.endDate).toLocaleDateString()}</td><td className={td}>{r.annualRentAmount.toLocaleString()}</td>
            <td className={td}>{r.nextDueDate ? new Date(r.nextDueDate).toLocaleDateString() : "-"}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function SalesPipelineTable({ rows, col }: { rows: SalesPipelineStageDto[]; col: ColLabel }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="bg-gray-50">
          <th className={th}>{col("stage")}</th><th className={th}>{col("count")}</th><th className={th}>{col("totalAskingValue")}</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.stage} className={trBase}>
            <td className={td}>{r.stage}</td><td className={td}>{r.count}</td><td className={td}>{r.totalAskingValue.toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function CommissionSummaryTable({ rows, col }: { rows: CommissionSummaryLineDto[]; col: ColLabel }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="bg-gray-50">
          <th className={th}>{col("agent")}</th><th className={th}>{col("count")}</th><th className={th}>{col("pending")}</th>
          <th className={th}>{col("approved")}</th><th className={th}>{col("paid")}</th><th className={th}>{col("totalNet")}</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.agentId} className={trBase}>
            <td className={td}>{r.agentNameAr}</td><td className={td}>{r.commissionsCount}</td>
            <td className={td}>{r.pendingAmount.toLocaleString()}</td><td className={td}>{r.approvedAmount.toLocaleString()}</td>
            <td className={td}>{r.paidAmount.toLocaleString()}</td><td className={td}>{r.totalNetAmount.toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function MaintenanceSummaryTable({ rows, col }: { rows: MaintenanceSummaryLineDto[]; col: ColLabel }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="bg-gray-50">
          <th className={th}>{col("status")}</th><th className={th}>{col("count")}</th><th className={th}>{col("estimatedCost")}</th><th className={th}>{col("actualCost")}</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.status} className={trBase}>
            <td className={td}>{r.status}</td><td className={td}>{r.count}</td>
            <td className={td}>{r.totalEstimatedCost.toLocaleString()}</td><td className={td}>{r.totalActualCost.toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function OccupancyTable({ rows, col }: { rows: OccupancyLineDto[]; col: ColLabel }) {
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="bg-gray-50">
          <th className={th}>{col("property")}</th><th className={th}>{col("totalUnits")}</th><th className={th}>{col("rented")}</th>
          <th className={th}>{col("sold")}</th><th className={th}>{col("available")}</th><th className={th}>{col("occupancyRate")}</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((r) => (
          <tr key={r.propertyId} className={trBase}>
            <td className={td}>{r.propertyName}</td><td className={td}>{r.totalUnits}</td><td className={td}>{r.rentedUnits}</td>
            <td className={td}>{r.soldUnits}</td><td className={td}>{r.availableUnits}</td><td className={td}>{r.occupancyRate}%</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
