using FalakAlkhair.Application.Common.Constants;
using FalakAlkhair.Domain.Common.Enums;
using FalakAlkhair.Domain.Entities;
using FalakAlkhair.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FalakAlkhair.Infrastructure.Persistence.Seed;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider services, string adminPassword)
    {
        Console.WriteLine("SEED-STEP 0: entered SeedAsync");
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        Console.WriteLine("SEED-STEP 1: before MigrateAsync");
        await context.Database.MigrateAsync();
        Console.WriteLine("SEED-STEP 2: after MigrateAsync");

        var existingCodes = await context.Permissions.Select(p => p.Code).ToListAsync();
        Console.WriteLine("SEED-STEP 3: after Permissions select, count=" + existingCodes.Count);
        foreach (var (code, module, action, descriptionAr) in Permissions.All)
        {
            if (!existingCodes.Contains(code))
            {
                context.Permissions.Add(new Permission { Code = code, Module = module, Action = action, DescriptionAr = descriptionAr });
            }
        }
        await context.SaveChangesAsync();
        Console.WriteLine("SEED-STEP 4: after permissions SaveChanges");

        var company = await context.Companies.FirstOrDefaultAsync();
        Console.WriteLine("SEED-STEP 5: after Companies FirstOrDefault, company is null = " + (company is null));
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
        Console.WriteLine("SEED-STEP 6: company ready, id=" + company.Id);

        var mainBranch = await context.Branches.FirstOrDefaultAsync(b => b.CompanyId == company.Id);
        Console.WriteLine("SEED-STEP 7: after Branches FirstOrDefault, mainBranch is null = " + (mainBranch is null));
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
        Console.WriteLine("SEED-STEP 8: branch ready, id=" + mainBranch.Id);

        foreach (var roleName in typeof(SystemRoles)
                     .GetFields().Where(f => f.IsLiteral)
                     .Select(f => (string)f.GetRawConstantValue()!))
        {
            Console.WriteLine("SEED-STEP 9: checking role " + roleName);
            if (await roleManager.FindByNameAsync(roleName) is not null) continue;

            var role = new ApplicationRole
            {
                Name = roleName,
                NameAr = SystemRoles.ArabicNames.GetValueOrDefault(roleName, roleName),
                IsSystemRole = true,
                CompanyId = null
            };

            await roleManager.CreateAsync(role);
            Console.WriteLine("SEED-STEP 10: created role " + roleName);
        }
        Console.WriteLine("SEED-STEP 11: roles loop done");

        var allPermissions = await context.Permissions.ToListAsync();
        Console.WriteLine("SEED-STEP 12: allPermissions count=" + allPermissions.Count);
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
        Console.WriteLine("SEED-STEP 13: role permissions saved");

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
        Console.WriteLine("SEED-STEP 14: viewer role permissions saved");

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("تخطي إنشاء مستخدم Admin الافتراضي: لم تُحدَّد كلمة مرور في الإعدادات (Seed:AdminPassword).");
            return;
        }

        const string adminUserName = "admin";
        Console.WriteLine("SEED-STEP 15: before FindByNameAsync admin");
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
        Console.WriteLine("SEED-STEP 16: admin user step done");

        if (!await context.Owners.AnyAsync(o => o.CompanyId == company.Id))
        {
            var sampleOwner = new Owner
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

            var sampleProperty = new Property
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

            context.Units.Add(new Unit
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
            });
            await context.SaveChangesAsync();
        }
        Console.WriteLine("SEED-STEP 17: sample data step done, SeedAsync fully complete");
    }
}