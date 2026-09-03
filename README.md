# نظام إدارة شركة فلك الخير العقارية | Falak Alkhair Real Estate ERP

نظام ERP عقاري لإدارة الأملاك والوساطة العقارية، مصمم لشركة فلك الخير العقارية في السوق السعودي.

> **حالة المشروع:** حتى نهاية Phase 4 (راجع [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md#10-الموديولات-المبنية-في-هذا-الإصدار)) — Production-ready للنطاق المبني فعليًا، وليس نموذجًا تجريبيًا: مصادقة وتفويض حقيقيان، سجل تدقيق تلقائي، عمليات فعلية على SQL Server. بقية الموديولات (المبيعات، الصيانة، المزادات ...) موثّقة كخطة تنفيذ واضحة في [docs/ROADMAP.md](./docs/ROADMAP.md) ولم تُبنَ بعد — لا يوجد أي ادّعاء بخلاف ذلك.

## المستندات

- [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) — العمارة، Tech Stack، الأمان، RBAC، Audit Log.
- [docs/DATABASE.md](./docs/DATABASE.md) — ERD وتفاصيل الجداول وحالة الـ Migrations.
- [docs/ROADMAP.md](./docs/ROADMAP.md) — خطة المراحل القادمة (Phase 5 → 9).

## ✅ حالة التحقق الفعلي (Build/Test/Run)

على عكس الإصدارات التأسيسية الأولى (حيث لم تتوفر بيئة بها .NET SDK أو Docker)، تم في جلسة بناء Phase 4 تنفيذ كل ما يلي **فعليًا، لا نظريًا**:

- `dotnet restore && dotnet build` على الحل الكامل (5 مشاريع) — نجح بلا أخطاء ولا تحذيرات.
- `dotnet test` — **25 اختبارًا ناجحًا** (FluentValidation validators، Handlers، منطق توليد العمولات التلقائي).
- توليد migration حقيقي (`dotnet ef migrations add`) وتطبيقه فعليًا على **SQL Server 2022 حقيقي عبر Docker** (`dotnet ef database update`) — قاعدة بيانات كاملة بكل الجداول والفهارس والعلاقات، وليس ملفًا نظريًا لم يُختبر.
- تشغيل الـ API الفعلي (`dotnet run`) مع تفعيل الـ Seed، تسجيل دخول حقيقي عبر JWT، وتنفيذ طلبات CRUD حقيقية (إنشاء/قراءة) على كل Endpoint جديد في Phase 4 — بما فيها اكتشاف وإصلاح خطأ حقيقي في `NumberGeneratorService` كان سيمنع أول عملية إنشاء فعلية لأي كيان (راجع docs/ROADMAP.md لتفاصيل الإصلاح).
- `npm install && npm run lint && npm run build` على الواجهة الأمامية (46 صفحة، مسارين لغويين) — نجح بلا أخطاء.

بيئة تنفيذ لاحقة قد لا تملك دائمًا وصولًا لـ .NET SDK/Docker بنفس السهولة؛ إن حدث ذلك مستقبلًا، ستُوثَّق أي قيود بنفس الصراحة كما في الإصدارات الأولى بدل الادّعاء بخلاف الواقع.

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

# 4. تطبيق الـ Migrations الموجودة بالفعل في المستودع (InitialCreate → AddTenantsLeasesPayments
#    → AddAgentsBuyersSellersLeadsCommissions) — لا حاجة لإنشاء migration جديد إلا عند إضافة
#    كيانات جديدة فعليًا. dotnet run أدناه يطبّقها تلقائيًا في بيئة Development، أو نفّذها يدويًا:
dotnet ef database update \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API

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

25 اختبارًا ناجحًا (تم التحقق فعليًا). تُغطّي الاختبارات الحالية: FluentValidation validators (الملاك، العقارات، المسوّقين)، منطق Workflow اعتماد عقود إدارة الأملاك (Draft/PendingApproval → Active، ورفض اعتماد عقد منتهٍ)، محرك مطابقة المشترين بالعقارات (Buyer-Property Matching)، وتوليد عمولة المسوّق تلقائيًا عند تفعيل عقد الإيجار — كلها عبر EF Core InMemory.

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

GET    /api/agents                     [Permission: Agent.View]
POST   /api/agents                     [Permission: Agent.Create]
PUT    /api/agents/{id}                [Permission: Agent.Edit]
DELETE /api/agents/{id}                [Permission: Agent.Delete]

GET    /api/buyers                     [Permission: Buyer.View]
POST   /api/buyers                     [Permission: Buyer.Create]
PUT    /api/buyers/{id}                [Permission: Buyer.Edit]
DELETE /api/buyers/{id}                [Permission: Buyer.Delete]
GET    /api/buyers/{id}/matches        [Permission: Buyer.View]   (محرك مطابقة بسيط مع الوحدات المعروضة للبيع)

GET    /api/sellers                    [Permission: Seller.View]
POST   /api/sellers                    [Permission: Seller.Create]
PUT    /api/sellers/{id}               [Permission: Seller.Edit]
DELETE /api/sellers/{id}               [Permission: Seller.Delete]

GET    /api/leads                      [Permission: Lead.View]
POST   /api/leads                      [Permission: Lead.Create]
PUT    /api/leads/{id}                 [Permission: Lead.Edit]
POST   /api/leads/{id}/assign          [Permission: Lead.Assign]
DELETE /api/leads/{id}                 [Permission: Lead.Delete]

GET    /api/commissions                [Permission: Commission.View]
POST   /api/commissions                [Permission: Commission.Manage]  (تسجيل يدوي استثنائي؛ عمولات الإيجار تُولَّد تلقائيًا عند التفعيل)
POST   /api/commissions/{id}/mark-paid [Permission: Commission.Manage]

GET    /api/roles                      [Permission: Role.View]
POST   /api/roles                      [Permission: Role.Manage]
GET    /api/roles/permissions          [Permission: Role.View]
```

كل الاستجابات بصيغة موحّدة: `{ success, message, data, errors }`.

## 7) خارطة التكامل المستقبلية

راجع [docs/ROADMAP.md](./docs/ROADMAP.md) لتفاصيل: منصة المزادات المستقلة (Integration Layer + Webhooks)، REGA/Ejar/FAL (لا تكامل تلقائي بلا API رسمي متاح)، WhatsApp/Email/SMS (Interfaces جاهزة للتفعيل لاحقًا)، بوابات الملاك/المستأجرين/المسوقين.

## الترخيص والملكية

هذا المشروع خاص بشركة فلك الخير العقارية.
