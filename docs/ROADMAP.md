# خارطة الطريق — المراحل القادمة

هذا الإصدار غطّى **PHASE 1** إلى **PHASE 6** بالكامل. الجدول أدناه يلخّص ما تبقّى، بنفس ترقيم المراحل الأصلي في متطلبات المشروع.

| المرحلة | النطاق | الحالة |
|---|---|---|
| **Phase 1** | Architecture, Database, Authentication, Users, Roles, Permissions, Settings | ✅ منجزة |
| **Phase 2** | Properties, Units, Owners, Property Management | ✅ منجزة (عدا Owner Portal المستقبلي) |
| **Phase 3** | Tenants, Leases, Payments, Receivables, Owner Statements | ✅ منجزة (عدا لوحة Overdue في الواجهة الأمامية — الـ API جاهز) |
| **Phase 4** | Buyers, Sellers, Leads, Agents, Commissions | ✅ منجزة |
| **Phase 5** | Listings, Marketing, Viewings, Sales, Offers | ✅ منجزة |
| **Phase 6** | Maintenance, Employees, Vendors, Quotations | ✅ منجزة |
| **Phase 7** | Auctions, Auction Integration Layer, Auction Audit | ⏭ التالية |
| **Phase 8** | Reports, Notifications, Documents (رفع فعلي), Dashboard (إحصائيات كاملة) | ⏭ |
| **Phase 9** | Testing (تغطية شاملة), Security Hardening, Performance, Deployment, Documentation | جزئي (أساسيات الأمان والاختبارات موجودة، التغطية الكاملة لاحقًا) |

## ما تم فعليًا في Phase 6

1. **MaintenanceRequest (طلبات الصيانة)**: دورة عمل كاملة حقيقية (`MaintenanceStatus`): `New → Assigned → Inspection → Quotation → WaitingApproval → Approved → InProgress → WaitingParts → Completed/Cancelled`. `POST /api/maintenancerequests/{id}/assign` يسند فنيًا داخليًا و/أو مورّدًا خارجيًا (New→Assigned). `POST /api/maintenancerequests/{id}/status` يمنع القفز للأمام بترتيب خاطئ، **ويمنع تحديدًا** ضبط الحالة "Approved" يدويًا (لا تتحقق إلا عبر اعتماد عرض سعر)، ويسجّل `StartDate`/`CompletionDate` تلقائيًا.
2. **MaintenanceEmployee (فنيو الصيانة)**: CRUD كامل + حماية من حذف فني لديه طلبات مسندة غير مكتملة.
3. **Vendor (موردو/شركات الصيانة)**: CRUD كامل بتقييم (Rating) وبيانات سجل تجاري/ضريبي.
4. **MaintenanceQuotation (عروض أسعار الصيانة)**: يدعم بنودًا متعددة (`MaintenanceQuotationItem`: كمية × سعر وحدة) مع حساب **حقيقي من الخادم** للمجموع الفرعي وضريبة القيمة المضافة والإجمالي (وليس إدخالًا يدويًا قابلاً للتلاعب). يدعم تعدُّد العروض على نفس الطلب للمقارنة فعليًا. `POST /api/maintenancequotations/{id}/approve` يرفض تلقائيًا بقية العروض المعلَّقة على نفس الطلب، ويحدّث طلب الصيانة (التكلفة التقديرية، المورّد المسند، الحالة → Approved) — بنفس فلسفة توليد العمولة التلقائي في تفعيل الإيجار/إتمام البيع.
5. **الواجهة الأمامية**: أربع صفحات قوائم جديدة (`/maintenance` — استبدلت ComingSoon، `/maintenance-employees`, `/vendors`, `/quotations`).

