# مخطط قاعدة البيانات — حتى نهاية Phase 8

قاعدة البيانات: **SQL Server**. هذا المستند يغطي الجداول المبنية فعليًا حتى Phase 8 (Properties/Units/Owners، Tenants/Leases/Payments، Agents/Buyers/Sellers/Leads/Commissions، Listings/MarketingCampaigns/Viewings/Offers/Sales، MaintenanceRequests/MaintenanceEmployees/Vendors/MaintenanceQuotations، Auctions/AuctionAuditLogs، Notifications، Documents). جداول المراحل القادمة (لا جداول جديدة معروفة حاليًا لـ Phase 9 — تركيزها اختبارات/أمان/نشر) موثّقة في [ROADMAP.md](./ROADMAP.md).

جداول Phase 8 الإضافية: `Notification` (UserId?, Type, Title, Message, Link?, IsRead, ReadAt? — UserId فارغ يعني إشعارًا عامًا لكل مستخدمي الشركة). جدول `Document` كان موجودًا منذ Phase 1 (Migration `InitialCreate`) لكن بلا وظيفة رفع/تنزيل فعلية حتى هذه المرحلة — لم يتغيَّر الـ Schema، فقط أُضيفت طبقة `IFileStorageService` في Infrastructure لتخزين الملفات فعليًا على القرص (خارج `wwwroot`) والربط بـ `Document.FilePath`. كلا الكيانين يرث `BaseAuditableEntity`.

جداول Phase 7 الإضافية: `Auction` (PropertyId, UnitId?, OwnerId, SellerId?, AgentId?, StartDate, EndDate, StartingPrice, ReservePrice?, DepositAmount?, CommissionPercentage, Status, WinnerBuyerId?, FinalPrice?, ExternalAuctionId?, CurrentBidAmount?, BidsCount)، `AuctionAuditLog` (AuctionId, EventType, Payload?, SourceIp?, OccurredAt — سجل Append-Only بلا أمر تعديل/حذف مقابل). `Commission` اكتسبت عمود `AuctionId` (nullable) إضافةً إلى `LeaseId`/`SaleId` الموجودين — عمولة المزاد تُولَّد تلقائيًا عند الإرساء (`AwardAuctionCommand`) بنفس آلية تفعيل الإيجار/إتمام البيع. كل هذه الكيانات ترث `BaseAuditableEntity`.

جداول Phase 6 الإضافية: `MaintenanceEmployee`، `Vendor`، `MaintenanceRequest` (PropertyId, UnitId, TenantId?, RequestType, Priority, Status, AssignedEmployeeId?, AssignedVendorId?)، `MaintenanceQuotation` + `MaintenanceQuotationItem` (بند بكمية وسعر وحدة، المجموع محسوب من الخادم). كل هذه الكيانات ترث `BaseAuditableEntity` عدا `MaintenanceQuotationItem` (كيان بسيط `BaseEntity` بلا Soft Delete، يتبع دورة حياة `MaintenanceQuotation` الأب).

جداول Phase 5 الإضافية (راجع الكيانات في `FalakAlkhair.Domain/Entities`): `Listing` (PropertyId, UnitId, ListingType, Price, Status)، `MarketingCampaign` (Channel, Budget, ActualCost, PropertyId, AgentId — وLeads مرتبطة عبر `Lead.CampaignId`)، `Viewing` (PropertyId, UnitId, BuyerId/TenantId, ScheduledAt, Status)، `Offer` (BuyerId, UnitId, Amount, Status)، `Sale` (PropertyId, UnitId, SellerId, BuyerId, Stage, FinalPrice — وCommission مرتبطة عبر `Commission.SaleId`). كل هذه الكيانات ترث `BaseAuditableEntity` بنفس ضمانات Soft Delete/Audit/Multi-Company الموثّقة أدناه.

## ERD

