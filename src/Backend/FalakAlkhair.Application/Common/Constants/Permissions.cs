namespace FalakAlkhair.Application.Common.Constants;

/// <summary>
/// كتالوج الصلاحيات الثابت في الكود (Seed) — لا يوجد Hard-coded permissions
/// داخل الـ Controllers، بل يُشار إليها بهذه الثوابت في [Authorize(Policy = ...)]
/// وتُقرأ نفس القيم من هنا عند زرع البيانات (Seed) لضمان تطابق تام.
/// الأدوار وربطها بهذه الصلاحيات ديناميكي بالكامل ويُدار من واجهة الإدارة.
/// </summary>
public static class Permissions
{
    public static class Modules
    {
        public const string Property = nameof(Property);
        public const string Unit = nameof(Unit);
        public const string Owner = nameof(Owner);
        public const string Agreement = nameof(Agreement);
        public const string User = nameof(User);
        public const string Role = nameof(Role);
        public const string Document = nameof(Document);
        public const string AuditLog = nameof(AuditLog);
        public const string Financial = nameof(Financial);
        public const string Settings = nameof(Settings);
        public const string Tenant = nameof(Tenant);
        public const string Lease = nameof(Lease);
        public const string Payment = nameof(Payment);
        public const string Agent = nameof(Agent);
        public const string Buyer = nameof(Buyer);
        public const string Seller = nameof(Seller);
        public const string Lead = nameof(Lead);
        public const string Commission = nameof(Commission);
        public const string Listing = nameof(Listing);
        public const string Marketing = nameof(Marketing);
        public const string Viewing = nameof(Viewing);
        public const string Offer = nameof(Offer);
        public const string Sale = nameof(Sale);
    }

    public static class Actions
    {
        public const string View = nameof(View);
        public const string Create = nameof(Create);
        public const string Edit = nameof(Edit);
        public const string Delete = nameof(Delete);
        public const string Approve = nameof(Approve);
        public const string Reject = nameof(Reject);
        public const string Export = nameof(Export);
        public const string Print = nameof(Print);
        public const string Manage = nameof(Manage);
        public const string Financial = nameof(Financial);
        public const string Assign = nameof(Assign);
    }

    // --- Property ---
    public const string PropertyView = "Property.View";
    public const string PropertyCreate = "Property.Create";
    public const string PropertyEdit = "Property.Edit";
    public const string PropertyDelete = "Property.Delete";
    public const string PropertyExport = "Property.Export";

    // --- Unit ---
    public const string UnitView = "Unit.View";
    public const string UnitCreate = "Unit.Create";
    public const string UnitEdit = "Unit.Edit";
    public const string UnitDelete = "Unit.Delete";

    // --- Owner ---
    public const string OwnerView = "Owner.View";
    public const string OwnerCreate = "Owner.Create";
    public const string OwnerEdit = "Owner.Edit";
    public const string OwnerDelete = "Owner.Delete";

    // --- Agreement (عقود إدارة الأملاك) ---
    public const string AgreementView = "Agreement.View";
    public const string AgreementCreate = "Agreement.Create";
    public const string AgreementEdit = "Agreement.Edit";
    public const string AgreementApprove = "Agreement.Approve";

    // --- Users & Roles ---
    public const string UserView = "User.View";
    public const string UserManage = "User.Manage";
    public const string RoleView = "Role.View";
    public const string RoleManage = "Role.Manage";

    // --- Documents ---
    public const string DocumentView = "Document.View";
    public const string DocumentManage = "Document.Manage";

    // --- Audit ---
    public const string AuditLogView = "AuditLog.View";

    // --- Financial ---
    public const string FinancialView = "Financial.View";
    public const string FinancialManage = "Financial.Manage";

    // --- Settings ---
    public const string SettingsManage = "Settings.Manage";

    // --- Tenant (المستأجرون) ---
    public const string TenantView = "Tenant.View";
    public const string TenantCreate = "Tenant.Create";
    public const string TenantEdit = "Tenant.Edit";
    public const string TenantDelete = "Tenant.Delete";

    // --- Lease (عقود الإيجار) ---
    public const string LeaseView = "Lease.View";
    public const string LeaseCreate = "Lease.Create";
    public const string LeaseEdit = "Lease.Edit";
    public const string LeaseActivate = "Lease.Activate";
    public const string LeaseTerminate = "Lease.Terminate";

    // --- Payment (التحصيل والمدفوعات) ---
    public const string PaymentView = "Payment.View";
    public const string PaymentCreate = "Payment.Create";

    // --- Agent (المسوّقون العقاريون) ---
    public const string AgentView = "Agent.View";
    public const string AgentCreate = "Agent.Create";
    public const string AgentEdit = "Agent.Edit";
    public const string AgentDelete = "Agent.Delete";

