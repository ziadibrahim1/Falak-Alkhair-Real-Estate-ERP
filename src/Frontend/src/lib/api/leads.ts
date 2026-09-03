import { apiClient } from "@/lib/api-client";
import type { ApiResponse, LeadDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getLeads(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<LeadDto>>>("/leads", { params });
  return data.data;
}

export async function assignLead(id: string, agentId: string) {
  await apiClient.post(`/leads/${id}/assign`, { agentId });
}
