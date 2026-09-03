# العمارة التقنية — نظام إدارة شركة فلك الخير العقارية

هذا المستند يوثّق القرارات المعمارية للنظام، ويُقرأ مع [DATABASE.md](./DATABASE.md) (مخطط قاعدة البيانات) و[ROADMAP.md](./ROADMAP.md) (خطة المراحل القادمة).

## 1. لمحة عامة

النظام هو ERP عقاري لشركة فلك الخير العقارية (السوق السعودي)، مبني بعمارة Clean Architecture قابلة للتوسع دون إعادة كتابة الأساس عند إضافة موديولات جديدة (تأجير، مبيعات، صيانة، مزادات ...).

**حالة هذا الإصدار:** حتى نهاية Phase 8 (راجع ROADMAP.md) — كامل الوظائف وقابل للتشغيل الفعلي، وليس نموذجًا تجريبيًا: Authentication/Authorization حقيقي، Audit Log حقيقي، عمليات CRUD حقيقية على قاعدة بيانات حقيقية (SQL Server)، وليس بيانات وهمية. تم فعليًا عبر كل مرحلة: `dotnet build`/`dotnet test` (70 اختبارًا ناجحًا حاليًا)، توليد وتطبيق كل Migration على SQL Server 2022 حقيقي (Docker)، تشغيل الـ API الفعلي، وتنفيذ طلبات HTTP حقيقية على كل Endpoint جديد (تسجيل دخول، إنشاء، قوائم، انتقالات الحالة، رفع/تنزيل مستندات، تصدير CSV) — وليس مجرد مراجعة كود بصرية. الموديول المتبقي هو Phase 9 فقط (تعميق الاختبارات، مراجعة أمنية، مراجعة النشر) — موثَّق بوضوح في ROADMAP.md ولا يوجد أي ادّعاء بأنه منفَّذ.

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

**تنبيه معماري مهم (اكتُشف وأُصلِح أثناء Phase 7)**: القيد العام على `ValidationBehaviour<TRequest,TResponse>`/`LoggingBehaviour<TRequest,TResponse>` (وهما `IPipelineBehavior<,>` مسجَّلان عبر `AddOpenBehavior` في `Application/DependencyInjection.cs`) هو `where TRequest : notnull` — **وليس** `where TRequest : IRequest<TResponse>` كما هو شائع في قوالب Clean Architecture الأقدم. السبب: في MediatR 12.x لم تعد الواجهة غير المعمَّمة `IRequest` (المستخدَمة في كل أمر بلا نتيجة إرجاع — اعتماد، إلغاء، تحديث حالة، إسناد) ترث `IRequest<Unit>`؛ فلو أُعيد القيد إلى `IRequest<TResponse>` سيتوقف DI Container عن تسجيل هذين الـ Behaviors صامتًا لكل أمر من هذا النوع، وتُتخطى FluentValidation بالكامل دون أي خطأ ظاهر (راجع ROADMAP.md، قسم "إصلاح جوهري عابر لكل المراحل" لتفاصيل الاكتشاف والتحقق الكامل). **لا تُعِد هذا القيد إلى `IRequest<TResponse>` مستقبلاً** — اختبار `FalakAlkhair.UnitTests/Behaviours/ValidationPipelineTests.cs` يرسل أوامر عبر `ISender` الفعلي تحديدًا لرصد هذا الانحدار.

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

## 8. طبقة تكامل المزادات

بحسب المتطلبات، منصة المزادات مستقلة ولا تُدمج داخل هذا النظام. الـ ERP يحتفظ بالبيانات الأساسية للمزاد (Master Data) عبر `Auction`/`AuctionAuditLog`، وطبقة التكامل مبنية فعليًا (Phase 7):
- **الاتجاه الصادر**: `IAuctionPlatformClient` (عقد في Application) ينفَّذه `HttpAuctionPlatformClient` (HttpClient حقيقي) في `Infrastructure/Integrations/Auctions`، يُستدعى عند `POST /api/auctions/{id}/publish`. إن لم تُضبَط إعدادات المزوّد الفعلي (`AuctionIntegration:BaseUrl`/`ApiKey`) يرمي `BusinessRuleException` واضحة بدل التظاهر بتكامل غير موجود — النشر الداخلي في الـ ERP لا يفشل بسبب ذلك، فقط يُسجَّل السبب في `AuctionAuditLog`.
- **الاتجاه الوارد**: `AuctionWebhooksController` على مسار مستقل `/api/integrations/auctions/webhook` (لا يرث `BaseApiController` عمدًا لتفادي ازدواج المسار الناتج عن `[Route]` بخاصية `Inherited=true`)، محمي بسرّ مشترك (`X-Auction-Webhook-Secret`) بدل JWT لأن المستدعي نظام خارجي وليس مستخدمًا داخل النظام. يطبّق فقط تحديثات معلوماتية آمنة (سعر/عدد المزايدات، تمديد الوقت، الانتقال إلى Live/Ended) — الإرساء والتسوية المالية يبقيان أمرين داخليين صريحين لا يُفعَّلان تلقائيًا من حدث خارجي.
- كل حدث (صادر أو وارد) يُسجَّل في `AuctionAuditLog` — سجل Append-Only بلا أمر تعديل/حذف مقابل له في طبقة Application.

