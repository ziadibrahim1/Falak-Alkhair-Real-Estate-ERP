"use client";

import { useTranslations } from "next-intl";
import { ComingSoon } from "@/components/ui/ComingSoon";

export default function SettingsPage() {
  const t = useTranslations("nav");
  return <ComingSoon title={t("settings")} />;
}
