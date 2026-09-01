using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Roles.Commands.CreateRole;

/// <summary>
/// إنشاء دور جديد بصلاحيات مخصصة. هذا ما يحقق متطلب أن الأدوار ليست ثابتة:
/// يستطيع System Administrator إنشاء أدوار جديدة وتحديد صلاحياتها من واجهة الإدارة.
/// </summary>
public record CreateRoleCommand : IRequest<Guid>
{
    public string Name { get; init; } = default!;
    public string NameAr { get; init; } = default!;
    public string? Description { get; init; }
    public List<string> PermissionCodes { get; init; } = new();
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Matches("^[A-Za-z0-9_]+$").WithMessage("اسم الدور التقني يجب أن يكون إنجليزيًا بدون مسافات.");
        RuleFor(x => x.NameAr).NotEmpty();
        RuleFor(x => x.PermissionCodes).NotEmpty().WithMessage("يجب تحديد صلاحية واحدة على الأقل للدور.");
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IRoleManagementService _roleService;

    public CreateRoleCommandHandler(IRoleManagementService roleService)
    {
        _roleService = roleService;
    }

    public Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken) =>
        _roleService.CreateRoleAsync(request.Name, request.NameAr, request.Description, request.PermissionCodes, cancellationToken);
}