## 8.5 تخزين المستندات وطبقة الإشعارات (Phase 8)

- **`IFileStorageService`** (عقد في Application) خلف تنفيذ `LocalDiskFileStorageService` في Infrastructure — يحفظ الملفات فعليًا على القرص **خارج `wwwroot`** عمدًا (`FileStorage:RootPath`، افتراضيًا `App_Data/documents`)، بحيث لا يمكن الوصول لأي مستند كملف ثابت عام. الوصول الوحيد للمحتوى عبر `GetDocumentDownloadQuery` الذي يتحقق من نطاق الشركة أولًا، ثم `DocumentsController.Download` يبث الملف. يمنع صراحة الخروج عن المجلد الجذر (Path Traversal) بمقارنة المسار المطلق النهائي بعد الدمج، ويولّد اسم ملف عشوائي (`GUID`) على القرص بدل استخدام اسم الملف الأصلي. `UploadDocumentCommandValidator` يفرض قائمة بيضاء لامتدادات الملفات (`pdf/jpg/jpeg/png/doc/docx/xls/xlsx`) وحدًا أقصى للحجم (20 ميغابايت) — القرار مصمَّم ليكون قابلاً للاستبدال لاحقًا بمزوّد سحابي (S3/Azure Blob) خلف نفس الواجهة دون تغيير Application.
- **`INotificationService.Notify(...)`** يضيف صف `Notification` مباشرة إلى الـ DbContext الحالي دون استدعاء `SaveChanges` بنفسه — يُحفَظ ضمن نفس معاملة الـ Handler المستدعي، بنفس فلسفة إضافة `AuctionAuditLog` داخل معالجات المزادات. `Notification.UserId` الفارغ يعني إشعارًا عامًا لكل مستخدمي الشركة؛ وإن حُدِّد فهو موجَّه لمستخدم بعينه. لا توجد بعد بنية Background Job (Hangfire/Quartz.NET) في هذا الإصدار، لذا كل الإشعارات المبنية فعليًا حدثية (Event-driven) — تُطلَق مباشرة من داخل معالجات أوامر قائمة أصلًا (إسناد عميل محتمل، طلب صيانة عاجل، اعتماد عرض سعر، إتمام بيع، إرساء مزاد) — لا إشعارات مبنية على فحص دوري زمني (مثال: "عقد سينتهي خلال 30 يومًا") حتى تتوفر بنية جدولة حقيقية.

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
| طلبات الصيانة (Maintenance) — دورة عمل كاملة | ✅ CRUD + `/assign` + `/status` | ✅ قائمة + بحث/تصفّح |
| فنيو الصيانة (Maintenance Employees) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| موردو الصيانة (Vendors) | ✅ CRUD كامل | ✅ قائمة + بحث/تصفّح |
| عروض أسعار الصيانة (Quotations) — بنود + حساب تلقائي + اعتماد | ✅ CRUD + `/approve` | ✅ قائمة + بحث/تصفّح |
| المزادات (Auctions) — دورة حياة كاملة + عمولة تلقائية عند الإرساء | ✅ CRUD + `/approve` + `/publish` + `/status` + `/award` + `/settle` | ✅ قائمة + بحث/تصفّح |
| سجل تدقيق المزاد (Auction Audit Log) — Append-Only | ✅ `/audit-log` | — (يُعرض ضمن تفاصيل المزاد لاحقًا) |
| تكامل منصة المزادات المستقلة (صادر + Webhook وارد) | ✅ (يرمي خطأ واضحًا بلا مزوّد فعلي مضبوط) | — |
| المستندات (Documents) — رفع/تنزيل/حذف فعلي على القرص | ✅ CRUD + `/download` + `/by-entity` | ✅ رفع + قائمة + تنزيل/حذف |
| الإشعارات (Notifications) — حدثية من أوامر حقيقية قائمة | ✅ `/unread-count` + `/mark-read` + `/mark-all-read` | ✅ جرس بالترويسة + صفحة كاملة |
| لوحة التحكم بإحصائيات كاملة (Dashboard) — عشرون مؤشرًا مجمَّعًا على الخادم | ✅ `/stats` | ✅ خمسة أقسام |
| التقارير التشغيلية (Rent Roll، مسار المبيعات، ملخص العمولات، ملخص الصيانة، الإشغال) + تصدير CSV | ✅ لكل تقرير مسار JSON + `/export` | ✅ منتقي تبويبات + جدول + تصدير |
| المرجّعات المركزية (Number Generator) | ✅ (متزامن مع بيانات البذر) | — |

بقية الموديولات (Owner/Tenant/Agent Portals مستقبلية، WhatsApp/Email/SMS، Background Job Scheduler للإشعارات الزمنية ...) موثّقة كخطة تنفيذ في [ROADMAP.md](./ROADMAP.md) ولم تُبنَ بعد.
