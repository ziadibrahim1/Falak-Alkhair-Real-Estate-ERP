import { apiClient } from "@/lib/api-client";
import type { ApiResponse, MarketingCampaignDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getCampaigns(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<MarketingCampaignDto>>>("/marketing/campaigns", { params });
  return data.data;
}