تم التحقق من Phase 6 فعليًا: `dotnet build`/`dotnet test` (38 اختبارًا ناجحًا)، migration حقيقية (`AddMaintenanceModule`) مُولَّدة ومُطبَّقة على SQL Server 2022 حقيقي (Docker)، وتشغيل الـ API الفعلي لاختبار دورة صيانة كاملة عبر طلبات HTTP حقيقية: إنشاء طلب → إسناد → تسجيل عرض سعر (تحقّق من حساب الخادم للمجموع/الضريبة/الإجمالي) → اعتماد (تحقّق من رفض العرض المنافس تلقائيًا وتحديث الطلب) → رفض ضبط "Approved" يدويًا (422 كما هو مصمَّم) → InProgress → Completed (تحقّق من `CompletionDate`/`ActualCost`)، بالإضافة إلى `npm run lint`/`npm run build` (58 صفحة).

ملاحظة فنية: EF Core يُصدر تحذيرًا (Warning غير حاجب) بأن `MaintenanceQuotationItem` (كيان بسيط بلا Soft Delete) تابع لكيان أب (`MaintenanceQuotation`) يحمل Query Filter — هذا متوقَّع ومقبول لأنه لا يوجد أمر حذف لعروض الأسعار حاليًا؛ إن أُضيف مستقبلًا يجب معالجته بحذف تسلسلي (Cascade) صريح بدل الحذف الناعم لعروض الأسعار المرفوضة/القديمة.

## ما تم فعليًا في Phase 5

1. **Listings (الإعلانات العقارية)**: كيان `Listing` لوحدة محدَّدة (بيع/إيجار)، مع أمر `POST /api/listings/{id}/publish` يمنع النشر بلا سعر ووصف (تحقيق فعلي لمتطلب "منع نشر إعلان بدون البيانات المطلوبة")، ويحدّث حالة الوحدة تلقائيًا (`ListedForSale`/`ListedForRent`) عند النشر.
2. **Marketing (الحملات التسويقية)**: كيان `MarketingCampaign` بقناة تسويقية (`MarketingChannel`)، ميزانية وتكلفة فعلية. الأداء (`LeadsCount`/`ConversionsCount`) **محسوب من بيانات حقيقية** عبر ربط `Lead.CampaignId` بالحملة — وليس عدّادًا يدويًا وهميًا.
3. **Viewings (المعاينات)**: كيان `Viewing` لمعاينة عقار/وحدة من مشترٍ أو مستأجر محتمل، مع `POST /api/viewings/{id}/complete` لتسجيل النتيجة (Completed/Cancelled/NoShow) والملاحظات.
4. **Offers (عروض الشراء)**: كيان `Offer` يدعم تعدُّد العروض على نفس الوحدة من مشترين مختلفين، مع `POST /api/offers/{id}/status` لقبول/رفض/سحب العرض.
5. **Sales (المبيعات)**: كيان `Sale` بمسار مبيعات كامل (Sales Pipeline: `Lead→Qualified→Viewing→Offer→Negotiation→Reserved→Contract→Payment→Completed/Cancelled`) عبر `POST /api/sales/{id}/stage` الذي يمنع الرجوع لمرحلة سابقة. عند الوصول لمرحلة **Completed**، يُولَّد سجل `Commission` تلقائيًا (SourceType = Sale) وتتحدَّث حالة الوحدة إلى `Sold` — نفس الآلية التلقائية المستخدمة في تفعيل عقود الإيجار (Phase 3/4)، وSourceType.Sale المعرَّف مسبقًا في Commission أصبح مُستخدَمًا فعليًا الآن.
6. **الواجهة الأمامية**: خمس صفحات قوائم حقيقية جديدة (`/listings`, `/marketing`, `/viewings`, `/offers`, `/sales`) — استبدلت صفحتَي `/marketing` و`/sales` النموذج المؤقت (ComingSoon) ببيانات حقيقية من الـ API، بنفس نمط بقية صفحات القوائم في النظام.

تم التحقق من Phase 5 فعليًا بنفس منهجية Phase 4: `dotnet build`/`dotnet test` (34 اختبارًا ناجحًا)، migration حقيقية (`AddListingsMarketingViewingsOffersSales`) مُولَّدة ومُطبَّقة على SQL Server 2022 حقيقي (Docker)، تشغيل الـ API الفعلي واختبار مسار البيع الكامل من الإعلان حتى توليد العمولة عبر طلبات HTTP حقيقية (بما في ذلك رفض الانتقال للخلف في مسار المبيعات بخطأ 422 كما هو مصمَّم)، و`npm run lint`/`npm run build` (52 صفحة، مساران لغويان).

