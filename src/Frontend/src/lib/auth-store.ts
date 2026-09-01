import { create } from "zustand";
import { persist } from "zustand/middleware";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  userName: string | null;
  permissions: string[];
  setTokens: (accessToken: string, refreshToken: string) => void;
  setUser: (userName: string, permissions: string[]) => void;
  logout: () => void;
  hasPermission: (code: string) => boolean;
}

/**
 * حالة المصادقة على مستوى المتصفح فقط (Zustand + localStorage). لا يُخزَّن
 * أي سرّ على الخادم هنا؛ الـ Access/Refresh Token هما نفس ما يصدرهما الـ API.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      refreshToken: null,
      userName: null,
      permissions: [],
      setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
      setUser: (userName, permissions) => set({ userName, permissions }),
      logout: () => set({ accessToken: null, refreshToken: null, userName: null, permissions: [] }),
      hasPermission: (code) => get().permissions.includes(code),
    }),
    { name: "falak-auth-storage" }
  )
);
