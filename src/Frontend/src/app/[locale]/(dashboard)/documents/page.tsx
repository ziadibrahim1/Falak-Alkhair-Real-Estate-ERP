"use client";

import { useTranslations } from "next-intl";
import { ComingSoon } from "@/components/ui/ComingSoon";

export default function DocumentsPage() {
  const t = useTranslations("nav");
  return <ComingSoon title={t("documents")} />;
}
