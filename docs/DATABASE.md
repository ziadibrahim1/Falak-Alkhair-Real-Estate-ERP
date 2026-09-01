# مخطط قاعدة البيانات — الإصدار التأسيسي

قاعدة البيانات: **SQL Server**. هذا المستند يغطي الجداول المبنية فعليًا في هذا الإصدار. جداول المراحل القادمة (Leases, Payments, Maintenance, Auctions ...) موثّقة في [ROADMAP.md](./ROADMAP.md) وليست جزءًا من هذا الـ Schema بعد.

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

    Company ||--o{ Owner : "نطاق"
    Company ||--o{ Property : "نطاق"
    Company ||--o{ NumberSequence : "نطاق"
    Company ||--o{ AuditLog : "نطاق"

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
```

## ملاحظات تصميمية مهمة

- **Soft Delete**: `Owner`, `Property`, `Unit`, `PropertyManagementAgreement`, `Document` تحمل `IsDeleted` + `DeletedAt` + `DeletedBy`، مع Global Query Filter في EF Core يستبعدها تلقائيًا من كل الاستعلامات. لا يوجد حذف فعلي (`DELETE`) لأي سجل عمل.
- **الفهرسة (Indexes)**: فهارس فريدة مركّبة على `(CompanyId, Code)` لكل الجداول ذات الترقيم المرجعي، وفهارس على الحقول المستخدمة في البحث/الفلترة (الحالة، المدينة، رقم الجوال، رقم الصك، التواريخ) تحقيقًا لمتطلب الأداء تحت آلاف السجلات.
- **الدقة المالية**: كل الحقول المالية (`decimal`) بدقة `(18,2)` لتفادي أخطاء التقريب.
- **RowVersion**: `NumberSequence` يحمل `RowVersion` (Concurrency Token) كحماية إضافية، رغم أن الآلية الأساسية لمنع التعارض هي عبارة SQL ذرّية (راجع ARCHITECTURE.md، البند 7).
- **AuditLog بلا FK صارم على المستخدم**: `UserId` بلا Foreign Key قسري حتى يبقى السجل قابلاً للقراءة حتى لو حُذف المستخدم مستقبلاً (وإن كان الحذف الفعلي للمستخدمين غير متوقَّع أصلًا).

## أمر توليد الـ Migration الفعلي

ملفات EF Core Migrations لم تُولَّد داخل هذه الجلسة (بيئة التطوير السحابية هنا محجوبة عن NuGet)، لكن كل الكيانات وإعدادات EF (Fluent API) جاهزة بالكامل. نفّذ من جذر المشروع بعد `dotnet restore`:

```bash
dotnet tool install --global dotnet-ef
cd src/Backend
dotnet ef migrations add InitialCreate \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project FalakAlkhair.Infrastructure \
  --startup-project FalakAlkhair.API
```