```mermaid
erDiagram
    Company ||--o{ Branch : "له"
    Company ||--o{ ApplicationUser : "يعمل بها"
    Branch  ||--o{ ApplicationUser : "يتبع"

    ApplicationRole ||--o{ RolePermission : "يملك"
    Permission ||--o{ RolePermission : "يُمنح ضمن"
    ApplicationUser ||--o{ RefreshToken : "يصدر"

    Owner ||--o{ Property : "يملك"
    Owner ||--o{ PropertyManagementAgreement : "طرف في"
    Property ||--o{ Unit : "يحتوي"
    Property ||--o{ PropertyManagementAgreement : "موضوع"

    Tenant ||--o{ Lease : "يستأجر"
    Owner ||--o{ Lease : "طرف في"
    Unit ||--o{ Lease : "مؤجَّرة عبر"
    Lease ||--o{ LeasePayment : "جدول سداد"
    LeasePayment ||--o{ Payment : "تحصيل مقابل"
    Agent ||--o{ Lease : "أبرم"

    Agent ||--o{ Commission : "يستحق"
    Lease ||--o{ Commission : "يولّد"
    Sale ||--o{ Commission : "يولّد"
    Auction ||--o{ Commission : "يولّد"
    Agent ||--o{ Buyer : "مسؤول عن"
    Agent ||--o{ Seller : "مسؤول عن"
    Agent ||--o{ Lead : "مسؤول عن"
    Owner ||--o{ Seller : "طرف في"
    Property ||--o{ Seller : "موضوع"
    Property ||--o{ Lead : "مهتم بـ"

    Property ||--o{ Auction : "موضوع"
    Owner ||--o{ Auction : "طرف في"
    Agent ||--o{ Auction : "يدير"
    Buyer ||--o{ Auction : "يفوز بـ"
    Auction ||--o{ AuctionAuditLog : "سجل تدقيق"

    Company ||--o{ Owner : "نطاق"
    Company ||--o{ Property : "نطاق"
    Company ||--o{ Agent : "نطاق"
    Company ||--o{ NumberSequence : "نطاق"
    Company ||--o{ AuditLog : "نطاق"
    Company ||--o{ Notification : "نطاق"
    Company ||--o{ Document : "نطاق"

    Company {
        guid Id PK
        string Code UK
        string NameAr
        string NameEn
        string CommercialRegistrationNumber
        string VatNumber
        string FalLicenseNumber
        bool IsActive
    }

    Branch {
        guid Id PK
        guid CompanyId FK
        string Code
        string NameAr
        bool IsMainBranch
    }

    ApplicationUser {
        guid Id PK
        string UserName UK
        string Email UK
        string FullNameAr
        guid CompanyId FK
        guid BranchId FK
        bool IsActive
    }

    ApplicationRole {
        guid Id PK
        string Name UK
        string NameAr
        bool IsSystemRole
        guid CompanyId FK "nullable = عام"
    }

    Permission {
        guid Id PK
        string Code UK
        string Module
        string Action
    }

    RolePermission {
        guid Id PK
        guid RoleId FK
        guid PermissionId FK
    }

    RefreshToken {
        guid Id PK
        guid UserId FK
        string Token UK
        datetime ExpiresAt
        datetime RevokedAt
    }

    AuditLog {
        guid Id PK
        guid UserId
        string EntityType
        string EntityId
        string Action
        string OldValues
        string NewValues
        string IpAddress
        datetime Timestamp
    }

    NumberSequence {
        guid Id PK
        string EntityKey
        string Prefix
        long CurrentNumber
        guid CompanyId FK
    }

    Document {
        guid Id PK
        string DocumentType
        string EntityType
        guid EntityId
        string FilePath
        datetime ExpiryDate
    }

    Owner {
        guid Id PK
        string OwnerCode UK
        string PartyType
        string NameAr
        string NationalId
        string Mobile
        string Iban
        guid CompanyId FK
    }

    Property {
        guid Id PK
        string PropertyCode UK
        string PropertyName
        string PropertyType
        string PropertyCategory
        string Status
        guid OwnerId FK
        string DeedNumber
        string City
        decimal TotalArea
        guid CompanyId FK
    }

    Unit {
        guid Id PK
        string UnitCode UK
        string UnitNumber
        guid PropertyId FK
        string UnitType
        string CurrentStatus
        decimal Area
        decimal RentalPrice
        decimal SalePrice
        guid CompanyId FK
    }

    PropertyManagementAgreement {
        guid Id PK
        string ContractNumber UK
        guid OwnerId FK
        guid PropertyId FK
        datetime StartDate
        datetime EndDate
        decimal ManagementFee
        string Status
        guid CompanyId FK
    }

    Tenant {
        guid Id PK
        string TenantCode UK
        string NameAr
        string Mobile
        guid CompanyId FK
    }

    Lease {
        guid Id PK
        string LeaseNumber UK
        guid TenantId FK
        guid OwnerId FK
        guid PropertyId FK
        guid UnitId FK
        guid AgentId FK "nullable"
        decimal AnnualRentAmount
        decimal CommissionPercentage
        string Status
        guid CompanyId FK
    }

    LeasePayment {
        guid Id PK
        guid LeaseId FK
        int InstallmentNumber
        datetime DueDate
        decimal Amount
        decimal PaidAmount
        string Status
    }

    Payment {
        guid Id PK
        string PaymentNumber UK
        guid LeaseId FK
        guid LeasePaymentId FK "nullable"
        decimal Amount
        datetime PaymentDate
        string PaymentMethod
    }

    Agent {
        guid Id PK
        string AgentCode UK
        string NameAr
        string Mobile
        string FalLicenseNumber
        string Status
        decimal DefaultCommissionPercentage
        guid CompanyId FK
    }

    Buyer {
        guid Id PK
        string BuyerCode UK
        string NameAr
        string Mobile
        decimal Budget
        string PreferredCity
        string PreferredPropertyType
        guid AssignedAgentId FK "nullable"
        guid CompanyId FK
    }

    Seller {
        guid Id PK
        string SellerCode UK
        guid OwnerId FK
        guid PropertyId FK "nullable"
        decimal AskingPrice
        decimal CommissionPercentage
        string MandateStatus
        guid AssignedAgentId FK "nullable"
        guid CompanyId FK
    }

    Lead {
        guid Id PK
        string LeadCode UK
        string NameAr
        string Mobile
        string Source
        string LeadType
        guid InterestedPropertyId FK "nullable"
        guid AssignedAgentId FK "nullable"
        string Status
        string Priority
        guid CompanyId FK
    }

    Commission {
        guid Id PK
        string CommissionNumber UK
        guid AgentId FK
        string SourceType
        guid LeaseId FK "nullable"
        guid SaleId FK "nullable"
        guid AuctionId FK "nullable"
        decimal BaseAmount
        decimal CommissionPercentage
        decimal CommissionAmount
        decimal VatAmount
        decimal NetCommissionAmount
        string Status
        guid CompanyId FK
    }

    Auction {
        guid Id PK
        string AuctionNumber UK
        guid PropertyId FK
        guid UnitId FK "nullable"
        guid OwnerId FK
        guid SellerId FK "nullable"
        guid AgentId FK "nullable"
        datetime StartDate
        datetime EndDate
        decimal StartingPrice
        decimal ReservePrice "nullable"
        decimal DepositAmount "nullable"
        decimal CommissionPercentage
        string Status
        guid WinnerBuyerId FK "nullable"
        decimal FinalPrice "nullable"
        string ExternalAuctionId "nullable"
        decimal CurrentBidAmount "nullable"
        int BidsCount
        guid CompanyId FK
    }

    AuctionAuditLog {
        guid Id PK
        guid AuctionId FK
        string EventType
        string Payload "nullable"
        string SourceIp "nullable"
        datetime OccurredAt
        guid CompanyId FK
    }

    Notification {
        guid Id PK
        guid UserId FK "nullable — فارغ يعني إشعار عام للشركة"
        string Type
        string Title
        string Message
        string Link "nullable"
        bool IsRead
        datetime ReadAt "nullable"
        guid CompanyId FK
    }

    Document {
        guid Id PK
        string DocumentType
        string EntityType "مثال: Property, Owner, Lease"
        guid EntityId
        string FileName
        string FilePath "مسار نسبي على القرص، خارج wwwroot"
        long FileSize
        string MimeType
        datetime ExpiryDate "nullable"
        guid CompanyId FK
    }
```

