import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, UnitDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getUnits(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<UnitDto>>>("/units", { params });
  return data.data;
}
