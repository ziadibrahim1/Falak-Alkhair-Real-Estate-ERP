"use client";

import { useEffect, type ReactNode } from "react";
import { useRouter } from "@/i18n/navigation";
import { useAuthStore } from "@/lib/auth-store";

/**
 * حارس مصادقة بسيط على مستوى المتصفح: يوجّه لصفحة الدخول إن لم يوجد Access
 * Token. الحماية الحقيقية تبقى دائمًا من طرف الخادم (كل نقطة API محمية
 * بـ [Authorize] + فحص الصلاحيات)، وهذا فقط لتحسين تجربة المستخدم.
 */
export function AuthGuard({ children }: { children: ReactNode }) {
  const router = useRouter();
  const accessToken = useAuthStore((s) => s.accessToken);

  useEffect(() => {
    if (!accessToken) {
      router.replace("/login");
    }
  }, [accessToken, router]);

  if (!accessToken) return null;

  return <>{children}</>;
}
