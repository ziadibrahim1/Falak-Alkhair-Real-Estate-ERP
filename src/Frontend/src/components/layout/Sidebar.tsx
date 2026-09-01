"use client";

import { useTranslations } from "next-intl";
import { Link, usePathname } from "@/i18n/navigation";
import clsx from "clsx";

const NAV_ITEMS = [
  { key: "dashboard", href: "/dashboard" },
  { key: "properties", href: "/properties" },
  { key: "units", href: "/units" },
  { key: "owners", href: "/owners" },
  { key: "agreements", href: "/agreements" },
  { key: "tenants", href: "/tenants" },
  { key: "leases", href: "/leases" },
  { key: "sales", href: "/sales" },
  { key: "maintenance", href: "/maintenance" },
  { key: "marketing", href: "/marketing" },
  { key: "auctions", href: "/auctions" },
  { key: "documents", href: "/documents" },
  { key: "reports", href: "/reports" },
  { key: "finance", href: "/finance" },
  { key: "users", href: "/users" },
  { key: "settings", href: "/settings" },
] as const;

/**
 * القائمة الجانبية الكاملة حسب متطلبات النظام (البند 50). بعض المسارات غير
 * مُنفَّذة بعد ضمن هذه المرحلة التأسيسية وستُبنى في مراحل لاحقة، لكنها معروضة
 * هنا حتى يعكس الهيكل التنقّلي الكامل للنظام المستهدف.
 */
export function Sidebar() {
  const t = useTranslations("nav");
  const pathname = usePathname();

  return (
    <aside className="hidden w-64 shrink-0 border-e border-border bg-surface lg:block">
      <nav className="sticky top-0 flex h-screen flex-col gap-1 overflow-y-auto p-4">
        {NAV_ITEMS.map((item) => {
          const active = pathname === item.href;
          return (
            <Link
              key={item.key}
              href={item.href}
              className={clsx(
                "rounded-lg px-3 py-2 text-sm font-medium transition",
                active ? "bg-brand text-white" : "text-gray-700 hover:bg-gray-100"
              )}
            >
              {t(item.key)}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