## ملاحظات تصميمية مهمة

- **Soft Delete**: كل الكيانات التي ترث `BaseAuditableEntity` (`Owner`, `Property`, `Unit`, `PropertyManagementAgreement`, `Document`, `Tenant`, `Lease`, `LeasePayment`, `Payment`, `Agent`, `Buyer`, `Seller`, `Lead`, `Commission`, `Listing`, `MarketingCampaign`, `Viewing`, `Offer`, `Sale`, `MaintenanceEmployee`, `Vendor`, `MaintenanceRequest`, `MaintenanceQuotation`, `Auction`, `AuctionAuditLog`, `Notification`) تحمل `IsDeleted` + `DeletedAt` + `DeletedBy`، مع Global Query Filter في EF Core يستبعدها تلقائيًا من كل الاستعلامات. لا يوجد حذف فعلي (`DELETE`) لأي سجل عمل.
- **عمولات المسوّقين تلقائية**: `Commission` لا تُنشأ يدويًا في المسار الطبيعي — تُولَّد تلقائيًا عند تفعيل `Lease` له `AgentId` ونسبة عمولة > صفر (راجع `ActivateLeaseCommand`)، أو عند إتمام `Sale` (`UpdateSaleStageCommand` → `Completed`)، أو عند إرساء `Auction` (`AwardAuctionCommand` → `Awarded`) — راجع ROADMAP.md لتفاصيل كل مرحلة. `POST /api/commissions` مخصص فقط لحالات استثنائية يدوية.
- **AuctionAuditLog سجل Append-Only حقيقي**: لا يوجد أمر تعديل أو حذف له في طبقة Application عمدًا (خلافًا لبقية الكيانات القابلة للحذف الناعم) — تحقيقًا لمتطلب عدم السماح بتعديل سجلات المزايدة بعد تسجيلها. يُسجَّل صف جديد فيه عند كل تغيّر حالة للمزاد وعند كل حدث Webhook وارد من منصة المزادات الخارجية.
- **Document.FilePath ليس رابطًا عامًا**: مسار نسبي داخل مجلد تخزين خارج `wwwroot` (`IFileStorageService`) — لا يُفسَّر أو يُقدَّم كملف ثابت مباشرة؛ الوصول الوحيد له عبر `GET /api/documents/{id}/download` بعد التحقق من الصلاحية ونطاق الشركة.
- **Notification.UserId اختياري بمعنيين**: قيمة محدَّدة تعني إشعارًا خاصًا بمستخدم بعينه، وقيمة فارغة (`NULL`) تعني إشعارًا عامًا على مستوى الشركة يظهر لكل مستخدميها — الفهرس المركّب `(CompanyId, UserId, IsRead)` مصمَّم لدعم كلا الاستعلامين بكفاءة.
- **مزامنة عدّادات الترقيم مع بيانات البذر (Seed)**: أي كيان يُزرَع ببيانات تطويرية بكود ثابت (`Owner.OwnerCode = "OWNER-000001"` مثلًا) يجب أن يُسجَّل أيضًا في `EnsureNumberSequenceSeededAsync` بنهاية `ApplicationDbContextSeed.SeedAsync`، وإلا فسيصطدم أول طلب فعلي عبر الـ API لنفس النوع بقيد التفرّد (Unique Index) — هذا خطأ تم اكتشافه وإصلاحه فعليًا أثناء بناء Phase 4 (راجع ROADMAP.md).
- **الفهرسة (Indexes)**: فهارس فريدة مركّبة على `(CompanyId, Code)` لكل الجداول ذات الترقيم المرجعي، وفهارس على الحقول المستخدمة في البحث/الفلترة (الحالة، المدينة، رقم الجوال، رقم الصك، التواريخ) تحقيقًا لمتطلب الأداء تحت آلاف السجلات.
- **الدقة المالية**: كل الحقول المالية (`decimal`) بدقة `(18,2)` لتفادي أخطاء التقريب.
- **RowVersion**: `NumberSequence` يحمل `RowVersion` (Concurrency Token) كحماية إضافية، رغم أن الآلية الأساسية لمنع التعارض هي عبارة SQL ذرّية (راجع ARCHITECTURE.md، البند 7).
- **AuditLog بلا FK صارم على المستخدم**: `UserId` بلا Foreign Key قسري حتى يبقى السجل قابلاً للقراءة حتى لو حُذف المستخدم مستقبلاً (وإن كان الحذف الفعلي للمستخدمين غير متوقَّع أصلًا).

