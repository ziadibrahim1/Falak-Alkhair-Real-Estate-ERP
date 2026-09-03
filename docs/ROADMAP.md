# خارطة الطريق — المراحل القادمة

هذا الإصدار غطّى **PHASE 1** و**PHASE 2** و**PHASE 3** بالكامل، بالإضافة إلى **PHASE 4** (Buyers, Sellers, Leads, Agents, Commissions) كأساس متين حقيقي وقابل للتشغيل. الجدول أدناه يلخّص ما تبقّى، بنفس ترقيم المراحل الأصلي في متطلبات المشروع.

| المرحلة | النطاق | الحالة |
|---|---|---|
| **Phase 1** | Architecture, Database, Authentication, Users, Roles, Permissions, Settings | ✅ منجزة |
| **Phase 2** | Properties, Units, Owners, Property Management | ✅ منجزة (عدا Owner Portal المستقبلي) |
| **Phase 3** | Tenants, Leases, Payments, Receivables, Owner Statements | ✅ منجزة (عدا لوحة Overdue في الواجهة الأمامية — الـ API جاهز) |
| **Phase 4** | Buyers, Sellers, Leads, Agents, Commissions | ✅ منجزة |
| **Phase 5** | Listings, Marketing, Viewings, Sales | ⏭ التالية |
| **Phase 6** | Maintenance, Employees, Vendors, Quotations | ⏭ |
| **Phase 7** | Auctions, Auction Integration Layer, Auction Audit | ⏭ |
| **Phase 8** | Reports, Notifications, Documents (رفع فعلي), Dashboard (إحصائيات كاملة) | ⏭ |
| **Phase 9** | Testing (تغطية شاملة), Security Hardening, Performance, Deployment, Documentation | جزئي (أساسيات الأمان والاختبارات موجودة، التغطية الكاملة لاحقًا) |

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
