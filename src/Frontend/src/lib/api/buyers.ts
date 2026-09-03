import { apiClient } from "@/lib/api-client";
import type { ApiResponse, BuyerDto, PaginatedResult, PropertyMatchDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getBuyers(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<BuyerDto>>>("/buyers", { params });
  return data.data;
}

export async function getBuyerMatches(id: string) {
  const { data } = await apiClient.get<ApiResponse<PropertyMatchDto[]>>(`/buyers/${id}/matches`);
  return data.data;
}
