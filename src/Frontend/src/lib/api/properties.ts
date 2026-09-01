import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, PropertyDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getProperties(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<PropertyDto>>>("/properties", { params });
  return data.data;
}
