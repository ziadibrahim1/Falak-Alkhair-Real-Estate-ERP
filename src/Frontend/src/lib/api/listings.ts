import { apiClient } from "@/lib/api-client";
import type { ApiResponse, ListingDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getListings(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<ListingDto>>>("/listings", { params });
  return data.data;
}

export async function publishListing(id: string) {
  await apiClient.post(`/listings/${id}/publish`);
}
