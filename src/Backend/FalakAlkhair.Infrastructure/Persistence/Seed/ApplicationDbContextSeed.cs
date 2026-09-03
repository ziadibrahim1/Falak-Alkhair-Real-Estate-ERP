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

        // 6) مسوّق عقاري تجريبي (Phase 4) — يُربَط بعقد الإيجار التجريبي أدناه لتوليد
        // عمولة حقيقية تلقائيًا، تمامًا كما يحدث فعليًا عند تفعيل أي عقد له مسوّق.
        var sampleAgent = await context.Agents.FirstOrDefaultAsync(a => a.CompanyId == company.Id);
        if (sampleAgent is null)
        {
            sampleAgent = new Agent
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                AgentCode = "AGENT-000001",
                NameAr = "فهد بن ناصر القحطاني",
                Mobile = "0566666666",
                FalLicenseNumber = "FAL-100001",
                FalLicenseExpiryDate = DateTime.UtcNow.AddYears(1),
                Status = AgentStatus.Active,
                CommissionSchemeType = CommissionType.Percentage,
                DefaultCommissionPercentage = 5,
                IsActive = true
            };
            context.Agents.Add(sampleAgent);
            await context.SaveChangesAsync();
        }

        // 7) مشترٍ تجريبي بمعايير بحث تُستخدم فعليًا في محرك المطابقة البسيط (Buyer Matching).
        var sampleBuyer = await context.Buyers.FirstOrDefaultAsync(b => b.CompanyId == company.Id);
        if (sampleBuyer is null)
        {
            sampleBuyer = new Buyer
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                BuyerCode = "BUYER-000001",
                NameAr = "منيرة بنت عبدالعزيز الدوسري",
                Mobile = "0577777777",
                Budget = 900_000,
                PreferredCity = "الرياض",
                Purpose = BuyerPurpose.PersonalUse,
                FinancingStatus = FinancingStatus.BankFinancing,
                AssignedAgentId = sampleAgent.Id,
                IsActive = true
            };
            context.Buyers.Add(sampleBuyer);
            await context.SaveChangesAsync();
        }

        // 8) عميل محتمل تجريبي (Lead) لتغطية نقطة الدخول المركزية لـ CRM النظام.
        var sampleLead = await context.Leads.FirstOrDefaultAsync(l => l.CompanyId == company.Id);
        if (sampleLead is null)
        {
            sampleLead = new Lead
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                LeadCode = "LEAD-000001",
                NameAr = "عبدالرحمن بن سالم الغامدي",
                Mobile = "0588888888",
                Source = LeadSource.Website,
                LeadType = LeadType.Buyer,
                AssignedAgentId = sampleAgent.Id,
                Status = LeadStatus.Contacted,
                Priority = LeadPriority.High
            };
            context.Leads.Add(sampleLead);
            await context.SaveChangesAsync();
        }

        // 9) مستأجر وعقد إيجار تجريبيان — يغطيان جدول السداد التلقائي ودفعة مسددة جزئيًا،
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
                AgentId = sampleAgent.Id,
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

            // عمولة المسوّق الناتجة عن تفعيل العقد أعلاه — نفس المعادلة التي يطبّقها
            // ActivateLeaseCommand تلقائيًا (تُدرَج هنا يدويًا لأن هذا العقد يُزرَع
            // مباشرة بحالة Active دون المرور بالأمر نفسه).
            const decimal commissionAmount = 35000 * 5 / 100m; // 1750
            const decimal vatAmount = commissionAmount * 15 / 100m; // 262.5
            context.Commissions.Add(new Commission
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                CommissionNumber = "COMM-000001",
                AgentId = sampleAgent.Id,
                SourceType = CommissionSourceType.Lease,
                LeaseId = sampleLease.Id,
                BaseAmount = 35000,
                CommissionPercentage = 5,
                CommissionAmount = commissionAmount,
                VatPercentage = 15,
                VatAmount = vatAmount,
                NetCommissionAmount = commissionAmount + vatAmount,
                Status = CommissionStatus.Pending
            });

            await context.SaveChangesAsync();
        }

        // 10) بيانات تجريبية لموديولات Phase 5 (Listings/Marketing/Viewings/Offers/Sales) —
        // تغطي مسار بيع كامل من الإعلان حتى الإتمام مع توليد عمولة تلقائي، بنفس فلسفة عقد
        // الإيجار التجريبي أعلاه، لتسهيل اختبار المسار الكامل من الواجهة الأمامية مباشرة.
        if (!await context.Sellers.AnyAsync(s => s.CompanyId == company.Id))
        {
            var saleProperty = new Property
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PropertyCode = "PROP-000002",
                PropertyName = "فيلا الأندلس - حي النرجس",
                PropertyType = PropertyType.Villa,
                PropertyCategory = PropertyCategory.Residential,
                Status = PropertyStatus.Active,
                OwnerId = sampleOwner.Id,
                City = "الرياض",
                District = "النرجس",
                NumberOfFloors = 2,
                TotalArea = 450
            };
            context.Properties.Add(saleProperty);
            await context.SaveChangesAsync();

            var saleUnit = new Unit
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PropertyId = saleProperty.Id,
                UnitCode = "UNIT-000002",
                UnitNumber = "1",
                UnitType = UnitType.Villa,
                CurrentStatus = UnitStatus.Available,
                Area = 400,
                Bedrooms = 5,
                Bathrooms = 4,
                SalePrice = 950_000
            };
            context.Units.Add(saleUnit);
            await context.SaveChangesAsync();

            var seller = new Seller
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                SellerCode = "SELLER-000001",
                OwnerId = sampleOwner.Id,
                PropertyId = saleProperty.Id,
                AskingPrice = 1_000_000,
                MinimumPrice = 900_000,
                CommissionPercentage = 2.5m,
                MandateStatus = ListingMandateStatus.Active,
                MandateStartDate = DateTime.UtcNow.AddMonths(-2),
                AssignedAgentId = sampleAgent.Id
            };
            context.Sellers.Add(seller);

            var listing = new Listing
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                ListingCode = "LIST-000001",
                PropertyId = saleProperty.Id,
                UnitId = saleUnit.Id,
                ListingType = ListingType.ForSale,
                Price = 1_000_000,
                Description = "فيلا فاخرة 5 غرف نوم في حي النرجس، تشطيب سوبر لوكس.",
                Features = "مسبح، مصعد داخلي، غرفة سائق",
                AgentId = sampleAgent.Id,
                ListingStartDate = DateTime.UtcNow.AddMonths(-2),
                Status = ListingStatus.Published
            };
            context.Listings.Add(listing);
            await context.SaveChangesAsync();

            var campaign = new MarketingCampaign
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                CampaignCode = "CAMP-000001",
                Name = "حملة إطلاق فيلا الأندلس",
                Channel = MarketingChannel.Instagram,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(-1),
                Budget = 5_000,
                ActualCost = 4_200,
                PropertyId = saleProperty.Id,
                AgentId = sampleAgent.Id,
                IsActive = false
            };
            context.MarketingCampaigns.Add(campaign);
            await context.SaveChangesAsync();

            if (sampleLead.CampaignId is null)
            {
                sampleLead.CampaignId = campaign.Id;
            }

            var viewing = new Viewing
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                ViewingCode = "VIEW-000001",
                PropertyId = saleProperty.Id,
                UnitId = saleUnit.Id,
                ListingId = listing.Id,
                BuyerId = sampleBuyer.Id,
                AgentId = sampleAgent.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(-20),
                Status = Domain.Common.Enums.ViewingStatus.Completed,
                Feedback = "أُعجب المشتري بالفيلا وأبدى رغبته بتقديم عرض."
            };
            context.Viewings.Add(viewing);

            var offer = new Offer
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                OfferNumber = "OFFER-000001",
                BuyerId = sampleBuyer.Id,
                PropertyId = saleProperty.Id,
                UnitId = saleUnit.Id,
                Amount = 950_000,
                OfferDate = DateTime.UtcNow.AddDays(-15),
                Status = Domain.Common.Enums.OfferStatus.Accepted
            };
            context.Offers.Add(offer);
            await context.SaveChangesAsync();

            const decimal saleCommissionAmount = 950_000 * 2.5m / 100m; // 23,750
            const decimal saleVatAmount = saleCommissionAmount * 15 / 100m; // 3,562.5

            var sale = new Sale
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                SaleNumber = "SALE-000001",
                PropertyId = saleProperty.Id,
                UnitId = saleUnit.Id,
                SellerId = seller.Id,
                BuyerId = sampleBuyer.Id,
                AgentId = sampleAgent.Id,
                OfferId = offer.Id,
                AskingPrice = 1_000_000,
                FinalPrice = 950_000,
                CommissionPercentage = 2.5m,
                VatPercentage = 15,
                Stage = SaleStage.Completed,
                CompletedAt = DateTime.UtcNow.AddDays(-5)
            };
            context.Sales.Add(sale);
            saleUnit.CurrentStatus = UnitStatus.Sold;
            await context.SaveChangesAsync();

            context.Commissions.Add(new Commission
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                CommissionNumber = "COMM-000002",
                AgentId = sampleAgent.Id,
                SourceType = CommissionSourceType.Sale,
                SaleId = sale.Id,
                BaseAmount = 950_000,
                CommissionPercentage = 2.5m,
                CommissionAmount = saleCommissionAmount,
                VatPercentage = 15,
                VatAmount = saleVatAmount,
                NetCommissionAmount = saleCommissionAmount + saleVatAmount,
                Status = CommissionStatus.Pending
            });

            await context.SaveChangesAsync();
        }

        // 11) بيانات تجريبية لموديولات Phase 6 (Maintenance/Employees/Vendors/Quotations) —
        // تغطي دورة عمل طلب صيانة كاملة: إنشاء → إسناد → عروض أسعار متعددة → اعتماد أحدها
        // (مع رفض الآخر تلقائيًا) → اكتمال، على العقار/الوحدة التجريبيين في الخطوة 5.
        if (sampleProperty is not null && sampleUnit is not null && !await context.Vendors.AnyAsync(v => v.CompanyId == company.Id))
        {
            var employee = new MaintenanceEmployee
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                EmployeeCode = "EMP-000001",
                NameAr = "سلطان بن فهد المطيري",
                Mobile = "0533333333",
                Department = "الصيانة الكهربائية والتكييف",
                Skills = "كهرباء، تكييف مركزي",
                IsAvailable = true,
                IsActive = true
            };
            context.MaintenanceEmployees.Add(employee);

            var vendorApproved = new Vendor
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                VendorCode = "VEND-000001",
                NameAr = "مؤسسة الصيانة المتكاملة",
                ContactPerson = "ماجد العنزي",
                Mobile = "0544444444",
                CommercialRegistrationNumber = "1010999999",
                VatNumber = "300999999900003",
                Services = "صيانة كهربائية وسباكة وتكييف",
                Rating = 4.5m,
                IsActive = true
            };
            var vendorRejected = new Vendor
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                VendorCode = "VEND-000002",
                NameAr = "شركة الخليج للصيانة الفنية",
                ContactPerson = "ياسر الحربي",
                Mobile = "0555111111",
                Services = "صيانة عامة",
                Rating = 3.8m,
                IsActive = true
            };
            context.Vendors.AddRange(vendorApproved, vendorRejected);
            await context.SaveChangesAsync();

            var maintenanceRequest = new MaintenanceRequest
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                RequestNumber = "MAINT-000001",
                PropertyId = sampleProperty.Id,
                UnitId = sampleUnit.Id,
                OwnerId = sampleOwner.Id,
                RequestType = MaintenanceRequestType.AC,
                Priority = MaintenancePriority.High,
                Description = "تسريب مياه من وحدة التكييف الداخلية في غرفة المعيشة.",
                AssignedEmployeeId = employee.Id,
                AssignedVendorId = vendorApproved.Id,
                EstimatedCost = 575,
                Status = MaintenanceStatus.Approved
            };
            context.MaintenanceRequests.Add(maintenanceRequest);
            await context.SaveChangesAsync();

            var approvedQuotation = new MaintenanceQuotation
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                QuotationNumber = "QUOT-000001",
                VendorId = vendorApproved.Id,
                MaintenanceRequestId = maintenanceRequest.Id,
                ValidUntil = DateTime.UtcNow.AddDays(15),
                VatPercentage = 15,
                SubtotalAmount = 500,
                VatAmount = 75,
                TotalAmount = 575,
                Status = QuotationStatus.Approved,
                Items =
                {
                    new MaintenanceQuotationItem { Description = "قطعة غيار كمبروسر", Quantity = 1, UnitPrice = 300, LineTotal = 300 },
                    new MaintenanceQuotationItem { Description = "أجرة فني (ساعتان)", Quantity = 2, UnitPrice = 100, LineTotal = 200 }
                }
            };
            var rejectedQuotation = new MaintenanceQuotation
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                QuotationNumber = "QUOT-000002",
                VendorId = vendorRejected.Id,
                MaintenanceRequestId = maintenanceRequest.Id,
                ValidUntil = DateTime.UtcNow.AddDays(10),
                VatPercentage = 15,
                SubtotalAmount = 650,
                VatAmount = 97.5m,
                TotalAmount = 747.5m,
                Status = QuotationStatus.Rejected,
                Items =
                {
                    new MaintenanceQuotationItem { Description = "قطعة غيار كمبروسر (بديل)", Quantity = 1, UnitPrice = 450, LineTotal = 450 },
                    new MaintenanceQuotationItem { Description = "أجرة فني (ساعتان)", Quantity = 2, UnitPrice = 100, LineTotal = 200 }
                }
            };
            context.MaintenanceQuotations.AddRange(approvedQuotation, rejectedQuotation);

            await context.SaveChangesAsync();
        }

        // 12) بيانات تجريبية لموديول Phase 7 (Auctions) — مزاد كامل من الإنشاء وحتى
        // التسوية المالية، مع سجل تدقيق (Append-Only) يغطي كل مرحلة، وعمولة تلقائية
        // للمسوّق عند الإرساء بنفس فلسفة عقد الإيجار والبيع أعلاه.
        if (sampleOwner is not null && !await context.Auctions.AnyAsync(a => a.CompanyId == company.Id))
        {
            var auctionProperty = new Property
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                PropertyCode = "PROP-000003",
                PropertyName = "أرض تجارية - طريق الملك فهد",
                PropertyType = PropertyType.Land,
                PropertyCategory = PropertyCategory.Commercial,
                Status = PropertyStatus.Active,
                OwnerId = sampleOwner.Id,
                City = "الرياض",
                District = "العليا",
                TotalArea = 2500
            };
            context.Properties.Add(auctionProperty);
            await context.SaveChangesAsync();

            const decimal auctionFinalPrice = 2_300_000m;
            const decimal auctionCommissionAmount = auctionFinalPrice * 2 / 100m; // 46,000
            const decimal auctionVatAmount = auctionCommissionAmount * 15 / 100m; // 6,900

            var auctionStart = DateTime.UtcNow.AddDays(-30);
            var auctionEnd = DateTime.UtcNow.AddDays(-23);

            var auction = new Auction
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                AuctionNumber = "AUCT-000001",
                PropertyId = auctionProperty.Id,
                OwnerId = sampleOwner.Id,
                AgentId = sampleAgent.Id,
                StartDate = auctionStart,
                EndDate = auctionEnd,
                StartingPrice = 2_000_000,
                ReservePrice = 2_100_000,
                DepositAmount = 100_000,
                CommissionPercentage = 2,
                VatPercentage = 15,
                Status = AuctionStatus.Settled,
                WinnerBuyerId = sampleBuyer.Id,
                FinalPrice = auctionFinalPrice,
                CurrentBidAmount = auctionFinalPrice,
                BidsCount = 12,
                SettledAt = auctionEnd.AddDays(3),
                Notes = "مزاد علني لأرض تجارية على طريق الملك فهد."
            };

            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionCreated, OccurredAt = auctionStart.AddDays(-5), Notes = "تم إنشاء المزاد كمسودة." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionApproved, OccurredAt = auctionStart.AddDays(-3), Notes = "تم اعتماد المزاد." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionPublished, OccurredAt = auctionStart.AddDays(-1), Notes = "تم نشر المزاد." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionWentLive, OccurredAt = auctionStart, Notes = "بدأ المزاد فعليًا." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionEnded, OccurredAt = auctionEnd, Notes = "انتهى وقت المزايدة." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionAwarded, OccurredAt = auctionEnd.AddHours(2), Notes = $"أُرسي المزاد بسعر نهائي {auctionFinalPrice:N2}." });
            auction.AuditLogs.Add(new AuctionAuditLog { CompanyId = company.Id, BranchId = mainBranch.Id, EventType = AuctionEventType.AuctionSettled, OccurredAt = auctionEnd.AddDays(3), Notes = "تمت التسوية المالية النهائية." });

            context.Auctions.Add(auction);
            await context.SaveChangesAsync();

            context.Commissions.Add(new Commission
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                CommissionNumber = "COMM-000003",
                AgentId = sampleAgent.Id,
                SourceType = CommissionSourceType.Auction,
                AuctionId = auction.Id,
                BaseAmount = auctionFinalPrice,
                CommissionPercentage = 2,
                CommissionAmount = auctionCommissionAmount,
                VatPercentage = 15,
                VatAmount = auctionVatAmount,
                NetCommissionAmount = auctionCommissionAmount + auctionVatAmount,
                Status = CommissionStatus.Pending
            });

            await context.SaveChangesAsync();
        }

        // 13) إشعارات تجريبية لموديول Phase 8 (Notifications) — إشعار عام على مستوى الشركة
        // وآخر مقروء مسبقًا، لتغطية شارة "غير مقروء" وقائمة الإشعارات فور أول تشغيل.
        if (!await context.Notifications.AnyAsync(n => n.CompanyId == company.Id))
        {
            context.Notifications.Add(new Notification
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                UserId = null,
                Type = NotificationType.System,
                Title = "مرحبًا بك في نظام فلك الخير العقارية",
                Message = "تم إعداد بيانات تجريبية لجميع الموديولات لتسهيل الاستكشاف الأولي للنظام.",
                Link = "/dashboard",
                IsRead = false
            });

            context.Notifications.Add(new Notification
            {
                CompanyId = company.Id,
                BranchId = mainBranch.Id,
                UserId = null,
                Type = NotificationType.MaintenanceRequestUrgent,
                Title = "طلب صيانة عاجل",
                Message = "طلب صيانة بأولوية High على الوحدة \"101\": تسريب مياه من وحدة التكييف الداخلية في غرفة المعيشة.",
                Link = "/maintenance",
                IsRead = true,
                ReadAt = DateTime.UtcNow.AddDays(-1)
            });

            await context.SaveChangesAsync();
        }

        // 14) مزامنة عدّادات الترقيم المرجعي (NumberSequences) مع الأكواد المزروعة يدويًا
        // أعلاه (مثال: "LEAD-000001"). بيانات البذر تُدرَج مباشرة بأكواد ثابتة دون المرور
        // بـ NumberGeneratorService، فإن لم تُزامَن العدّادات هنا، أول طلب فعلي عبر الـ API
        // لنفس النوع يولّد نفس الكود المستخدم مسبقًا فيصطدم بقيد التفرّد (Unique Index).
        await EnsureNumberSequenceSeededAsync(context, company.Id, "PROPERTY", "PROP", 3);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "UNIT", "UNIT", 2);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "OWNER", "OWNER", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "TEN", "TEN", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "LEASE", "LEASE", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "PAY", "PAY", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "AGENT", "AGENT", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "BUYER", "BUYER", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "LEAD", "LEAD", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "COMM", "COMM", 3);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "SELLER", "SELLER", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "LIST", "LIST", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "CAMP", "CAMP", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "VIEW", "VIEW", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "OFFER", "OFFER", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "SALE", "SALE", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "EMP", "EMP", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "VEND", "VEND", 2);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "MAINT", "MAINT", 1);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "QUOT", "QUOT", 2);
        await EnsureNumberSequenceSeededAsync(context, company.Id, "AUCT", "AUCT", 1);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// يضمن وجود عدّاد (NumberSequence) لنوع مُعطى بحيث لا يقل CurrentNumber عن
    /// minCurrentNumber (عدد السجلات المزروعة يدويًا بأكواد ثابتة لهذا النوع)،
    /// دون إنقاص عدّاد موجود بالفعل بقيمة أعلى (مثال: بعد إنشاء سجلات فعلية عبر الـ API).
    /// </summary>
    private static async Task EnsureNumberSequenceSeededAsync(
        ApplicationDbContext context, Guid companyId, string entityKey, string prefix, long minCurrentNumber)
    {
        var sequence = await context.NumberSequences
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.EntityKey == entityKey);

        if (sequence is null)
        {
            context.NumberSequences.Add(new NumberSequence
            {
                CompanyId = companyId,
                EntityKey = entityKey,
                Prefix = prefix,
                CurrentNumber = minCurrentNumber,
                PaddingLength = 6
            });
        }
        else if (sequence.CurrentNumber < minCurrentNumber)
        {
            sequence.CurrentNumber = minCurrentNumber;
        }
    }
}
