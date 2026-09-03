import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, SaleDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getSales(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<SaleDto>>>("/sales", { params });
  return data.data;
}
