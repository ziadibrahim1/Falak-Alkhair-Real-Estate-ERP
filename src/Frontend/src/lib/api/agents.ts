import { apiClient } from "@/lib/api-client";
import type { AgentDto, ApiResponse, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getAgents(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<AgentDto>>>("/agents", { params });
  return data.data;
}
