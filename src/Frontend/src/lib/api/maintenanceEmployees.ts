import { apiClient } from "@/lib/api-client";
import type { ApiResponse, MaintenanceEmployeeDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getMaintenanceEmployees(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<MaintenanceEmployeeDto>>>("/maintenanceemployees", { params });
  return data.data;
}
