"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslations } from "next-intl";
import { useRouter } from "@/i18n/navigation";
import { login } from "@/lib/api/auth";
import { useAuthStore } from "@/lib/auth-store";
import { decodeJwtPayload, extractPermissions, type JwtClaims } from "@/lib/jwt";

const schema = z.object({
  userNameOrEmail: z.string().min(1),
  password: z.string().min(1),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const t = useTranslations("auth");
  const tApp = useTranslations("app");
  const router = useRouter();
  const { setTokens, setUser } = useAuthStore();
  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    setSubmitting(true);
    try {
      const result = await login(values.userNameOrEmail, values.password);
      setTokens(result.accessToken, result.refreshToken);

      const claims = decodeJwtPayload<JwtClaims>(result.accessToken);
      setUser(claims?.unique_name ?? values.userNameOrEmail, extractPermissions(claims));

      router.push("/dashboard");
    } catch {
      setServerError(t("loginError"));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-8 shadow-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-brand-dark">{tApp("name")}</h1>
          <p className="mt-1 text-sm text-gray-500">{t("loginSubtitle")}</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div>
            <label className="mb-1 block text-sm font-medium" htmlFor="userNameOrEmail">
              {t("username")}
            </label>
            <input
              id="userNameOrEmail"
              type="text"
              autoComplete="username"
              className="w-full rounded-lg border border-border px-3 py-2 outline-none focus:border-brand"
              {...register("userNameOrEmail")}
            />
            {errors.userNameOrEmail && (
              <p className="mt-1 text-xs text-danger">{t("usernameRequired")}</p>
            )}
          </div>

          <div>
            <label className="mb-1 block text-sm font-medium" htmlFor="password">
              {t("password")}
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              className="w-full rounded-lg border border-border px-3 py-2 outline-none focus:border-brand"
              {...register("password")}
            />
            {errors.password && <p className="mt-1 text-xs text-danger">{t("passwordRequired")}</p>}
          </div>

          {serverError && (
            <div className="rounded-lg bg-red-50 px-3 py-2 text-sm text-danger">{serverError}</div>
          )}

          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-lg bg-brand py-2.5 font-medium text-white transition hover:bg-brand-dark disabled:opacity-60"
          >
            {t("loginButton")}
          </button>
        </form>
      </div>
    </main>
  );
}
