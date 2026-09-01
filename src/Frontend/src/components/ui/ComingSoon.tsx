"use client";

import { useTranslations } from "next-intl";

export function ComingSoon({ title }: { title: string }) {
  const t = useTranslations("common");

  return (
    <div className="flex flex-col items-center justify-center rounded-2xl border border-dashed border-border bg-surface p-16 text-center">
      <h1 className="mb-2 text-xl font-bold">{title}</h1>
      <p className="text-sm text-gray-500">{t("roadmapNotice")}</p>
    </div>
  );
}
