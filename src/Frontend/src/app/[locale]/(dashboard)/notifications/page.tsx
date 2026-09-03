"use client";

import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { getNotifications, markAllNotificationsRead, markNotificationRead } from "@/lib/api/notifications";
import type { NotificationDto, PaginatedResult } from "@/lib/types";

export default function NotificationsPage() {
  const t = useTranslations("notifications");
  const [pageNumber, setPageNumber] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [data, setData] = useState<PaginatedResult<NotificationDto> | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await getNotifications({ pageNumber, pageSize: 20, searchTerm: searchTerm || undefined });
      setData(result);
    } finally {
      setLoading(false);
    }
  }, [pageNumber, searchTerm]);

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);

  const handleMarkRead = async (id: string) => {
    await markNotificationRead(id);
    setData((prev) => (prev ? { ...prev, items: prev.items.map((n) => (n.id === id ? { ...n, isRead: true } : n)) } : prev));
  };

  const handleMarkAllRead = async () => {
    await markAllNotificationsRead();
    setData((prev) => (prev ? { ...prev, items: prev.items.map((n) => ({ ...n, isRead: true })) } : prev));
  };

  const columns: Column<NotificationDto>[] = [
    {
      key: "status",
      header: "",
      render: (r) => <span className={`inline-block h-2 w-2 rounded-full ${r.isRead ? "bg-transparent" : "bg-brand"}`} />,
    },
    { key: "title", header: "", render: (r) => <span className="font-medium text-gray-800">{r.title}</span> },
    { key: "message", header: "", render: (r) => <span className="text-gray-500">{r.message}</span> },
    { key: "createdAt", header: "", render: (r) => new Date(r.createdAt).toLocaleString() },
    {
      key: "actions",
      header: "",
      render: (r) => !r.isRead && (
        <button onClick={() => handleMarkRead(r.id)} className="text-brand hover:underline">
          ✓
        </button>
      ),
    },
  ];

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">{t("title")}</h1>
        <button
          onClick={handleMarkAllRead}
          className="rounded-lg border border-border px-4 py-2 text-sm font-medium hover:bg-gray-50"
        >
          {t("markAllRead")}
        </button>
      </div>

      <DataTable
        columns={columns}
        rows={data?.items ?? []}
        rowKey={(r) => r.id}
        loading={loading}
        pageNumber={pageNumber}
        pageSize={20}
        totalCount={data?.totalCount ?? 0}
        totalPages={data?.totalPages ?? 0}
        onPageChange={setPageNumber}
        searchTerm={searchTerm}
        onSearchChange={(v) => {
          setSearchTerm(v);
          setPageNumber(1);
        }}
      />
    </div>
  );
}