    // --- Buyer (المشترون) ---
    public const string BuyerView = "Buyer.View";
    public const string BuyerCreate = "Buyer.Create";
    public const string BuyerEdit = "Buyer.Edit";
    public const string BuyerDelete = "Buyer.Delete";

    // --- Seller (البائعون) ---
    public const string SellerView = "Seller.View";
    public const string SellerCreate = "Seller.Create";
    public const string SellerEdit = "Seller.Edit";
    public const string SellerDelete = "Seller.Delete";

    // --- Lead (العملاء المحتملون) ---
    public const string LeadView = "Lead.View";
    public const string LeadCreate = "Lead.Create";
    public const string LeadEdit = "Lead.Edit";
    public const string LeadDelete = "Lead.Delete";
    public const string LeadAssign = "Lead.Assign";

    // --- Commission (عمولات المسوّقين) ---
    public const string CommissionView = "Commission.View";
    public const string CommissionManage = "Commission.Manage";

    // --- Listing (الإعلانات العقارية) ---
    public const string ListingView = "Listing.View";
    public const string ListingCreate = "Listing.Create";
    public const string ListingEdit = "Listing.Edit";
    public const string ListingDelete = "Listing.Delete";
    public const string ListingPublish = "Listing.Approve";

    // --- Marketing (الحملات التسويقية) ---
    public const string MarketingView = "Marketing.View";
    public const string MarketingCreate = "Marketing.Create";
    public const string MarketingEdit = "Marketing.Edit";
    public const string MarketingDelete = "Marketing.Delete";

    // --- Viewing (المعاينات) ---
    public const string ViewingView = "Viewing.View";
    public const string ViewingCreate = "Viewing.Create";
    public const string ViewingEdit = "Viewing.Edit";
    public const string ViewingDelete = "Viewing.Delete";

    // --- Offer (عروض الشراء) ---
    public const string OfferView = "Offer.View";
    public const string OfferCreate = "Offer.Create";
    public const string OfferEdit = "Offer.Edit";

    // --- Sale (المبيعات) ---
    public const string SaleView = "Sale.View";
    public const string SaleCreate = "Sale.Create";
    public const string SaleEdit = "Sale.Edit";
    public const string SaleManage = "Sale.Manage";

