import { apiClient } from "@/lib/api-client";
import type { ApiResponse, PaginatedResult, VendorDto } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getVendors(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<VendorDto>>>("/vendors", { params });
  return data.data;
}
