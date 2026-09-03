import { apiClient } from "@/lib/api-client";
import type { ApiResponse, DocumentDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getDocuments(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<DocumentDto>>>("/documents", { params });
  return data.data;
}

export async function getDocumentsByEntity(entityType: string, entityId: string) {
  const { data } = await apiClient.get<ApiResponse<DocumentDto[]>>("/documents/by-entity", {
    params: { entityType, entityId },
  });
  return data.data;
}

export interface UploadDocumentParams {
  file: File;
  documentType: string;
  entityType: string;
  entityId: string;
  notes?: string;
  expiryDate?: string;
}

export async function uploadDocument(params: UploadDocumentParams) {
  const form = new FormData();
  form.append("File", params.file);
  form.append("DocumentType", params.documentType);
  form.append("EntityType", params.entityType);
  form.append("EntityId", params.entityId);
  if (params.notes) form.append("Notes", params.notes);
  if (params.expiryDate) form.append("ExpiryDate", params.expiryDate);

  const { data } = await apiClient.post<ApiResponse<string>>("/documents", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data.data;
}

/**
 * التنزيل عبر axios (وليس رابط <a href> مباشر) لأن نقطة التنزيل محمية بـ JWT
 * في الترويسة، لا عبر معامل URL أو كوكي — apiClient يرفق التوكن تلقائيًا.
 */
export async function downloadDocument(id: string, fileName: string) {
  const response = await apiClient.get(`/documents/${id}/download`, { responseType: "blob" });
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}

export async function deleteDocument(id: string) {
  await apiClient.delete(`/documents/${id}`);
}
