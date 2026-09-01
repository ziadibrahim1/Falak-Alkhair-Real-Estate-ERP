"use client";

import { useTranslations } from "next-intl";
import type { ReactNode } from "react";

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  loading?: boolean;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  searchTerm: string;
  onSearchChange: (value: string) => void;
  toolbar?: ReactNode;
}

/**
 * جدول بيانات عام يدعم: البحث، التقسيم لصفحات (Server-side Pagination)،
 * وحالتي التحميل/عدم وجود بيانات — يُستخدم في كل شاشات القوائم بالنظام
 * (العقارات، الوحدات، الملاك، العقود ...) بدل تكرار نفس المنطق في كل صفحة.
 */
export function DataTable<T>({
  columns,
  rows,
  rowKey,
  loading,
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  searchTerm,
  onSearchChange,
  toolbar,
}: DataTableProps<T>) {
  const t = useTranslations("common");
  const from = totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="rounded-2xl border border-border bg-surface shadow-sm">
      <div className="flex flex-col gap-3 border-b border-border p-4 sm:flex-row sm:items-center sm:justify-between">
        <input
          type="search"
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          placeholder={t("search")}
          className="w-full rounded-lg border border-border px-3 py-2 text-sm outline-none focus:border-brand sm:max-w-xs"
        />
        {toolbar}
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border bg-gray-50 text-start">
              {columns.map((col) => (
                <th key={col.key} className="px-4 py-3 text-start font-medium text-gray-600">
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-8 text-center text-gray-500">
                  {t("loading")}
                </td>
              </tr>
            ) : rows.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-8 text-center text-gray-500">
                  {t("noData")}
                </td>
              </tr>
            ) : (
              rows.map((row) => (
                <tr key={rowKey(row)} className="border-b border-border last:border-0 hover:bg-gray-50">
                  {columns.map((col) => (
                    <td key={col.key} className="px-4 py-3">
                      {col.render(row)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="flex flex-col items-center justify-between gap-3 p-4 sm:flex-row">
        <span className="text-xs text-gray-500">
          {t("totalRecords")}: {totalCount} ({from}-{to})
        </span>
        <div className="flex items-center gap-2">
          <button
            className="rounded-md border border-border px-3 py-1 text-sm disabled:opacity-40"
            disabled={pageNumber <= 1}
            onClick={() => onPageChange(pageNumber - 1)}
          >
            {"‹"}
          </button>
          <span className="text-sm text-gray-600">
            {t("page")} {pageNumber} {t("of")} {Math.max(totalPages, 1)}
          </span>
          <button
            className="rounded-md border border-border px-3 py-1 text-sm disabled:opacity-40"
            disabled={pageNumber >= totalPages}
            onClick={() => onPageChange(pageNumber + 1)}
          >
            {"›"}
          </button>
        </div>
      </div>
    </div>
  );
}