    public static readonly IReadOnlyList<(string Code, string Module, string Action, string DescriptionAr)> All = new List<(string, string, string, string)>
    {
        (PropertyView, Modules.Property, Actions.View, "عرض العقارات"),
        (PropertyCreate, Modules.Property, Actions.Create, "إضافة عقار"),
        (PropertyEdit, Modules.Property, Actions.Edit, "تعديل عقار"),
        (PropertyDelete, Modules.Property, Actions.Delete, "حذف عقار"),
        (PropertyExport, Modules.Property, Actions.Export, "تصدير بيانات العقارات"),

        (UnitView, Modules.Unit, Actions.View, "عرض الوحدات"),
        (UnitCreate, Modules.Unit, Actions.Create, "إضافة وحدة"),
        (UnitEdit, Modules.Unit, Actions.Edit, "تعديل وحدة"),
        (UnitDelete, Modules.Unit, Actions.Delete, "حذف وحدة"),

        (OwnerView, Modules.Owner, Actions.View, "عرض الملاك"),
        (OwnerCreate, Modules.Owner, Actions.Create, "إضافة مالك"),
        (OwnerEdit, Modules.Owner, Actions.Edit, "تعديل مالك"),
        (OwnerDelete, Modules.Owner, Actions.Delete, "حذف مالك"),

        (AgreementView, Modules.Agreement, Actions.View, "عرض عقود إدارة الأملاك"),
        (AgreementCreate, Modules.Agreement, Actions.Create, "إنشاء عقد إدارة أملاك"),
        (AgreementEdit, Modules.Agreement, Actions.Edit, "تعديل عقد إدارة أملاك"),
        (AgreementApprove, Modules.Agreement, Actions.Approve, "اعتماد عقد إدارة أملاك"),

        (UserView, Modules.User, Actions.View, "عرض المستخدمين"),
        (UserManage, Modules.User, Actions.Manage, "إدارة المستخدمين"),
        (RoleView, Modules.Role, Actions.View, "عرض الأدوار والصلاحيات"),
        (RoleManage, Modules.Role, Actions.Manage, "إدارة الأدوار والصلاحيات"),

        (DocumentView, Modules.Document, Actions.View, "عرض المستندات"),
        (DocumentManage, Modules.Document, Actions.Manage, "إدارة المستندات"),

        (AuditLogView, Modules.AuditLog, Actions.View, "عرض سجل التدقيق"),

        (FinancialView, Modules.Financial, Actions.View, "عرض البيانات المالية"),
        (FinancialManage, Modules.Financial, Actions.Financial, "إدارة العمليات المالية"),

        (SettingsManage, Modules.Settings, Actions.Manage, "إدارة إعدادات النظام"),

        (TenantView, Modules.Tenant, Actions.View, "عرض المستأجرين"),
        (TenantCreate, Modules.Tenant, Actions.Create, "إضافة مستأجر"),
        (TenantEdit, Modules.Tenant, Actions.Edit, "تعديل مستأجر"),
        (TenantDelete, Modules.Tenant, Actions.Delete, "حذف مستأجر"),

        (LeaseView, Modules.Lease, Actions.View, "عرض عقود الإيجار"),
        (LeaseCreate, Modules.Lease, Actions.Create, "إنشاء عقد إيجار"),
        (LeaseEdit, Modules.Lease, Actions.Edit, "تعديل عقد إيجار"),
        (LeaseActivate, Modules.Lease, Actions.Approve, "تفعيل عقد إيجار"),
        (LeaseTerminate, Modules.Lease, Actions.Manage, "إنهاء عقد إيجار"),

        (PaymentView, Modules.Payment, Actions.View, "عرض المدفوعات"),
        (PaymentCreate, Modules.Payment, Actions.Create, "تسجيل دفعة"),

        (AgentView, Modules.Agent, Actions.View, "عرض المسوّقين العقاريين"),
        (AgentCreate, Modules.Agent, Actions.Create, "إضافة مسوّق عقاري"),
        (AgentEdit, Modules.Agent, Actions.Edit, "تعديل مسوّق عقاري"),
        (AgentDelete, Modules.Agent, Actions.Delete, "حذف مسوّق عقاري"),

        (BuyerView, Modules.Buyer, Actions.View, "عرض المشترين"),
        (BuyerCreate, Modules.Buyer, Actions.Create, "إضافة مشترٍ"),
        (BuyerEdit, Modules.Buyer, Actions.Edit, "تعديل مشترٍ"),
        (BuyerDelete, Modules.Buyer, Actions.Delete, "حذف مشترٍ"),

        (SellerView, Modules.Seller, Actions.View, "عرض البائعين"),
        (SellerCreate, Modules.Seller, Actions.Create, "إضافة بائع"),
        (SellerEdit, Modules.Seller, Actions.Edit, "تعديل بائع"),
        (SellerDelete, Modules.Seller, Actions.Delete, "حذف بائع"),

        (LeadView, Modules.Lead, Actions.View, "عرض العملاء المحتملين"),
        (LeadCreate, Modules.Lead, Actions.Create, "إضافة عميل محتمل"),
        (LeadEdit, Modules.Lead, Actions.Edit, "تعديل عميل محتمل"),
        (LeadDelete, Modules.Lead, Actions.Delete, "حذف عميل محتمل"),
        (LeadAssign, Modules.Lead, Actions.Assign, "إسناد عميل محتمل لمسوّق"),

        (CommissionView, Modules.Commission, Actions.View, "عرض عمولات المسوّقين"),
        (CommissionManage, Modules.Commission, Actions.Manage, "إدارة عمولات المسوّقين"),

        (ListingView, Modules.Listing, Actions.View, "عرض الإعلانات العقارية"),
        (ListingCreate, Modules.Listing, Actions.Create, "إنشاء إعلان عقاري"),
        (ListingEdit, Modules.Listing, Actions.Edit, "تعديل إعلان عقاري"),
        (ListingDelete, Modules.Listing, Actions.Delete, "حذف إعلان عقاري"),
        (ListingPublish, Modules.Listing, Actions.Approve, "نشر إعلان عقاري"),

        (MarketingView, Modules.Marketing, Actions.View, "عرض الحملات التسويقية"),
        (MarketingCreate, Modules.Marketing, Actions.Create, "إنشاء حملة تسويقية"),
        (MarketingEdit, Modules.Marketing, Actions.Edit, "تعديل حملة تسويقية"),
        (MarketingDelete, Modules.Marketing, Actions.Delete, "حذف حملة تسويقية"),

        (ViewingView, Modules.Viewing, Actions.View, "عرض المعاينات"),
        (ViewingCreate, Modules.Viewing, Actions.Create, "جدولة معاينة"),
        (ViewingEdit, Modules.Viewing, Actions.Edit, "تعديل معاينة"),
        (ViewingDelete, Modules.Viewing, Actions.Delete, "حذف معاينة"),

        (OfferView, Modules.Offer, Actions.View, "عرض عروض الشراء"),
        (OfferCreate, Modules.Offer, Actions.Create, "تسجيل عرض شراء"),
        (OfferEdit, Modules.Offer, Actions.Edit, "تحديث حالة عرض شراء"),

        (SaleView, Modules.Sale, Actions.View, "عرض معاملات البيع"),
        (SaleCreate, Modules.Sale, Actions.Create, "إنشاء معاملة بيع"),
        (SaleEdit, Modules.Sale, Actions.Edit, "تعديل معاملة بيع"),
        (SaleManage, Modules.Sale, Actions.Manage, "إدارة مسار معاملة البيع (تغيير المرحلة)"),
    };
}
