# خارطة الطريق — المراحل القادمة

هذا الإصدار غطّى **PHASE 1** بالكامل وجزءًا من **PHASE 2** (Properties, Units, Owners, Property Management) كأساس متين حقيقي وقابل للتشغيل. الجدول أدناه يلخّص ما تبقّى، بنفس ترقيم المراحل الأصلي في متطلبات المشروع.

| المرحلة | النطاق | الحالة |
|---|---|---|
| **Phase 1** | Architecture, Database, Authentication, Users, Roles, Permissions, Settings | ✅ منجزة |
| **Phase 2** | Properties, Units, Owners, Property Management | ✅ منجزة (عدا Owner Portal المستقبلي) |
| **Phase 3** | Tenants, Leases, Payments, Receivables, Owner Statements | ⏭ التالية |
| **Phase 4** | Buyers, Sellers, Leads, Agents, Commissions | ⏭ |
| **Phase 5** | Listings, Marketing, Viewings, Sales | ⏭ |
| **Phase 6** | Maintenance, Employees, Vendors, Quotations | ⏭ |
| **Phase 7** | Auctions, Auction Integration Layer, Auction Audit | ⏭ |
| **Phase 8** | Reports, Notifications, Documents (رفع فعلي), Dashboard (إحصائيات كاملة) | ⏭ |
| **Phase 9** | Testing (تغطية شاملة), Security Hardening, Performance, Deployment, Documentation | جزئي (أساسيات الأمان والاختبارات موجودة، التغطية الكاملة لاحقًا) |

## تفصيل المرحلة التالية المقترحة (Phase 3)

1. **Tenants**: كيان `Tenant` (بيانات المستأجر + المستندات) بنفس نمط `Owner` الحالي.
2. **Leases**: كيان `Lease` مرتبط بـ `Unit`/`Tenant`/`Owner`، مع توليد جدول دفعات (`LeasePayment`) تلقائيًا عند تفعيل العقد حسب `PaymentFrequency`.
3. **Payments / Receivables**: موديول `Payment` عام (Rent, Deposit, Commission, Maintenance Charge) بنفس نمط الترقيم المرجعي والتدقيق المستخدم حاليًا.
4. **Owner Statements**: تقرير مُجمَّع (Query فقط، بلا كيان تخزين) يحسب: الرصيد الافتتاحي + إيرادات الإيجار − رسوم الإدارة − الصيانة ± الضريبة = صافي مستحق المالك، لفترة زمنية محددة.
5. **Overdue Dashboard**: استعلام `GetOverduePaymentsQuery` يفلتر `LeasePayment` حيث `DueDate < Today AND RemainingAmount > 0`.

كل كيان جديد يجب أن يرث `BaseAuditableEntity` (يحصل تلقائيًا على Audit + Soft Delete + Multi-Company scoping)، ويُسجَّل نوعه في `NumberGeneratorService.Prefixes`، وتُضاف صلاحياته إلى `Permissions.All` — هذا هو العقد الذي تلتزم به كل الموديولات الحالية، فلا حاجة لإعادة اختراعه.

## نقاط تكامل مستقبلية (خارج نطاق هذا الكود، لكن العمارة تسمح بإضافتها دون إعادة كتابة الأساس)

- **REGA / Ejar / FAL**: لا يوجد API رسمي عام متاح حاليًا لدمجه تلقائيًا. الحقول ذات العلاقة (`FalLicenseNumber`, `FalLicenseExpiryDate` في `Company`) موجودة بالفعل لتُدخَل يدويًا أو تُربط لاحقًا بمجرد توفر API رسمي، دون تغيير الـ Schema.
- **منصة المزادات المستقلة**: طبقة تكامل مقترحة تحت `FalakAlkhair.Infrastructure/Integrations/Auctions` تنفّذ `IAuctionPlatformClient` (HTTP Client + Webhook Receiver Controller)، خلف واجهة قابلة للاستبدال — لا Mock حقيقي يُبنى إلا عند ربطه بمزوّد فعلي، تفاديًا لادّعاء تكامل غير موجود.
- **WhatsApp / Email / SMS**: تُبنى كواجهات (`IWhatsAppService`, `IEmailService`, `ISmsService`) في Application، بتنفيذ Infrastructure قابل للتفعيل لاحقًا (Twilio, WhatsApp Business API ...)، مع Background Job Queue (Hangfire أو Quartz.NET) لإرسال غير متزامن.
- **Payment Gateways / Maps / AI**: نفس المبدأ — Interfaces أولاً، تنفيذ حقيقي فقط عند اختيار مزوّد فعلي وتوفر بيانات اعتماد حقيقية.

## بوابات مستقبلية (Portals)

`ApplicationUser` مصمم أصلًا بحيث يمكن أن يمثّل مستخدمًا داخليًا أو مالكًا/مستأجرًا لاحقًا (عبر دور `PropertyOwner` الموجود بالفعل في `SystemRoles`)، فبناء Owner/Tenant/Buyer/Agent Portal لاحقًا هو إضافة صفحات وسياسات تفويض جديدة فوق نفس البنية — دون تعديل نموذج المستخدم الأساسي.
