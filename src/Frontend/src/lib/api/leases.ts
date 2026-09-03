import { apiClient } from "@/lib/api-client";
import type { ApiResponse, LeaseDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getLeases(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<LeaseDto>>>("/leases", { params });
  return data.data;
}

export async function activateLease(id: string) {
  await apiClient.post(`/leases/${id}/activate`);
}

export async function terminateLease(id: string, reason?: string) {
  await apiClient.post(`/leases/${id}/terminate`, { reason });
}
