import { apiClient } from "@/lib/api-client";
import type { ApiResponse, MaintenanceQuotationDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getMaintenanceQuotations(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<MaintenanceQuotationDto>>>("/maintenancequotations", { params });
  return data.data;
}
