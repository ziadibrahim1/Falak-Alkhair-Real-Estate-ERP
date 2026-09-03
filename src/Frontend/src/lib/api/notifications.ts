import { apiClient } from "@/lib/api-client";
import type { ApiResponse, NotificationDto, PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

export async function getNotifications(params: ListParams) {
  const { data } = await apiClient.get<ApiResponse<PaginatedResult<NotificationDto>>>("/notifications", { params });
  return data.data;
}

export async function getUnreadNotificationCount() {
  const { data } = await apiClient.get<ApiResponse<number>>("/notifications/unread-count");
  return data.data;
}

export async function markNotificationRead(id: string) {
  await apiClient.post(`/notifications/${id}/mark-read`);
}

export async function markAllNotificationsRead() {
  await apiClient.post("/notifications/mark-all-read");
}
