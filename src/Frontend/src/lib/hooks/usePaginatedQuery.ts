"use client";

import { useEffect, useState } from "react";
import type { PaginatedResult } from "@/lib/types";
import type { ListParams } from "@/lib/api/owners";

/**
 * Hook عام لأي شاشة قائمة (Data Table) يدير حالة البحث/التقسيم لصفحات
 * ويعيد الاستعلام تلقائيًا عند تغيّرها، مع Debounce بسيط لنص البحث لتقليل
 * عدد الطلبات أثناء الكتابة.
 */
export function usePaginatedQuery<T>(fetcher: (params: ListParams) => Promise<PaginatedResult<T>>, pageSize = 10) {
  const [pageNumber, setPageNumber] = useState(1);
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [data, setData] = useState<PaginatedResult<T> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const handle = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setPageNumber(1);
    }, 400);
    return () => clearTimeout(handle);
  }, [searchTerm]);

  useEffect(() => {
    let cancelled = false;
    // مؤشر التحميل هنا مرتبط بجلب بيانات خارجي (API) وليس حالة مشتقة من Props/State أخرى،
    // وهو النمط الموصى به فعليًا لعرض Loading أثناء fetch — نتجاوز القاعدة التجريبية عمدًا.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setLoading(true);

    fetcher({ pageNumber, pageSize, searchTerm: debouncedSearch || undefined })
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, pageSize, debouncedSearch]);

  return {
    rows: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 0,
    pageNumber,
    pageSize,
    loading,
    searchTerm,
    setSearchTerm,
    setPageNumber,
  };
}
