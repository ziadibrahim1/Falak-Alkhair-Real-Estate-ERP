import { apiClient } from "@/lib/api-client";
import type { ApiResponse, OfferDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getOffers(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<OfferDto>>>("/offers", { params });
  return data.data;
}
