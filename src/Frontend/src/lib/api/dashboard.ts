import { apiClient } from "@/lib/api-client";
import type { ApiResponse, DashboardStatsDto } from "@/lib/types";

export async function getDashboardStats() {
  const { data } = await apiClient.get<ApiResponse<DashboardStatsDto>>("/dashboard/stats");
  return data.data;
}
