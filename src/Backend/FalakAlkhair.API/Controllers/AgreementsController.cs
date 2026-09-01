using FalakAlkhair.Application.Agreements.Commands.ApproveAgreement;
using FalakAlkhair.Application.Agreements.Commands.CreateAgreement;
using FalakAlkhair.Application.Agreements.Commands.UpdateAgreement;
using FalakAlkhair.Application.Agreements.Queries.GetAgreementById;
using FalakAlkhair.Application.Agreements.Queries.GetAgreementsList;
using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FalakAlkhair.API.Controllers;

/// <summary>إدارة عقود إدارة الأملاك (Property Management Agreements) مع Workflow الاعتماد.</summary>
[Route("api/agreements")]
public class AgreementsController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "Permission:" + Permissions.AgreementView)]
    public async Task<IActionResult> GetList([FromQuery] GetAgreementsListQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AgreementView)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(ApiResponse<object>.Ok(await Mediator.Send(new GetAgreementByIdQuery(id), cancellationToken)));

    [HttpPost]
    [Authorize(Policy = "Permission:" + Permissions.AgreementCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAgreementCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse<Guid>.Ok(id, "تم إنشاء عقد إدارة الأملاك بنجاح (مسودة)."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Permission:" + Permissions.AgreementEdit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAgreementCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Fail("معرّف الطلب لا يطابق معرّف المسار."));
        await Mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم تحديث العقد بنجاح."));
    }

    /// <summary>اعتماد العقد: Draft/PendingApproval → Active.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Permission:" + Permissions.AgreementApprove)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ApproveAgreementCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Ok<object?>(null, "تم اعتماد العقد وأصبح نشطًا."));
    }
}
