import { apiClient } from "@/lib/api-client";
import type { ApiResponse, AuctionDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getAuctions(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<AuctionDto>>>("/auctions", { params });
  return data.data;
}
