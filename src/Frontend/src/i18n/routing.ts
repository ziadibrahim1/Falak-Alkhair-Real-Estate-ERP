import { defineRouting } from "next-intl/routing";

/**
 * إعدادات اللغات المدعومة: العربية (الافتراضية، RTL) والإنجليزية (LTR).
 * لا توجد أي نصوص عربية Hard-coded داخل مكوّنات React — كل النصوص تُقرأ من
 * ملفات الترجمة تحت src/messages.
 */
export const routing = defineRouting({
  locales: ["ar", "en"],
  defaultLocale: "ar",
});
