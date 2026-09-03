namespace FalakAlkhair.Application.Reports.DTOs;

/// <summary>
/// كشف حساب مالك (البند 40). محسوب من بيانات حقيقية فعلًا (عقود إيجار
/// ودفعات مسجّلة)، وليس بيانات وهمية. بنود المصروفات وتسويات الدفع للمالك
/// ستُفعَّل تلقائيًا عند بناء موديول الصيانة (Phase 6) وموديول تسوية الملاك
/// المالي — معروضة هنا بقيمة صفر صراحةً حتى تُبنى، وليست مُخفاة أو ملفّقة.
/// </summary>
public class OwnerStatementDto
{
    public Guid OwnerId { get; set; }
    public string OwnerNameAr { get; set; } = default!;
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal RentalIncome { get; set; }
    public decimal ManagementFees { get; set; }
    public decimal MaintenanceExpenses { get; set; }
    public decimal OtherExpenses { get; set; }
    public decimal NetOwnerAmount { get; set; }
    public decimal PaymentsToOwner { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<OwnerStatementLineDto> Lines { get; set; } = new();
}

public class OwnerStatementLineDto
{
    public DateTime PaymentDate { get; set; }
    public string LeaseNumber { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public string PaymentNumber { get; set; } = default!;
    public decimal Amount { get; set; }
}

/// <summary>كشف حساب مستأجر (البند 41).</summary>
public class TenantStatementDto
{
    public Guid TenantId { get; set; }
    public string TenantNameAr { get; set; } = default!;
    public decimal TotalRentDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public int OverdueInstallmentsCount { get; set; }
    public List<TenantStatementLeaseLineDto> Leases { get; set; } = new();
}

public class TenantStatementLeaseLineDto
{
    public Guid LeaseId { get; set; }
    public string LeaseNumber { get; set; } = default!;
    public string PropertyName { get; set; } = default!;
    public string UnitNumber { get; set; } = default!;
    public string Status { get; set; } = default!;
    public decimal AnnualRentAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Outstanding { get; set; }
}
