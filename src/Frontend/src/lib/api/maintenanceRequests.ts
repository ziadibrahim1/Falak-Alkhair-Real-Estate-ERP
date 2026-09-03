import { apiClient } from "@/lib/api-client";
import type { ApiResponse, MaintenanceRequestDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getMaintenanceRequests(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<MaintenanceRequestDto>>>("/maintenancerequests", { params });
  return data.data;
}
