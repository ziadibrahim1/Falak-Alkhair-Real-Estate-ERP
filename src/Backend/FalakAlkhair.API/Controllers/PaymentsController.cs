using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using FalakAlkhair.Application.Payments.Commands.RecordPayment;
using FalakAlkhair.Application.Payments.Queries.GetOverduePayments;
using FalakAlkhair.Application.Payments.Queries.GetPaymentsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>التحصيل والمدفوعات (Accounts Receivable) — تسجيل الدفعات ولوحة المتأخرات.</summary>
public class PaymentsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.PaymentView)]
    public async Task<IActionResult> GetList([FromQuery] GetPaymentsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>لوحة المتأخرات: كل الأقساط المستحقة غير المسددة بالكامل.</summary>
    [HttpGet("overdue")]
    [Authorize(Policy = "Permission:" + Permissions.PaymentView)]
    public async Task<IActionResult> GetOverdue([FromQuery] GetOverduePaymentsQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>تسجيل دفعة تحصيل فعلية على عقد إيجار.</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.PaymentCreate)]
    public async Task<IActionResult> Create([FromBody] RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<Guid>.Ok(id, "تم تسجيل الدفعة بنجاح."));
    }
}
