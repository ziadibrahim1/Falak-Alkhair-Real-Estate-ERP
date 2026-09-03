import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, SellerDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getSellers(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<SellerDto>>>("/sellers", { params });
  return data.data;
}
