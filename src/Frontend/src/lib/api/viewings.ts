import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, ViewingDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getViewings(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<ViewingDto>>>("/viewings", { params });
  return data.data;
}
