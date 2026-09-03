"use client";

import { useTranslations, useLocale } from "next-intl";
import { usePathname, useRouter } from "@/i18n/navigation";
import { useAuthStore } from "@/lib/auth-store";
import { NotificationBell } from "@/components/layout/NotificationBell";

export function Topbar() {
  const t = useTranslations("common");
  const tApp = useTranslations("app");
  const locale = useLocale();
  const pathname = usePathname();
  const router = useRouter();
  const { userName, logout } = useAuthStore();

  const otherLocale = locale === "ar" ? "en" : "ar";

  const handleLogout = () => {
    logout();
    router.push("/login");
  };

  return (
    <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-3">
      <span className="font-semibold text-brand-dark">{tApp("name")}</span>

      <div className="flex items-center gap-4 text-sm">
        <button
          onClick={() => router.replace(pathname, { locale: otherLocale })}
          className="rounded-md border border-border px-2 py-1 text-xs text-gray-600 hover:bg-gray-50"
          aria-label={t("language")}
        >
          {otherLocale.toUpperCase()}
        </button>
        <NotificationBell />
        {userName && <span className="text-gray-600">{userName}</span>}
        <button onClick={handleLogout} className="text-danger hover:underline">
          {t("logout")}
        </button>
      </div>
    </header>
  );
}
