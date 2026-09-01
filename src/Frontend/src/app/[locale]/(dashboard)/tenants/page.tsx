"use client";

import { useTranslations } from "next-intl";
import { ComingSoon } from "@/components/ui/ComingSoon";

export default function TenantsPage() {
  const t = useTranslations("nav");
  return <ComingSoon title={t("tenants")} />;
}
