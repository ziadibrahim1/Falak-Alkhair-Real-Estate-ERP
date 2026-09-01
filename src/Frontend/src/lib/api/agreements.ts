import { apiClient } from "@/lib/api-client";
import type { AgreementDto, ApiResponse, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getAgreements(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<AgreementDto>>>("/agreements", { params });
  return data.data;
}

export async function approveAgreement(id: string) {
  await apiClient.post(`/agreements/${id}/approve`);
}