## الـ Migrations

على عكس الإصدارات التأسيسية الأولى (Phase 1/2 حيث لم يتوفر وصول لـ NuGet)، migrations هذا الإصدار **مُولَّدة فعليًا وموجودة في المستودع** (`src/Backend/FalakAlkhair.Infrastructure/Persistence/Migrations/`):

1. `InitialCreate` — Phase 1/2 (Companies, Branches, Identity, Permissions, Owners, Properties, Units, PropertyManagementAgreement, Documents — الجدول الأخير بلا وظيفة فعلية حتى Phase 8).
2. `AddTenantsLeasesPayments` — Phase 3 (Tenants, Leases, LeasePayments, Payments).
3. `AddAgentsBuyersSellersLeadsCommissions` — Phase 4 (Agents, Buyers, Sellers, Leads, Commissions, وإضافة `Lease.AgentId`).
4. `AddListingsMarketingViewingsOffersSales` — Phase 5 (Listings, MarketingCampaigns, Viewings, Offers, Sales، وإضافة `Lead.CampaignId`/`Commission.SaleId`).
5. `AddMaintenanceModule` — Phase 6 (MaintenanceEmployees, Vendors, MaintenanceRequests, MaintenanceQuotations, MaintenanceQuotationItems).
6. `AddAuctionsModule` — Phase 7 (Auctions, AuctionAuditLogs، وإضافة `Commission.AuctionId`).
7. `AddNotificationsModule` — Phase 8 (Notifications).

كل migration من السبعة أعلاه **طُبِّق فعليًا** على SQL Server 2022 حقيقي (Docker) وتم التحقق من عمل النظام الكامل (Seed، تسجيل الدخول، CRUD عبر كل Endpoint) قبل رفعه — وليس كودًا مكتوبًا يدويًا بلا اختبار.

عند إضافة كيانات جديدة مستقبلًا، نفّذ من `src/Backend` بعد `dotnet restore`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <MigrationName> \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API
```