## ما تم فعليًا في Phase 4

1. **Agents (المسوّقون العقاريون)**: كيان `Agent` (كود مرجعي `AGENT-000001`) يحمل بيانات رخصة فال (`FalLicenseNumber`, `FalLicenseExpiryDate`)، حالة (`AgentStatus`: Active/Suspended/Inactive)، ومخطط عمولة افتراضي (`CommissionSchemeType`, `DefaultCommissionPercentage`). CRUD كامل + حماية من حذف مسوّق مرتبط بعمولات مسجّلة.
2. **Buyers (المشترون)**: كيان `Buyer` بمعايير بحث (الميزانية، المدينة/الحي المفضّل، نوع العقار، نطاق المساحة، الغرض، الحالة التمويلية) + **محرك مطابقة بسيط وحقيقي** (`GET /api/buyers/{id}/matches`) يُعيد الوحدات المعروضة للبيع (`UnitStatus.ListedForSale`) التي تطابق ميزانية ومعايير المشتري فعليًا من قاعدة البيانات — مطابقة قواعدية (Rule-based)، وليست AI، التزامًا بعدم إضافة تكامل AI حقيقي دون طلب صريح.
3. **Sellers (البائعون)**: كيان `Seller` يمثّل تفويض بيع (Sale Mandate) مرتبط بمالك (`Owner`) وعقار اختياري (`Property`)، بسعر طلب/حد أدنى ونسبة عمولة، وWorkflow حالة: `Draft → Active → Expired/Cancelled/Completed`.
4. **Leads (العملاء المحتملون)**: كيان `Lead` كنقطة دخول مركزية لـ CRM النظام (مصدر، نوع: مشترٍ/مستأجر/مالك/بائع/مستثمر/مورّد، أولوية، حالة: New→Contacted→Qualified→Converted/Lost)، مع أمر `POST /api/leads/{id}/assign` لإسناده لمسوّق فعّال (يتحقق من وجود المسوّق وتفعيله قبل الإسناد، ويحدّث الحالة تلقائيًا من New إلى Contacted).
5. **Commissions (عمولات المسوّقين)**: محرّك عمولات حقيقي. `Lease` اكتسب حقل `AgentId` اختياريًا؛ عند **تفعيل** عقد إيجار له مسوّق ونسبة عمولة > صفر، يُولَّد سجل `Commission` تلقائيًا (المبلغ الأساسي = الإيجار السنوي، العمولة = المبلغ × النسبة، ضريبة القيمة المضافة 15% على العمولة، الصافي = العمولة + الضريبة) — بلا أي تدخل يدوي، بنفس فلسفة توليد جدول سداد الإيجار تلقائيًا في Phase 3. كما يوجد `POST /api/commissions` لتسجيل عمولة يدوية استثنائية (مصادر مستقبلية: بيع/مزاد عبر `CommissionSourceType`) و`POST /api/commissions/{id}/mark-paid` لتسجيل الصرف.
6. **الواجهة الأمامية**: أربع صفحات قوائم حقيقية جديدة (`/agents`, `/buyers`, `/sellers`, `/leads`) بنفس نمط صفحات الملاك/المستأجرين (بحث + تصفّح من الخادم)، مُضافة إلى القائمة الجانبية بترتيب المتطلبات الأصلي (بعد Leases وقبل Sales). نماذج الإنشاء/التعديل عبر الواجهة لا تزال غير مبنية لأي موديول حتى الآن (نفس القيد الموثّق سابقًا) — كل الإنشاء/التعديل متاح فعليًا عبر الـ API/Swagger.

### إصلاح جوهري تم اكتشافه وإصلاحه أثناء بناء Phase 4

