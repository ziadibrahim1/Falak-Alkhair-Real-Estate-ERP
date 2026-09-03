# العمارة التقنية — نظام إدارة شركة فلك الخير العقارية

هذا المستند يوثّق القرارات المعمارية للنظام، ويُقرأ مع [DATABASE.md](./DATABASE.md) (مخطط قاعدة البيانات) و[ROADMAP.md](./ROADMAP.md) (خطة المراحل القادمة).

## 1. لمحة عامة

النظام هو ERP عقاري لشركة فلك الخير العقارية (السوق السعودي)، مبني بعمارة Clean Architecture قابلة للتوسع دون إعادة كتابة الأساس عند إضافة موديولات جديدة (تأجير، مبيعات، صيانة، مزادات ...).

**حالة هذا الإصدار:** حتى نهاية Phase 4 (راجع ROADMAP.md) — كامل الوظائف وقابل للتشغيل الفعلي، وليس نموذجًا تجريبيًا: Authentication/Authorization حقيقي، Audit Log حقيقي، عمليات CRUD حقيقية على قاعدة بيانات حقيقية (SQL Server)، وليس بيانات وهمية. في هذه الجلسة تحديدًا تم فعليًا: `dotnet build`/`dotnet test` (25 اختبارًا ناجحًا)، توليد وتطبيق Migration حقيقي على SQL Server 2022 (Docker)، تشغيل الـ API الفعلي، وتنفيذ طلبات حقيقية على كل Endpoint جديد (تسجيل دخول، إنشاء، قوائم) — وليس مجرد مراجعة كود بصرية. الموديولات غير المبنية بعد (المبيعات، الصيانة، المزادات ...) موثّقة بوضوح في ROADMAP.md ولا يوجد أي ادّعاء بأنها منفَّذة.

## 2. Technology Stack

