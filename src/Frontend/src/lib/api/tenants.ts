import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, TenantDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getTenants(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<TenantDto>>>("/tenants", { params });
  return data.data;
}
