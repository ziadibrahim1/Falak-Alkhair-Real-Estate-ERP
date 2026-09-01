using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace FalakAlkhair.Application.Auth.Commands.RegisterUser;

/// <summary>
/// إنشاء مستخدم جديد — يُستدعى فقط من قِبل مستخدم يملك صلاحية Users.Manage
/// (التحقق يتم عبر [Authorize(Policy = "Permission:Users.Manage")] في الـ Controller).
/// </summary>
public record RegisterUserCommand : IRequest<Guid>
{
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FullNameAr { get; init; } = default!;
    public Guid? BranchId { get; init; }
    public List<string> RoleNames { get; init; } = new();
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(4);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10).WithMessage("كلمة المرور يجب ألا تقل عن 10 خانات.")
            .Matches("[A-Z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف كبير.")
            .Matches("[a-z]").WithMessage("يجب أن تحتوي كلمة المرور على حرف صغير.")
            .Matches("[0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رقم.")
            .Matches("[^a-zA-Z0-9]").WithMessage("يجب أن تحتوي كلمة المرور على رمز خاص.");
        RuleFor(x => x.FullNameAr).NotEmpty();
        RuleFor(x => x.RoleNames).NotEmpty().WithMessage("يجب تحديد دور واحد على الأقل للمستخدم.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public RegisterUserCommandHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUser.CompanyId!.Value;

        var result = await _identityService.CreateUserAsync(
            request.UserName, request.Email, request.Password, request.FullNameAr,
            companyId, request.BranchId, request.RoleNames, cancellationToken);

        if (!result.Succeeded)
        {
            throw new Common.Exceptions.BusinessRuleException(string.Join(" ", result.Errors));
        }

        return Guid.Parse(result.UserId!);
    }
}
