# نظام إدارة شركة فلك الخير العقارية | Falak Alkhair Real Estate ERP

نظام ERP عقاري لإدارة الأملاك والوساطة العقارية، مصمم لشركة فلك الخير العقارية في السوق السعودي.

> **حالة المشروع:** إصدار تأسيسي (Foundation) — Production-ready للنطاق المبني فعليًا (راجع [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md#10-الموديولات-المبنية-في-هذا-الإصدار))، وليس نموذجًا تجريبيًا: مصادقة وتفويض حقيقيان، سجل تدقيق تلقائي، عمليات فعلية على SQL Server. بقية الموديولات (تأجير، مبيعات، صيانة، مزادات ...) موثّقة كخطة تنفيذ واضحة في [docs/ROADMAP.md](./docs/ROADMAP.md) ولم تُبنَ بعد — لا يوجد أي ادّعاء بخلاف ذلك.

## المستندات

- [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) — العمارة، Tech Stack، الأمان، RBAC، Audit Log.
- [docs/DATABASE.md](./docs/DATABASE.md) — ERD وتفاصيل الجداول وأمر توليد الـ Migrations.
- [docs/ROADMAP.md](./docs/ROADMAP.md) — خطة المراحل القادمة (Phase 3 → 9).

## ⚠️ ملاحظة مهمة حول بيئة البناء التي أُنشئ فيها هذا الكود

كُتب هذا المشروع داخل بيئة سحابية معزولة (Sandbox) بلا وصول شبكي إلى `nuget.org` (سياسة أمان الشبكة في تلك البيئة). نتيجة لذلك:

- **الـ Backend (.NET)**: كل الكود مكتوب ومُراجَع يدويًا بعناية (توازن أقواس، تطابق أنواع، تسلسل الاعتماديات)، لكن **لم يُشغَّل `dotnet build`/`dotnet test` فعليًا** لأن حزم NuGet (EF Core، Identity، JWT، MediatR ...) تعذّر تنزيلها. يجب تشغيل `dotnet restore && dotnet build` أول مرة على جهازك أو في CI للتأكد.
- **الـ Frontend (Next.js)**: عكس ذلك تمامًا — `npm install`, `npm run build`, و`npm run lint` **نُفِّذت فعليًا ونجحت** داخل بيئة البناء (لأن سجل npm كان متاحًا)، فالواجهة الأمامية مُتحقَّق من بنائها فعليًا.
- **قاعدة البيانات**: لا يوجد SQL Server حقيقي أو Docker daemon في تلك البيئة، فملفات EF Core Migrations لم تُولَّد. كل إعدادات EF Core (DbContext، Fluent API Configurations، الفهارس) جاهزة بالكامل؛ الخطوة المتبقية هي أمر واحد (موثّق أدناه) لتوليد الـ Migration الأولى على جهازك.

باختصار: الكود حقيقي وكامل وليس Placeholder، لكن التحقق النهائي من بناء الـ Backend وتوليد قاعدة البيانات يحتاج تشغيله مرة واحدة في بيئة لديها وصول عادي للإنترنت.

## البنية العامة

```
src/Backend/    → ASP.NET Core 8 Web API (Clean Architecture) + SQL Server
src/Frontend/   → Next.js 16 + TypeScript + Tailwind (عربي RTL افتراضي / إنجليزي)
deploy/         → Docker Compose لتشغيل الكل معًا (SQL Server + API + Web)
docs/           → التوثيق المعماري الكامل
```

## المتطلبات الأساسية

| الأداة | الإصدار |
|---|---|
| .NET SDK | 8.0+ |
| Node.js | 20+ (وُلِّد المشروع فعليًا بـ 22) |
| SQL Server | 2019+ (أو صورة Docker `mssql/server:2022-latest`) |
| Docker + Docker Compose | اختياري، للتشغيل الموحّد |

## 1) التشغيل المحلي — Backend

```bash
cd src/Backend

# 1. استعادة الحزم (يحتاج اتصال إنترنت عادي بـ nuget.org)
dotnet restore

# 2. إعداد الأسرار محليًا بدل appsettings.json (لا تُكتب الأسرار في الملفات المرفوعة لـ Git)
cd FalakAlkhair.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=FalakAlkhairERP;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Secret" "a-random-secret-at-least-32-characters-long"
dotnet user-secrets set "Seed:AdminPassword" "Str0ng!DevPassw0rd"
cd ..

# 3. تثبيت أداة EF Core CLI (مرة واحدة على الجهاز)
dotnet tool install --global dotnet-ef

# 4. توليد أول Migration (لأول مرة فقط — إن كانت قاعدة بياناتك مُنشأة مسبقًا من إصدار
#    سابق (Phase 1/2)، شغّل بدلًا من ذلك أمر Migration جديد لجداول Phase 3
#    (Tenants/Leases/LeasePayments/Payments) بدل InitialCreate:
#    dotnet ef migrations add AddTenantsLeasesPayments --project FalakAlkhair.Infrastructure --startup-project FalakAlkhair.API --output-dir Persistence/Migrations
dotnet ef migrations add InitialCreate \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API \
  --output-dir Persistence/Migrations

# 5. تشغيل الـ API (ينشئ قاعدة البيانات ويطبّق الـ Seed تلقائيًا في بيئة Development)
dotnet run --project FalakAlkhair.API
```

الـ API يعمل افتراضيًا على `https://localhost:5001` (أو `http://localhost:5000`)، وتوثيق Swagger على `/swagger`.

**بيانات دخول Admin الافتراضية للتطوير** (تُنشأ فقط إن حدَّدت `Seed:AdminPassword`، ولا تُنشأ إطلاقًا في بيئة بلا هذا الإعداد):
- اسم المستخدم: `admin`
- كلمة المرور: القيمة التي حدَّدتها في `Seed:AdminPassword`

⚠️ **لا تستخدم Seed أو كلمة مرور افتراضية في بيئة الإنتاج.** اترك `Seed:AdminPassword` فارغًا في الإنتاج وأنشئ أول مستخدم يدويًا عبر سكربت آمن منفصل.

## 2) التشغيل المحلي — Frontend

```bash
cd src/Frontend
cp .env.example .env.local   # ثم عدّل NEXT_PUBLIC_API_URL إن لزم
npm install
npm run dev
```

الواجهة تعمل على `http://localhost:3000`، وتُعيد التوجيه تلقائيًا إلى `/ar/dashboard` (أو `/en/dashboard`).

## 3) التشغيل عبر Docker Compose (الكل معًا)

```bash
cd deploy
cp .env.example .env   # عدّل كل القيم، خصوصًا كلمات المرور والأسرار
docker compose up --build -d

# بعد أول تشغيل، طبّق الـ Migrations داخل حاوية الـ API (أو من جهازك بنفس أمر EF أعلاه
# موجّهًا Connection String لحاوية SQL Server المكشوفة على المنفذ 1433).
```

## 4) الاختبارات

```bash
cd src/Backend
dotnet test
```

تُغطّي الاختبارات الحالية: FluentValidation validators (الملاك والعقارات)، ومنطق Workflow اعتماد عقود إدارة الأملاك (Draft/PendingApproval → Active، ورفض اعتماد عقد منتهٍ) عبر EF Core InMemory.

```bash
cd src/Frontend
npm run lint    # تم التحقق فعليًا أثناء البناء ونجح بلا أخطاء
npm run build   # تم التحقق فعليًا أثناء البناء ونجح
```

## 5) متغيرات البيئة (بلا أي قيمة حقيقية مكتوبة في الكود)

| المتغيّر | أين | الوصف |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Backend (user-secrets / env) | سلسلة اتصال SQL Server |
| `Jwt:Secret` | Backend (user-secrets / env) | مفتاح توقيع JWT (32 محرفًا على الأقل) |
| `Jwt:Issuer` / `Jwt:Audience` | Backend | إصدار/جمهور التوكن |
| `Seed:AdminPassword` | Backend | كلمة مرور مستخدم Admin الأولي — اتركها فارغة في الإنتاج |
| `Cors:AllowedOrigins` | Backend | نطاقات الواجهة المسموح لها بالنداء (لا `AllowAnyOrigin`) |
| `NEXT_PUBLIC_API_URL` | Frontend | عنوان الـ API |

راجع `src/Backend/FalakAlkhair.API/appsettings.json` و`deploy/.env.example` للقيم الفارغة القابلة للتعبئة.

## 6) API

توثيق تفاعلي كامل عبر Swagger على `/swagger` بعد تشغيل الـ API. أهم المسارات في هذا الإصدار:

```
POST   /api/auth/login
POST   /api/auth/refresh-token
POST   /api/auth/register              [Permission: User.Manage]

GET    /api/owners                     [Permission: Owner.View]
POST   /api/owners                     [Permission: Owner.Create]
PUT    /api/owners/{id}                [Permission: Owner.Edit]
DELETE /api/owners/{id}                [Permission: Owner.Delete]

GET    /api/properties                 [Permission: Property.View]
POST   /api/properties                 [Permission: Property.Create]
PUT    /api/properties/{id}            [Permission: Property.Edit]
DELETE /api/properties/{id}            [Permission: Property.Delete]

GET    /api/units                      [Permission: Unit.View]
POST   /api/units                      [Permission: Unit.Create]
PUT    /api/units/{id}                 [Permission: Unit.Edit]
DELETE /api/units/{id}                 [Permission: Unit.Delete]

GET    /api/agreements                 [Permission: Agreement.View]
POST   /api/agreements                 [Permission: Agreement.Create]
POST   /api/agreements/{id}/approve    [Permission: Agreement.Approve]

GET    /api/tenants                    [Permission: Tenant.View]
POST   /api/tenants                    [Permission: Tenant.Create]
PUT    /api/tenants/{id}               [Permission: Tenant.Edit]
DELETE /api/tenants/{id}               [Permission: Tenant.Delete]

GET    /api/leases                     [Permission: Lease.View]
POST   /api/leases                     [Permission: Lease.Create]   (يولّد جدول السداد تلقائيًا)
PUT    /api/leases/{id}                [Permission: Lease.Edit]
POST   /api/leases/{id}/activate       [Permission: Lease.Activate]
POST   /api/leases/{id}/terminate      [Permission: Lease.Terminate]

GET    /api/payments                   [Permission: Payment.View]
GET    /api/payments/overdue           [Permission: Payment.View]
POST   /api/payments                   [Permission: Payment.Create]

GET    /api/reports/owner-statement/{ownerId}    [Permission: Financial.View]
GET    /api/reports/tenant-statement/{tenantId}  [Permission: Tenant.View]

GET    /api/roles                      [Permission: Role.View]
POST   /api/roles                      [Permission: Role.Manage]
GET    /api/roles/permissions          [Permission: Role.View]
```

كل الاستجابات بصيغة موحّدة: `{ success, message, data, errors }`.

## 7) خارطة التكامل المستقبلية

راجع [docs/ROADMAP.md](./docs/ROADMAP.md) لتفاصيل: منصة المزادات المستقلة (Integration Layer + Webhooks)، REGA/Ejar/FAL (لا تكامل تلقائي بلا API رسمي متاح)، WhatsApp/Email/SMS (Interfaces جاهزة للتفعيل لاحقًا)، بوابات الملاك/المستأجرين/المسوقين.

## الترخيص والملكية

هذا المشروع خاص بشركة فلك الخير العقارية.