أثناء اختبار هذه المرحلة فعليًا (توفّرت في هذه الجلسة بيئة تحتوي .NET SDK حقيقي وSQL Server عبر Docker، بخلاف الجلسات التأسيسية السابقة)، ظهر خطأ حقيقي في `NumberGeneratorService.EnsureSequenceExistsAsync`: تمرير `CancellationToken` كعنصر أخير ضمن `params object[] parameters` بدل تمريره كوسيط منفصل كان يجعل EF Core يحاول ربطه كمعامل SQL فيفشل. تم إصلاحه بتمرير `CancellationToken` صراحة عبر التوقيع الصحيح (`ExecuteSqlRawAsync(sql, IEnumerable<object>, CancellationToken)`). هذا كان سيؤثر على **أول** توليد رقم مرجعي فعلي لأي نوع كيان تم زرعه مسبقًا بكود ثابت (Owner/Property/Unit/Tenant/Lease/Payment أيضًا، وليس فقط كيانات Phase 4). بالإضافة لذلك، أُضيفت خطوة مزامنة (`EnsureNumberSequenceSeededAsync`) في `ApplicationDbContextSeed` تضمن أن عدّادات `NumberSequences` تبدأ من رقم يتجاوز الأكواد المزروعة يدويًا، لتفادي تعارض التفرّد (Unique Index) عند أول إنشاء فعلي عبر الـ API لكل نوع. تم التحقق من الإصلاح فعليًا: بناء كامل، Migration مُولَّدة ومُطبَّقة على SQL Server 2022 حقيقي (Docker)، تشغيل الـ API الفعلي، وتسجيل عناصر جديدة عبر كل Endpoint جديد بنجاح.

كل كيان جديد يجب أن يرث `BaseAuditableEntity` (يحصل تلقائيًا على Audit + Soft Delete + Multi-Company scoping)، ويُسجَّل نوعه في `NumberGeneratorService.Prefixes` **و**في خطوة المزامنة بالـ Seed إن زُرعت له بيانات تجريبية بكود ثابت، وتُضاف صلاحياته إلى `Permissions.All` — هذا هو العقد الذي تلتزم به كل الموديولات الحالية، فلا حاجة لإعادة اختراعه.

## نقاط تكامل مستقبلية (خارج نطاق هذا الكود، لكن العمارة تسمح بإضافتها دون إعادة كتابة الأساس)

- **REGA / Ejar / FAL**: لا يوجد API رسمي عام متاح حاليًا لدمجه تلقائيًا. الحقول ذات العلاقة (`FalLicenseNumber`, `FalLicenseExpiryDate` في `Company` و`Agent`) موجودة بالفعل لتُدخَل يدويًا أو تُربط لاحقًا بمجرد توفر API رسمي، دون تغيير الـ Schema.
- **منصة المزادات المستقلة**: طبقة تكامل مقترحة تحت `FalakAlkhair.Infrastructure/Integrations/Auctions` تنفّذ `IAuctionPlatformClient` (HTTP Client + Webhook Receiver Controller)، خلف واجهة قابلة للاستبدال — لا Mock حقيقي يُبنى إلا عند ربطه بمزوّد فعلي، تفاديًا لادّعاء تكامل غير موجود.
- **WhatsApp / Email / SMS**: تُبنى كواجهات (`IWhatsAppService`, `IEmailService`, `ISmsService`) في Application، بتنفيذ Infrastructure قابل للتفعيل لاحقًا (Twilio, WhatsApp Business API ...)، مع Background Job Queue (Hangfire أو Quartz.NET) لإرسال غير متزامن.
- **Payment Gateways / Maps / AI**: نفس المبدأ — Interfaces أولاً، تنفيذ حقيقي فقط عند اختيار مزوّد فعلي وتوفر بيانات اعتماد حقيقية. محرك مطابقة المشترين الحالي (Phase 4) قواعدي بحت (Rule-based) تحقيقًا لهذا المبدأ بالضبط.

## بوابات مستقبلية (Portals)

`ApplicationUser` مصمم أصلًا بحيث يمكن أن يمثّل مستخدمًا داخليًا أو مالكًا/مستأجرًا لاحقًا (عبر دور `PropertyOwner` الموجود بالفعل في `SystemRoles`)، فبناء Owner/Tenant/Buyer/Agent Portal لاحقًا هو إضافة صفحات وسياسات تفويض جديدة فوق نفس البنية — دون تعديل نموذج المستخدم الأساسي.
