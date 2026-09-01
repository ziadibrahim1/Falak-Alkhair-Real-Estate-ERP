import { apiClient } from "@/lib/api-client";
import type { ApiResponse } from "@/lib/types";

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
}

export async function login(userNameOrEmail: string, password: string) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>("/auth/login", {
    userNameOrEmail,
    password,
  });
  return data.data;
}