| الطبقة | التقنية |
|---|---|
| Frontend | Next.js 16 (App Router) + TypeScript + Tailwind CSS 4 |
| i18n / RTL | next-intl (عربي افتراضي RTL + إنجليزي LTR، بدون نصوص Hard-coded) |
| حالة الواجهة | Zustand (مصادقة) + React Hook Form + Zod (تحقق النماذج) |
| Backend | ASP.NET Core 8 Web API (C#) |
| ORM | Entity Framework Core 8 |
| قاعدة البيانات | SQL Server |
| المصادقة | ASP.NET Core Identity + JWT (Access + Refresh Token مع Rotation) |
| التفويض | Policy-based Authorization ديناميكي حسب الصلاحيات (`Permission:*`) |
| CQRS | MediatR (Commands/Queries منفصلة لكل موديول) |
| التحقق | FluentValidation (Pipeline Behaviour تلقائي قبل كل Handler) |
| السجلات | Serilog (Console + ملفات يومية) |
| التوثيق | Swagger / OpenAPI |
| الحاويات | Docker + docker-compose |

## 3. طبقات العمارة (Clean Architecture)

```
Presentation (FalakAlkhair.API)
        ↓
Application (FalakAlkhair.Application)
        ↓
Domain (FalakAlkhair.Domain)
        ↑
Infrastructure (FalakAlkhair.Infrastructure) — تنفّذ عقود Application
```

- **Domain**: كيانات العمل (Entities) والـ Enums فقط. لا يعتمد على أي حزمة خارجية (لا EF، لا ASP.NET Core). هذا يضمن أن قواعد العمل الجوهرية مستقلة عن التقنية.
- **Application**: منطق العمل عبر CQRS (Commands/Queries + Handlers)، DTOs، Validators، وعقود (Interfaces) لكل ما تحتاجه من طبقة أدنى (`IApplicationDbContext`, `ICurrentUserService`, `IIdentityService` ...). لا تعرف شيئًا عن SQL Server أو JWT تحديدًا — فقط عن العقود.
- **Infrastructure**: التنفيذ الفعلي: EF Core + SQL Server، ASP.NET Core Identity، JWT، الأدوار والصلاحيات، Audit Interceptor، Number Generator.
- **API**: Controllers رفيعة (Thin Controllers) تستدعي MediatR فقط، Middleware للأخطاء، Swagger، DI wiring.

هذا الفصل يسمح لاحقًا باستبدال SQL Server بأي مزوّد بيانات آخر، أو استبدال JWT بآلية مصادقة أخرى، دون لمس منطق العمل في Application/Domain.

## 4. لماذا CQRS عبر MediatR هنا؟

كل موديول (Owners, Properties, Units, Agreements, Auth, Roles) مقسّم إلى:
- `Commands/<Action>/<Action>Command.cs` — يحوي الـ Command + Validator + Handler في ملف واحد لسهولة التتبع (نمط Vertical Slice).
- `Queries/<Action>/<Action>Query.cs` — نفس النمط للقراءة.

هذا يفصل عمليات الكتابة (تتحقق من صحة البيانات، تُحدِّث الحالة، تُطلق Audit) عن عمليات القراءة (تُحسَّن للعرض والتصفح: Pagination/Filter/Sort عبر Server-side Projection مباشرة لتفادي N+1 queries).

## 5. الأمان والصلاحيات (RBAC)

- **المصادقة**: تسجيل الدخول يصدر Access Token (JWT قصير العمر، 30 دقيقة افتراضيًا) و Refresh Token (14 يومًا، عشوائي 512-bit، يُخزَّن في جدول `RefreshTokens` ويُدوَّر Rotation عند كل استخدام لمنع إعادة الاستخدام).
- **التفويض الديناميكي**: بدل صلاحيات ثابتة في الكود، كل نقطة API تُحمى بـ `[Authorize(Policy = "Permission:Property.View")]`. `PermissionPolicyProvider` (في Infrastructure) يبني هذه السياسة تلقائيًا لأي كود صلاحية — بما فيها صلاحيات تُنشأ لاحقًا من واجهة الإدارة، دون تعديل كود الـ API.
- **الأدوار**: 16 دورًا أساسيًا (Seed) بعلامة `IsSystemRole=true` غير قابلة للحذف، بالإضافة إلى إمكانية إنشاء أدوار جديدة كاملة الصلاحيات المخصصة من واجهة الإدارة (`POST /api/roles`) — هذا يحقق شرط "الأدوار ليست ثابتة".
- **كتالوج الصلاحيات**: مصدر واحد للحقيقة في `FalakAlkhair.Application.Common.Constants.Permissions` — تُقرأ منه أثناء الـ Seed وتُستخدم نفسها في الـ Controllers، فلا يوجد احتمال لعدم تطابق.
- **Multi-Company / Multi-Branch**: كل كيان عمل يرث `BaseAuditableEntity` (`CompanyId`, `BranchId?`, `IsDeleted`) ويُقيَّد كل استعلام بـ `CompanyId` الخاص بالمستخدم الحالي (من الـ JWT) — تعزل بيانات كل شركة تلقائيًا حتى لو كانت الشركة الحالية واحدة فقط.

## 6. سجل التدقيق (Audit Log)

`AuditableEntitySaveChangesInterceptor` (EF Core `SaveChangesInterceptor`) يعترض كل `SaveChanges`/`SaveChangesAsync` على مستوى الـ DbContext تلقائيًا:
1. يعبّئ `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` لأي كيان يرث `BaseEntity`.
2. يبني سجل `AuditLog` (غير قابل للتعديل، Append-Only) لكل عملية إنشاء/تعديل/حذف على كيان يرث `BaseAuditableEntity`، متضمنًا القيم القديمة والجديدة (JSON)، المستخدم، الـ IP، الـ User-Agent، والشركة/الفرع.

هذا يعني أن كل موديول جديد يُبنى مستقبلاً (تأجير، مبيعات، مزادات ...) يحصل على تدقيق كامل تلقائيًا بمجرد أن يرث كيانه من `BaseAuditableEntity` — دون كتابة أي كود تدقيق إضافي.

## 7. الأرقام المرجعية (Number Generator)

`NumberGeneratorService` يستخدم عبارة SQL Server ذرّية واحدة (`UPDATE ... OUTPUT`) على جدول `NumberSequences` بدل قفل تطبيقي (Application Lock)، فتضمن تفرّد الأرقام (`PROP-000001`, `LEASE-000001` ...) تحت تزامن عالٍ دون Race Condition، ودون حجز اتصالات طويلة.

## 8. طبقة تكامل المزادات (مستقبلية)

بحسب المتطلبات، منصة المزادات مستقلة ولا تُدمج داخل هذا النظام. الـ ERP يحتفظ فقط بالبيانات الأساسية للمزاد (Master Data)، بينما تُبنى طبقة تكامل (`/api/integrations/auctions/*` + Webhooks: `AuctionCreated`, `BidPlaced`, `AuctionEnded` ...) في مرحلة لاحقة (راجع ROADMAP.md، Phase 7) خلف Interface قابل لاستبدال المزوّد الفعلي دون تغيير باقي النظام.

## 9. هيكل المجلدات

```
/
├── docs/                         التوثيق المعماري (هذا الملف + ERD + خارطة الطريق)
├── deploy/                       Docker Compose + Dockerfiles
├── src/
│   ├── Backend/
│   │   ├── FalakAlkhair.Domain/          الكيانات والـ Enums (بلا اعتماديات خارجية)
│   │   ├── FalakAlkhair.Application/     CQRS + DTOs + Validators + عقود
│   │   ├── FalakAlkhair.Infrastructure/  EF Core + Identity + JWT + Audit + Seed
│   │   ├── FalakAlkhair.API/             Controllers + Program.cs + Swagger
│   │   ├── FalakAlkhair.UnitTests/       اختبارات xUnit
│   │   └── FalakAlkhair.sln
│   └── Frontend/                 Next.js (App Router) + next-intl
└── README.md                     دليل التشغيل الكامل
```

## 10. الموديولات المبنية في هذا الإصدار

| الموديول | Backend API | Frontend |
|---|---|---|
| المصادقة (Login/Refresh/Register) | ✅ | ✅ صفحة دخول |
| الشركات والفروع | ✅ (كيانات + Seed) | — (تُدار لاحقًا من الإعدادات) |
| الأدوار والصلاحيات الديناميكية | ✅ | جزئي (API جاهز، شاشة إدارة لاحقًا) |
| سجل التدقيق (Audit Log) | ✅ (تلقائي) | — (شاشة عرض لاحقًا) |
| الملاك (Owners) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| العقارات (Properties) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| الوحدات (Units) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| عقود إدارة الأملاك + Workflow الاعتماد | ✅ | ✅ قائمة + اعتماد |
| المستأجرون (Tenants) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| عقود الإيجار (Leases) + جدول سداد تلقائي | ✅ | ✅ قائمة + تفعيل/إنهاء |
| التحصيل والمدفوعات + كشوف حساب المالك/المستأجر | ✅ | جزئي (API جاهز، لوحة Overdue لاحقًا) |
| المسوّقون العقاريون (Agents) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| المشترون (Buyers) + محرك مطابقة بسيط | ✅ CRUD + `/matches` | ✅ قائمة + بحث/تصفّح |
| البائعون (Sellers) وتفويضات البيع | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| العملاء المحتملون (Leads) + إسناد لمسوّق | ✅ CRUD + `/assign` | ✅ قائمة + بحث/تصفّح |
| عمولات المسوّقين (Commissions) — توليد تلقائي عند تفعيل العقد/إتمام البيع | ✅ | — (تُعرض ضمن الموديول المالي لاحقًا) |
| الإعلانات العقارية (Listings) + منع نشر ناقص | ✅ CRUD + `/publish` | ✅ قائمة + نشر |
| الحملات التسويقية (Marketing) — أداء محسوب من بيانات حقيقية | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| المعاينات (Viewings) | ✅ CRUD + `/complete` | ✅ قائمة + بحث/تصفّح |
| عروض الشراء (Offers) | ✅ CRUD + `/status` | ✅ قائمة + بحث/تصفّح |
| المبيعات (Sales) — مسار كامل + عمولة تلقائية | ✅ CRUD + `/stage` | ✅ قائمة + بحث/تصفّح |
| المستندات (Document) | ✅ كيان + جدول جاهز | — (رفع الملفات لاحقًا) |
| المرجّعات المركزية (Number Generator) | ✅ (متزامن مع بيانات البذر) | — |

بقية الموديولات (Maintenance, Auctions, Reports/Notifications الكاملة ...) موثّقة كخطة تنفيذ في [ROADMAP.md](./ROADMAP.md) ولم تُبنَ بعد.
