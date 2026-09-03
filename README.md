# نظام إدارة شركة فلك الخير العقارية | Falak Alkhair Real Estate ERP

نظام ERP عقاري لإدارة الأملاك والوساطة العقارية، مصمم لشركة فلك الخير العقارية في السوق السعودي.

> **حالة المشروع:** حتى نهاية Phase 7 (راجع [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md#10-الموديولات-المبنية-في-هذا-الإصدار)) — Production-ready للنطاق المبني فعليًا، وليس نموذجًا تجريبيًا: مصادقة وتفويض حقيقيان، سجل تدقيق تلقائي، عمليات فعلية على SQL Server. بقية الموديولات (لوحة تحكم بإحصائيات كاملة، الإشعارات، Reports الكاملة ...) موثّقة كخطة تنفيذ واضحة في [docs/ROADMAP.md](./docs/ROADMAP.md) ولم تُبنَ بعد — لا يوجد أي ادّعاء بخلاف ذلك.

## المستندات

- [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) — العمارة، Tech Stack، الأمان، RBAC، Audit Log.
- [docs/DATABASE.md](./docs/DATABASE.md) — ERD وتفاصيل الجداول وحالة الـ Migrations.
- [docs/ROADMAP.md](./docs/ROADMAP.md) — خطة المراحل القادمة (Phase 8 → 9).

## ✅ حالة التحقق الفعلي (Build/Test/Run)

على عكس الإصدارات التأسيسية الأولى (حيث لم تتوفر بيئة بها .NET SDK أو Docker)، تم في جلسات بناء Phase 4 إلى 7 تنفيذ كل ما يلي **فعليًا، لا نظريًا**، لكل مرحلة على حدة:

- `dotnet restore && dotnet build` على الحل الكامل (5 مشاريع) — نجح بلا أخطاء ولا تحذيرات.
- `dotnet test` — **61 اختبارًا ناجحًا** (FluentValidation validators، Handlers، منطق توليد العمولات التلقائي (إيجار/بيع/مزاد)، محرك مطابقة المشترين، منع نشر إعلان ناقص، منع الرجوع في مسار المبيعات/المزادات، اعتماد عرض سعر صيانة يرفض المنافسين تلقائيًا، معالجة أحداث Webhook المزادات، واختبارا Pipeline حقيقي يمر عبر `ISender` الفعلي وليس استدعاء Handler مباشرًا).
- توليد migration حقيقي (`dotnet ef migrations add`) وتطبيقه فعليًا على **SQL Server 2022 حقيقي عبر Docker** (`dotnet ef database update`) — قاعدة بيانات كاملة بكل الجداول والفهارس والعلاقات، وليس ملفًا نظريًا لم يُختبر.
- تشغيل الـ API الفعلي (`dotnet run`) مع تفعيل الـ Seed، تسجيل دخول حقيقي عبر JWT، وتنفيذ طلبات CRUD حقيقية على كل Endpoint جديد — بما فيها اكتشاف وإصلاح خطأ حقيقي في `NumberGeneratorService` (Phase 4) كان سيمنع أول عملية إنشاء فعلية لأي كيان، التحقق من مسار البيع الكامل (إعلان → معاينة → عرض → بيع مكتمل → عمولة مولَّدة تلقائيًا)، دورة صيانة كاملة (إنشاء → إسناد → عرض سعر محسوب من الخادم → اعتماد يرفض المنافسين تلقائيًا → إكمال)، ودورة مزاد كاملة (إنشاء → اعتماد → نشر → Live → Ended → إرساء يرفض السعر تحت الاحتياطي → عمولة تلقائية بضريبة صحيحة → تسوية) بما فيها اختبار نقطة استقبال Webhook المزادات فعليًا (سرّ مشترك، رفض معرّف غير معروف، تحديث فعلي لقاعدة البيانات) — كلها عبر طلبات HTTP حقيقية.
- **اكتشاف وإصلاح خطأ جوهري عابر لكل المراحل (Phase 7)**: `ValidationBehaviour`/`LoggingBehaviour` لم يكونا يُطبَّقان إطلاقًا على أي أمر بلا نتيجة إرجاع (`IRequest`) في أي موديول منذ Phase 4، بسبب اختلاف سلوك MediatR 12.x عن الإصدارات الأقدم (`IRequest` لم تعد ترث `IRequest<Unit>`). تم تشخيصه بمعزل عن النظام، إصلاحه، وسدّ الفجوة الاختبارية التي أخفته (راجع docs/ROADMAP.md، قسم "إصلاح جوهري عابر لكل المراحل").
- `npm install && npm run lint && npm run build` على الواجهة الأمامية (58 صفحة، مسارين لغويين) — نجح بلا أخطاء، وتم فتح صفحة `/auctions` فعليًا في متصفح Chromium حقيقي (Playwright) بعد تسجيل دخول والتحقق البصري من البيانات.

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
#    → AddAgentsBuyersSellersLeadsCommissions → AddListingsMarketingViewingsOffersSales →
#    AddMaintenanceModule → AddAuctionsModule) — لا حاجة لإنشاء migration جديد إلا عند إضافة كيانات جديدة فعليًا.
#    dotnet run أدناه يطبّقها تلقائيًا في بيئة Development، أو نفّذها يدويًا:
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

61 اختبارًا ناجحًا (تم التحقق فعليًا). تُغطّي الاختبارات الحالية: FluentValidation validators، منطق Workflow اعتماد عقود إدارة الأملاك، محرك مطابقة المشترين بالعقارات، توليد عمولة المسوّق تلقائيًا (إيجار/بيع/مزاد)، منع نشر إعلان عقاري ناقص البيانات، منع الرجوع لمرحلة سابقة في مسار المبيعات/المزادات، اعتماد عرض سعر صيانة (يرفض العروض المنافسة تلقائيًا ويحدّث الطلب)، منع ضبط حالة "معتمد" لطلب صيانة يدويًا، معالجة أحداث Webhook مزاد وارد (تحديث آمن + رفض حدث غير معروف) — معظمها عبر EF Core InMemory، بالإضافة إلى اختبارَي Pipeline (`ValidationPipelineTests`) يمرّان فعليًا عبر حاوية DI و`ISender` الحقيقيَّين للتأكد أن FluentValidation تُطبَّق على كل أمر (بما فيها الأوامر بلا نتيجة إرجاع).

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
POST   /api/commissions                [Permission: Commission.Manage]  (تسجيل يدوي استثنائي؛ تُولَّد تلقائيًا عند تفعيل الإيجار/إتمام البيع)
POST   /api/commissions/{id}/mark-paid [Permission: Commission.Manage]

GET    /api/listings                   [Permission: Listing.View]
POST   /api/listings                   [Permission: Listing.Create]
PUT    /api/listings/{id}              [Permission: Listing.Edit]
POST   /api/listings/{id}/publish      [Permission: Listing.Approve]  (يمنع النشر بلا بيانات كافية)
DELETE /api/listings/{id}              [Permission: Listing.Delete]

GET    /api/marketing/campaigns        [Permission: Marketing.View]
POST   /api/marketing/campaigns        [Permission: Marketing.Create]
PUT    /api/marketing/campaigns/{id}   [Permission: Marketing.Edit]
DELETE /api/marketing/campaigns/{id}   [Permission: Marketing.Delete]

GET    /api/viewings                   [Permission: Viewing.View]
POST   /api/viewings                   [Permission: Viewing.Create]
POST   /api/viewings/{id}/complete     [Permission: Viewing.Edit]

GET    /api/offers                     [Permission: Offer.View]
POST   /api/offers                     [Permission: Offer.Create]
POST   /api/offers/{id}/status         [Permission: Offer.Edit]

GET    /api/sales                      [Permission: Sale.View]
POST   /api/sales                      [Permission: Sale.Create]
POST   /api/sales/{id}/stage           [Permission: Sale.Manage]  (يمنع الرجوع لمرحلة سابقة؛ يولّد عمولة عند Completed)

GET    /api/maintenancerequests               [Permission: MaintenanceRequest.View]
POST   /api/maintenancerequests               [Permission: MaintenanceRequest.Create]
POST   /api/maintenancerequests/{id}/assign   [Permission: MaintenanceRequest.Assign]
POST   /api/maintenancerequests/{id}/status   [Permission: MaintenanceRequest.Edit]  (لا يسمح بضبط Approved يدويًا)
DELETE /api/maintenancerequests/{id}          [Permission: MaintenanceRequest.Delete]

GET    /api/maintenanceemployees       [Permission: MaintenanceEmployee.View]
POST   /api/maintenanceemployees       [Permission: MaintenanceEmployee.Create]
PUT    /api/maintenanceemployees/{id}  [Permission: MaintenanceEmployee.Edit]
DELETE /api/maintenanceemployees/{id}  [Permission: MaintenanceEmployee.Delete]

GET    /api/vendors                    [Permission: Vendor.View]
POST   /api/vendors                    [Permission: Vendor.Create]
PUT    /api/vendors/{id}               [Permission: Vendor.Edit]
DELETE /api/vendors/{id}               [Permission: Vendor.Delete]

GET    /api/maintenancequotations              [Permission: Quotation.View]
POST   /api/maintenancequotations              [Permission: Quotation.Create]  (يحسب الإجمالي من البنود على الخادم)
POST   /api/maintenancequotations/{id}/approve [Permission: Quotation.Approve]  (يرفض العروض المنافسة تلقائيًا)

GET    /api/auctions                   [Permission: Auction.View]
GET    /api/auctions/{id}/audit-log    [Permission: Auction.View]  (سجل تدقيق كامل، للقراءة فقط)
POST   /api/auctions                   [Permission: Auction.Create]
POST   /api/auctions/{id}/approve      [Permission: Auction.Approve]  (Draft/PendingApproval → Scheduled)
POST   /api/auctions/{id}/publish      [Permission: Auction.Approve]  (Scheduled → Published + مزامنة بأفضل جهد مع المنصة الخارجية)
POST   /api/auctions/{id}/status       [Permission: Auction.Manage]  (انتقالات حرة يدوية + الإلغاء؛ يمنع ضبط Scheduled/Published/Awarded/Settled مباشرة)
POST   /api/auctions/{id}/award        [Permission: Auction.Manage]  (Ended → Awarded؛ يرفض سعرًا أقل من الاحتياطي؛ يولّد عمولة تلقائيًا)
POST   /api/auctions/{id}/settle       [Permission: Auction.Manage]  (Awarded → Settled)
POST   /api/integrations/auctions/webhook  [بلا JWT — سرّ مشترك X-Auction-Webhook-Secret]  (نقطة استقبال أحداث منصة المزادات الخارجية)

GET    /api/roles                      [Permission: Role.View]
POST   /api/roles                      [Permission: Role.Manage]
GET    /api/roles/permissions          [Permission: Role.View]
```

كل الاستجابات بصيغة موحّدة: `{ success, message, data, errors }`.

## 7) خارطة التكامل المستقبلية

راجع [docs/ROADMAP.md](./docs/ROADMAP.md) لتفاصيل: REGA/Ejar/FAL (لا تكامل تلقائي بلا API رسمي متاح)، WhatsApp/Email/SMS (Interfaces جاهزة للتفعيل لاحقًا)، بوابات الملاك/المستأجرين/المسوقين. تكامل منصة المزادات المستقلة (Integration Layer + Webhooks) مبني فعليًا الآن — راجع القسم أعلاه.

## الترخيص والملكية

هذا المشروع خاص بشركة فلك الخير العقارية.
