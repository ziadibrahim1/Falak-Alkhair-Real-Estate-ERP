import { apiClient } from "@/lib/api-client";
import type { ApiResponse, OwnerDto, PaginatedResult } from "@/lib/types";

export interface ListParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
}

export async function getOwners(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<OwnerDto>>>("/owners", { params });
  return data.data;
}
