using FalakAlkhair.Application;
using FalakAlkhair.Application.Common.Exceptions;
using FalakAlkhair.Application.Common.Interfaces;
using FalakAlkhair.Application.Leads.Commands.AssignLead;
using FalakAlkhair.Application.Owners.Commands.CreateOwner;
using FalakAlkhair.UnitTests.TestHelpers;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FalakAlkhair.UnitTests.Behaviours;

/// <summary>
/// اختبار على مستوى الـ Pipeline الحقيقي (وليس استدعاء الـ Handler مباشرة كبقية
/// الاختبارات) — يبني حاوية DI فعلية بنفس AddApplication() المستخدمة في الإنتاج
/// ليتأكد أن ValidationBehaviour يُطبَّق فعليًا عبر ISender.Send، بما في ذلك
/// الأوامر بلا نتيجة (IRequest). هذا يغطي عيبًا حقيقيًا اكتُشف: في MediatR 12.x
/// لا ترث IRequest غير المعمَّمة IRequest&lt;Unit&gt;، فلو قُيِّد الـ Behavior بـ
/// "TRequest : IRequest&lt;TResponse&gt;" يتوقف تسجيله صامتًا لكل IRequest بلا
/// نتيجة وتُتخطى قواعد FluentValidation الخاصة به دون أي خطأ ظاهر.
/// </summary>
public class ValidationPipelineTests
{
    private static ISender BuildSender(TestDbContext context, FakeCurrentUserService currentUser)
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IApplicationDbContext>(context);
        services.AddSingleton<ICurrentUserService>(currentUser);
        services.AddSingleton<INumberGeneratorService>(new FakeNumberGeneratorService());
        services.AddLogging();

        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Should_Throw_ValidationAppException_For_Invalid_Void_Command()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var sender = BuildSender(context, currentUser);

        // AssignLeadCommand : IRequest (بلا نتيجة) — AgentId فارغ يجب أن يُرفَض عبر
        // FluentValidation قبل الوصول للـ Handler إطلاقًا (لا NotFoundException).
        var act = async () => await sender.Send(new AssignLeadCommand { Id = Guid.NewGuid(), AgentId = Guid.Empty });

        await act.Should().ThrowAsync<ValidationAppException>();
    }

    [Fact]
    public async Task Should_Throw_ValidationAppException_For_Invalid_Typed_Command()
    {
        await using var context = TestDbContext.Create();
        var currentUser = new FakeCurrentUserService();
        var sender = BuildSender(context, currentUser);

        // CreateOwnerCommand : IRequest<Guid> (بنتيجة) — للتأكد أن تخفيف القيد
        // على TRequest لم يكسر المسار المعمَّم الذي كان يعمل أصلًا.
        var act = async () => await sender.Send(new CreateOwnerCommand { NameAr = "", Mobile = "" });

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
