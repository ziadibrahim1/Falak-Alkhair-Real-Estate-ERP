using FalakAlkhair.API.Middleware;
using FalakAlkhair.Application;
using FalakAlkhair.Infrastructure;
using FalakAlkhair.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog: Structured Logging إلى Console وملف يومي، بدل الاعتماد فقط على Console.WriteLine ---
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/falak-alkhair-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.AddControllers(options =>
{
    // كل نقطة نهاية محمية بالمصادقة افتراضيًا؛ [AllowAnonymous] صريح مطلوب لأي استثناء (مثل /login).
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Swagger / OpenAPI مع دعم توثيق JWT Bearer ---
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "نظام إدارة شركة فلك الخير العقارية - Falak Alkhair Real Estate ERP API",
        Version = "v1",
        Description = "REST API لإدارة الأملاك، العقارات، الوحدات، الملاك، عقود إدارة الأملاك، والمزيد."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل رمز JWT بالصيغة: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// --- CORS: نطاقات محددة صراحةً من الإعدادات، وليس AllowAnyOrigin على الإطلاق ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// --- Rate Limiting أساسي لحماية نقاط الدخول الحساسة مثل تسجيل الدخول ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("Default");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Falak Alkhair ERP API" }))
    .WithTags("Health");

// --- Seed تلقائي في بيئة التطوير فقط. في الإنتاج يُشغَّل عبر أمر منفصل أو Migration Job. ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var adminPassword = builder.Configuration["Seed:AdminPassword"] ?? string.Empty;
    await ApplicationDbContextSeed.SeedAsync(scope.ServiceProvider, adminPassword);
}

app.Run();

// يسمح بالإشارة لهذا الـ Program من مشروع الاختبارات (WebApplicationFactory<Program>).
public partial class Program { }
