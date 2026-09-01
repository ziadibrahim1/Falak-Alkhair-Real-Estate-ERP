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
    };
}
