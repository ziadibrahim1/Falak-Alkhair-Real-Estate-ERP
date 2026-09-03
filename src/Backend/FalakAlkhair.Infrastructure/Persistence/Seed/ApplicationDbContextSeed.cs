using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FalakAlkhair.Infrastructure.Persistence.Seed;

/// <summary>
/// بيانات تطويرية أولية (Development Seed): شركة فلك الخير، فرعها الرئيسي،
/// كتالوج الصلاحيات، الأدوار الأساسية (مع منح SuperAdmin كل الصلاحيات)،
/// ومستخدم إداري واحد لأغراض التطوير فقط (كلمة المرور تُقرأ من الإعدادات
/// ولا تُكتب أبدًا داخل الكود المصدري)، إضافة لبيانات تجريبية بسيطة تغطي
/// الأملاك والإيجارات لتسهيل الاختبار اليدوي والتطوير الأمامي.
/// </summary>
public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider services, string adminPassword)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        await context.Database.MigrateAsync();

        // 1) الصلاحيات
        var existingCodes = await context.Permissions.Select(p => p.Code).ToListAsync();
        foreach (var (code, module, action, descriptionAr) in Permissions.All)
        {
            if (!existingCodes.Contains(code))
            {
                context.Permissions.Add(new Permission { Code = code, Module = module, Action = action, DescriptionAr = descriptionAr });
            }
        }
        await context.SaveChangesAsync();

        // 2) الشركة والفرع الرئيسي
        var company = await context.Companies.FirstOrDefaultAsync();
        if (company is null)
        {
            company = new Company
            {
                Code = "FALAK",
                NameAr = "شركة فلك الخير العقارية",
                NameEn = "Falak Alkhair Real Estate",
                City = "الرياض",
                IsActive = true
            };
            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }

        var mainBranch = await context.Branches.FirstOrDefaultAsync(b => b.CompanyId == company.Id);
        if (mainBranch is null)
        {
            mainBranch = new Branch
            {
                CompanyId = company.Id,
                Code = "MAIN",
                NameAr = "الفرع الرئيسي",
                City = "الرياض",
                IsMainBranch = true,
                IsActive = true
            };
            context.Branches.Add(mainBranch);
            await context.SaveChangesAsync();
        }

        // 3) الأدوار الأساسية
        foreach (var roleName in typeof(SystemRoles)
                     .GetFields().Where(f => f.IsLiteral)
                     .Select(f => (string)f.GetRawConstantValue()!))
        {
            if (await roleManager.FindByNameAsync(roleName) is not null) continue;

            var role = new ApplicationRole
            {
                Name = roleName,
                NameAr = SystemRoles.ArabicNames.GetValueOrDefault(roleName, roleName),
                IsSystemRole = true,
                CompanyId = null
            };

            await roleManager.CreateAsync(role);
        }

        // SuperAdmin و SystemAdministrator يحصلان على كل الصلاحيات تلقائيًا (بما فيها صلاحيات
        // الوحدات الجديدة — إعادة تشغيل الـ Seed بعد إضافة موديول جديد يمنحها تلقائيًا لهما).
        var allPermissions = await context.Permissions.ToListAsync();
        foreach (var roleName in new[] { SystemRoles.SuperAdmin, SystemRoles.SystemAdministrator })
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var assigned = await context.RolePermissions.Where(rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToListAsync();
            foreach (var permission in allPermissions.Where(p => !assigned.Contains(p.Id)))
            {
                context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }
        await context.SaveChangesAsync();

        // Viewer يحصل فقط على صلاحيات العرض (View) كمثال لدور محدود الصلاحية.
        var viewerRole = await roleManager.FindByNameAsync(SystemRoles.Viewer);
        if (viewerRole is not null)
        {
            var assigned = await context.RolePermissions.Where(rp => rp.RoleId == viewerRole.Id).Select(rp => rp.PermissionId).ToListAsync();
            foreach (var permission in allPermissions.Where(p => p.Action == "View" && !assigned.Contains(p.Id)))
            {
                context.RolePermissions.Add(new RolePermission { RoleId = viewerRole.Id, PermissionId = permission.Id });
            }
            await context.SaveChangesAsync();
        }

        // 4) مستخدم إداري للتطوير فقط — لا يُنشأ إن كانت كلمة المرور فارغة (بيئة إنتاج بلا Seed حساسة).
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("تخطي إنشاء مستخدم Admin الافتراضي: لم تُحدَّد كلمة مرور في الإعدادات (Seed:AdminPassword).");
            return;
        }

        const string adminUserName = "admin";
        if (await userManager.FindByNameAsync(adminUserName) is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = "admin@falakalkhair.local",
                FullNameAr = "مدير النظام",
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, SystemRoles.SuperAdmin);
                logger.LogInformation("تم إنشاء مستخدم Admin افتراضي للتطوير (اسم المستخدم: admin).");
            }
            else
            {
                logger.LogError("فشل إنشاء مستخدم Admin الافتراضي: {Errors}", string.Join(" ", result.Errors.Select(e => e.Description)));
            }
        }

        // 5) بيانات تجريبية بسيطة (Sample Owner/Property/Unit) لتسهيل الاختبار اليدوي.
        Owner? sampleOwner = await context.Owners.FirstOrDefaultAsync(o => o.CompanyId == company.Id);
        Property? sampleProperty = null;
        Unit? sampleUnit = null;

        if (sampleOwner is null)
        {
            sampleOwner = new Owner
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                OwnerCode = "OWNER-000001",
                PartyType = PartyType.Individual,
                NameAr = "عبدالله بن محمد السالم",
                Mobile = "0512345678",
                NationalId = "1012345678",
                City = "الرياض",
                IsActive = true
            };
            context.Owners.Add(sampleOwner);
            await context.SaveChangesAsync();

            sampleProperty = new Property
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PropertyCode = "PROP-000001",
                PropertyName = "عمارة الياسمين - حي الملقا",
                PropertyType = PropertyType.Building,
                PropertyCategory = PropertyCategory.Residential,
                Status = PropertyStatus.Active,
                OwnerId = sampleOwner.Id,
                City = "الرياض",
                District = "الملقا",
                NumberOfFloors = 5,
                TotalArea = 1200
            };
            context.Properties.Add(sampleProperty);
            await context.SaveChangesAsync();

            sampleUnit = new Unit
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PropertyId = sampleProperty.Id,
                UnitCode = "UNIT-000001",
                UnitNumber = "101",
                Floor = "1",
                UnitType = UnitType.Apartment,
                CurrentStatus = UnitStatus.Available,
                Area = 140,
                Bedrooms = 3,
                Bathrooms = 2,
                RentalPrice = 35000
            };
            context.Units.Add(sampleUnit);
            await context.SaveChangesAsync();
        }
        else
        {
            sampleProperty = await context.Properties.FirstOrDefaultAsync(p => p.CompanyId == company.Id);
            sampleUnit = sampleProperty is null ? null : await context.Units.FirstOrDefaultAsync(u => u.PropertyId == sampleProperty.Id);
        }

        // 6) مستأجر وعقد إيجار تجريبيان — يغطيان جدول السداد التلقائي ودفعة مسددة جزئيًا،
        // ليكون موديول الإيجارات قابلًا للاختبار الفوري من الواجهة الأمامية دون إدخال يدوي.
        if (sampleProperty is not null && sampleUnit is not null && !await context.Tenants.AnyAsync(t => t.CompanyId == company.Id))
        {
            var sampleTenant = new Tenant
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                TenantCode = "TEN-000001",
                PartyType = PartyType.Individual,
                NameAr = "خالد بن سعد العتيبي",
                Mobile = "0555555555",
                NationalId = "1099999999",
                City = "الرياض",
                Employer = "شركة اتصالات",
                IsActive = true
            };
            context.Tenants.Add(sampleTenant);
            await context.SaveChangesAsync();

            var leaseStart = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var sampleLease = new Lease
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                LeaseNumber = "LEASE-000001",
                TenantId = sampleTenant.Id,
                OwnerId = sampleOwner.Id,
                PropertyId = sampleProperty.Id,
                UnitId = sampleUnit.Id,
                StartDate = leaseStart,
                EndDate = leaseStart.AddYears(1).AddDays(-1),
                AnnualRentAmount = 35000,
                PaymentFrequency = PaymentFrequency.Quarterly,
                NumberOfPayments = 4,
                SecurityDeposit = 3500,
                CommissionPercentage = 5,
                VatPercentage = 15,
                Status = LeaseStatus.Active,
                ActivatedAt = leaseStart
            };

            for (var i = 1; i <= 4; i++)
            {
                sampleLease.Payments.Add(new LeasePayment
                {
                    CompanyId = company.Id,
                    BranchId = mainBranch.Id,
                    InstallmentNumber = i,
                    DueDate = leaseStart.AddMonths((i - 1) * 3),
                    Amount = 8750,
                    PaidAmount = i == 1 ? 8750 : 0,
                    Status = i == 1 ? LeasePaymentStatus.Paid : LeasePaymentStatus.Pending
                });
            }

            context.Leases.Add(sampleLease);
            sampleUnit.CurrentStatus = UnitStatus.Rented;
            await context.SaveChangesAsync();

            var firstInstallment = sampleLease.Payments.First(p => p.InstallmentNumber == 1);
            context.Payments.Add(new Payment
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PaymentNumber = "PAY-000001",
                LeaseId = sampleLease.Id,
                LeasePaymentId = firstInstallment.Id,
                Amount = 8750,
                PaymentDate = leaseStart,
                PaymentMethod = PaymentMethod.BankTransfer,
                ReferenceNumber = "REF-0001",
                BankName = "البنك الأهلي السعودي"
            });
            await context.SaveChangesAsync();
        }
    }
}
