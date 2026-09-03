"use client";

import { useCallback, useEffect, useState } from "react";
import { useTranslations } from "next-intl";
import { DataTable, type Column } from "@/components/ui/DataTable";
import { deleteDocument, downloadDocument, getDocuments, uploadDocument } from "@/lib/api/documents";
import type { DocumentDto, PaginatedResult } from "@/lib/types";

const ENTITY_TYPES = ["Property", "Unit", "Owner", "Tenant", "Lease", "Agent", "Buyer", "Seller", "Auction"];

function formatFileSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function DocumentsPage() {
  const t = useTranslations("documents");
  const tCommon = useTranslations("common");

  const [pageNumber, setPageNumber] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [data, setData] = useState<PaginatedResult<DocumentDto> | null>(null);
  const [loading, setLoading] = useState(true);

  const [entityType, setEntityType] = useState(ENTITY_TYPES[0]);
  const [entityId, setEntityId] = useState("");
  const [documentType, setDocumentType] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await getDocuments({ pageNumber, pageSize: 10, searchTerm: searchTerm || undefined });
      setData(result);
    } finally {
      setLoading(false);
    }
  }, [pageNumber, searchTerm]);

  useEffect(() => {
    // مؤشر التحميل هنا مرتبط بجلب بيانات خارجي (API)، وهو النمط الموصى به فعليًا
    // لعرض Loading أثناء fetch — نتجاوز القاعدة التجريبية عمدًا (كما في usePaginatedQuery).
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file || !documentType || !entityId) return;
    setUploading(true);
    setUploadError(null);
    try {
      await uploadDocument({ file, documentType, entityType, entityId });
      setFile(null);
      setDocumentType("");
      setEntityId("");
      (e.target as HTMLFormElement).reset();
      await load();
    } catch (err) {
      const message =
        (err as { response?: { data?: { errors?: string[]; message?: string } } })?.response?.data?.errors?.[0] ??
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "";
      setUploadError(message || tCommon("error"));
    } finally {
      setUploading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm(t("confirmDelete"))) return;
    await deleteDocument(id);
    await load();
  };

  const columns: Column<DocumentDto>[] = [
    { key: "fileName", header: t("fileName"), render: (r) => r.fileName },
    { key: "documentType", header: t("documentType"), render: (r) => r.documentType },
    { key: "entityType", header: t("entityType"), render: (r) => r.entityType },
    { key: "fileSize", header: t("fileSize"), render: (r) => formatFileSize(r.fileSize) },
    { key: "uploadedAt", header: t("uploadedAt"), render: (r) => new Date(r.createdAt).toLocaleDateString() },
    {
      key: "actions",
      header: "",
      render: (r) => (
        <div className="flex gap-3">
          <button onClick={() => downloadDocument(r.id, r.fileName)} className="text-brand hover:underline">
            {t("download")}
          </button>
          <button onClick={() => handleDelete(r.id)} className="text-danger hover:underline">
            {t("delete")}
          </button>
        </div>
      ),
    },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">{t("title")}</h1>

      <form onSubmit={handleUpload} className="mb-6 flex flex-wrap items-end gap-3 rounded-2xl border border-border bg-surface p-4 shadow-sm">
        <div>
          <label className="mb-1 block text-xs text-gray-500">{t("entityType")}</label>
          <select
            value={entityType}
            onChange={(e) => setEntityType(e.target.value)}
            className="rounded-lg border border-border px-3 py-2 text-sm outline-none focus:border-brand"
          >
            {ENTITY_TYPES.map((et) => (
              <option key={et} value={et}>
                {et}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className="mb-1 block text-xs text-gray-500">Entity ID</label>
          <input
            value={entityId}
            onChange={(e) => setEntityId(e.target.value)}
            placeholder="00000000-0000-0000-0000-000000000000"
            className="w-72 rounded-lg border border-border px-3 py-2 text-sm outline-none focus:border-brand"
            required
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-gray-500">{t("documentType")}</label>
          <input
            value={documentType}
            onChange={(e) => setDocumentType(e.target.value)}
            className="rounded-lg border border-border px-3 py-2 text-sm outline-none focus:border-brand"
            required
          />
        </div>
        <div>
          <label className="mb-1 block text-xs text-gray-500">{t("selectFile")}</label>
          <input
            type="file"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            className="text-sm"
            required
          />
        </div>
        <button
          type="submit"
          disabled={uploading}
          className="rounded-lg bg-brand px-4 py-2 text-sm font-medium text-white hover:bg-brand-dark disabled:opacity-50"
        >
          {t("upload")}
        </button>
        {uploadError && <p className="w-full text-sm text-danger">{uploadError}</p>}
      </form>

      <DataTable
        columns={columns}
        rows={data?.items ?? []}
        rowKey={(r) => r.id}
        loading={loading}
        pageNumber={pageNumber}
        pageSize={10}
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
