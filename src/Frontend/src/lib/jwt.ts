/**
 * فكّ ترميز حمولة JWT (بدون التحقق من التوقيع — التحقق الحقيقي يتم في الخادم).
 * يُستخدم فقط لقراءة الصلاحيات/اسم المستخدم لعرضها في الواجهة.
 */
export function decodeJwtPayload<T = Record<string, unknown>>(token: string): T | null {
  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join("")
    );
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}

export interface JwtClaims {
  sub: string;
  unique_name?: string;
  full_name?: string;
  company_id?: string;
  branch_id?: string;
  permission?: string | string[];
  role?: string | string[];
}

export function extractPermissions(claims: JwtClaims | null): string[] {
  if (!claims?.permission) return [];
  return Array.isArray(claims.permission) ? claims.permission : [claims.permission];
}
