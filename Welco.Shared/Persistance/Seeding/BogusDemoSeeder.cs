using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Welco.Shared.Persistance.Seeding
{
    /// <summary>
    /// Development-only demo data seeder (Bogus). Runs ONLY when the host
    /// environment is Development AND the SEED_DEMO_DATA environment variable
    /// is "true". Never runs in Production (defense-in-depth check inside).
    /// Idempotent: skips entirely when marker rows (CreatedBy = "BogusSeeder")
    /// already exist; re-runs are no-ops. Deterministic faker seed (1234).
    /// </summary>
    public static class BogusDemoSeeder
    {
        public const string Marker = "BogusSeeder";
        private const int FakerSeed = 1234;

        private static readonly (string En, string Ar, string[] ChildEn, string[] ChildAr)[] Specialties =
        {
            ("Dental", "طب الأسنان",
                new[] { "Forceps", "Elevators", "Mirrors" },
                new[] { "ملاقط", "روافع", "مرايا" }),
            ("Orthopedic", "جراحة العظام",
                new[] { "Bone Saws", "Retractors", "Osteotomes" },
                new[] { "مناشير العظام", "مبعدات", "أزاميل العظام" }),
            ("Spine", "جراحة العمود الفقري",
                new[] { "Rongeurs", "Kerrisons", "Probes" },
                new[] { "قوارض", "ملاقط كيريسون", "مسابر" }),
            ("Plastic Surgery", "جراحة التجميل",
                new[] { "Scissors", "Needle Holders", "Calipers" },
                new[] { "مقصات", "حاملات الإبر", "فرجارات" }),
            ("CMF", "جراحة الوجه والفكين",
                new[] { "Mini Plates", "Screws", "Periosteals" },
                new[] { "صفائح مصغرة", "براغي", "روافع السمحاق" }),
            ("ENT", "الأنف والأذن والحنجرة",
                new[] { "Speculums", "Suction Tubes", "Curettes" },
                new[] { "مناظير", "أنابيب الشفط", "مكحتات" }),
        };

        private static readonly string[] InstrumentEn =
        {
            "Mayo Scissors", "Metzenbaum Scissors", "Kelly Forceps", "Needle Holder",
            "Army Retractor", "Scalpel Handle", "Towel Clamp", "Halsted Hemostat",
            "Bone Rongeur", "Periosteal Elevator", "Mouth Mirror", "Explorer Probe",
            "Nasal Speculum", "Osteotome Set", "Castroviejo Calipers", "Yankauer Suction",
            "Adson Forceps", "Iris Scissors", "Dental Syringe", "Bone File",
        };

        private static readonly string[] InstrumentAr =
        {
            "مشرط جراحي", "ملقط طبي", "مقص جراحي", "مبعد", "مسبار",
            "مبضع", "حامل إبر", "مرآة فم", "محقنة", "مبرد عظام",
            "أنبوب شفط", "فرجار قياس",
        };

        private static readonly string[] InstrumentModifiers =
            { "Standard", "Premium", "Deluxe", "Slim", "Curved", "Straight" };

        private static readonly string[] Materials =
            { "German Stainless Steel", "Titanium", "Tungsten Carbide", "AISI 420 Steel" };

        private static readonly string[] ProcedureTags =
        {
            "Oral Surgery", "Implantology", "Orthodontics", "Trauma",
            "Arthroscopy", "Spine Fusion", "Rhinoplasty", "Otology",
        };

        private static readonly (string Name, string ImageName, CompanyType Type)[] CompanySeedList =
        {
            ("Apex Surgical Supplies", "apex-surgical.svg", CompanyType.Distributor),
            ("MedCore Distributors", "medcore.svg", CompanyType.Distributor),
            ("Gulf Medical Trading", "gulf-medical.svg", CompanyType.Distributor),
            ("CityCare Hospitals Group", "citycare.svg", CompanyType.Hospital),
            ("Nova Health Clinic", "nova-health.svg", CompanyType.Clinic),
            ("PrimeCare Medical", "primecare.svg", CompanyType.Clinic),
            ("Sahara Med Import", "sahara-med.svg", CompanyType.Distributor),
            ("Delta Surgical Co.", "delta-surgical.svg", CompanyType.Distributor),
            ("LifeLine Hospitals", "lifeline.svg", CompanyType.Hospital),
            ("OrthoPlus Distributors", "orthoplus.svg", CompanyType.Distributor),
            ("CarePoint Clinics", "carepoint.svg", CompanyType.Clinic),
            ("Meridian Med Import", "meridian.svg", CompanyType.Distributor),
        };

        private static readonly string[] CompanyNames =
        {
            "Apex Surgical Supplies", "MedCore Distributors", "Gulf Medical Trading",
            "CityCare Hospitals Group", "Nova Health Clinic", "PrimeCare Medical",
            "Sahara Med Import", "Delta Surgical Co.", "LifeLine Hospitals",
            "OrthoPlus Distributors", "CarePoint Clinics", "Meridian Med Import",
        };

        private static readonly string[] OemServices =
            { "Private Label", "Custom Development", "Laser Marking", "Packaging", "Branding" };

        private static string Slugify(string value)
        {
            var s = (value ?? string.Empty).Trim().ToLowerInvariant().Replace(' ', '-').Replace('_', '-');
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var ch in s)
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-')
                    sb.Append(ch);
            var slug = sb.ToString();
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            return slug.Trim('-');
        }

        /// <summary>
        /// Crash-safe random subset: Bogus <c>PickRandom(items, n)</c> throws when
        /// <c>n</c> exceeds the collection size (e.g. a nearly-empty table), so the
        /// count is clamped to what actually exists.
        /// </summary>
        private static List<T> PickSome<T>(Faker faker, IList<T> source, int min, int max)
        {
            if (source.Count == 0) return new List<T>();
            var upper = Math.Min(max, source.Count);
            var n = upper <= min ? source.Count : faker.Random.Int(min, upper);
            return faker.PickRandom(source, n).Distinct().ToList();
        }

        private static string DemoPassword(IConfiguration? config = null)
        {
            var fromConfig = config?["Seeding:DemoPassword"];
            if (!string.IsNullOrWhiteSpace(fromConfig))
                return fromConfig!;
            return Environment.GetEnvironmentVariable("SEED_DEMO_PASSWORD") is { Length: > 0 } pwd
                ? pwd
                : "Demo123!";
        }

        /// <summary>
        /// Central opt-in check. Demo data runs ONLY in non-Production AND when
        /// explicitly enabled via <c>Seeding:SeedDemoData=true</c> (appsettings /
        /// user-secrets / command-line <c>--Seeding:SeedDemoData true</c>) OR the
        /// legacy <c>SEED_DEMO_DATA=true</c> environment variable.
        /// </summary>
        public static bool ShouldSeedDemoData(IHostEnvironment? env, IConfiguration? config, out string reason)
        {
            if (env != null && env.IsProduction())
            {
                reason = "refused: Production environment (Bogus demo data never runs in Production).";
                return false;
            }

            var fromConfig = config?.GetValue<bool>("Seeding:SeedDemoData") == true;
            var fromEnvVar = string.Equals(
                Environment.GetEnvironmentVariable("SEED_DEMO_DATA"),
                "true", StringComparison.OrdinalIgnoreCase);
            // Covers AddEnvironmentVariables mapping (SEED_DEMO_DATA is also visible
            // here) plus docker-style Seeding__SeedDemoData=true.
            var fromConfigString = string.Equals(
                config?["SEED_DEMO_DATA"],
                "true", StringComparison.OrdinalIgnoreCase);

            if (fromConfig || fromEnvVar || fromConfigString)
            {
                reason = fromConfig
                    ? "enabled via Seeding:SeedDemoData=true."
                    : "enabled via SEED_DEMO_DATA=true.";
                return true;
            }

            reason = "skipped: set \"Seeding\": { \"SeedDemoData\": true } in appsettings.Development.json "
                + "(or user-secrets / --Seeding:SeedDemoData true) or SEED_DEMO_DATA=true env var.";
            return false;
        }

        public static async Task SeedDemoAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
        {
            try
            {
                var env = services.GetService<IHostEnvironment>();
                var config = services.GetService<IConfiguration>();
                if (!ShouldSeedDemoData(env, config, out var gateReason))
                {
                    logger.LogWarning("BogusDemoSeeder {Reason}", gateReason);
                    return;
                }

                var db = services.GetRequiredService<WelcoDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                // Self-sufficient: ensure Identity roles (incl. Customer) exist
                // even when the host service never ran RoleSeeder (e.g. Product).
                var roleManager = services.GetService<RoleManager<IdentityRole<Guid>>>();
                if (roleManager != null)
                    await RoleSeeder.SeedRolesAsync(roleManager, logger);

                Randomizer.Seed = new Random(FakerSeed);
                var faker = new Faker("en");
                var year = DateTime.UtcNow.Year;

                // Reference data needed by several sections below.
                var usd = await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted && c.Code == "USD", ct)
                    ?? await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted, ct);
                if (usd == null)
                {
                    logger.LogInformation("USD currency missing; running CurrencySeeder...");
                    await CurrencySeeder.SeedAsync(db, logger);
                    usd = await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted && c.Code == "USD", ct)
                        ?? await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted, ct);
                }

                var countries = await db.Countries.Where(c => !c.IsDeleted).ToListAsync(ct);
                if (countries.Count == 0)
                {
                    logger.LogInformation("No countries found; running WorldLocationSeeder...");
                    await WorldLocationSeeder.SeedAsync(db, logger);
                    countries = await db.Countries.Where(c => !c.IsDeleted).ToListAsync(ct);
                }
                if (countries.Count == 0)
                {
                    logger.LogWarning("Bogus seeding stopped: no countries found (run WorldLocationSeeder first).");
                    return;
                }

                // ── 1. Categories (6 roots + 18 children) ────────────────────
                // Guarded by emptiness (names/SKUs below are deterministic, so a
                // re-run or a legacy-seeded DB must not insert them twice).
                List<Category> leaves;
                if (await db.Categories.AnyAsync(c => !c.IsDeleted, ct))
                {
                    logger.LogInformation("Categories already present, skipping Bogus category seeding.");
                    leaves = await db.Categories.Where(c => !c.IsDeleted && c.ParentCategoryId != null).ToListAsync(ct);
                    if (leaves.Count == 0)
                        leaves = await db.Categories.Where(c => !c.IsDeleted).ToListAsync(ct);
                }
                else
                {
                    var categories = new List<Category>();
                    foreach (var (en, ar, childEn, childAr) in Specialties)
                    {
                        var root = Category.Create(en, ar, $"{en} surgical instruments", null, null, Marker);
                        categories.Add(root);
                        for (var i = 0; i < childEn.Length; i++)
                            categories.Add(Category.Create($"{en} {childEn[i]}", $"{ar} - {childAr[i]}", null, null, root.Id, Marker));
                    }
                    await db.Categories.AddRangeAsync(categories, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} categories.", categories.Count);
                    leaves = categories.Where(c => c.ParentCategoryId.HasValue).ToList();
                }
                List<Product> products;
                if (await db.Products.AnyAsync(p => !p.IsDeleted, ct))
                {
                    logger.LogInformation("Products already present, skipping Bogus product seeding.");
                    products = await db.Products.Where(p => !p.IsDeleted).Take(200).ToListAsync(ct);
                }
                else
                {
                    products = new List<Product>();
                var specs = new List<ProductSpecification>();
                var media = new List<ProductMedia>();
                var tags = new List<ProductProcedureTag>();
                var seq = 0;
                foreach (var leaf in leaves)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        seq++;
                        var nameEn = $"{faker.PickRandom(InstrumentEn)} {faker.PickRandom(InstrumentModifiers)}";
                        var sku = $"WL-{100000 + seq}";
                        var slug = $"{Slugify(nameEn)}-{seq}";
                        var price = Math.Round(faker.Random.Decimal(15, 2500), 2);
                        var length = faker.Random.Bool(0.7f) ? (decimal?)Math.Round(faker.Random.Decimal(10, 30), 1) : null;
                        var material = faker.PickRandom(Materials);
                        var product = Product.Create(
                            nameEn,
                            $"{faker.PickRandom(InstrumentAr)} {seq}",
                            sku, slug,
                            faker.Lorem.Sentence(8, 4),
                            price,
                            faker.Random.Int(0, 300),
                            $"Autoclave 134°C; DIN 1.4021{(length.HasValue ? $"; {length} cm" : string.Empty)}",
                            $"demo/products/{slug}.jpg",
                            material, length,
                            usd?.Id,
                            leaf.Id, Marker);
                        products.Add(product);
                        specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Material", AttrValue = material });
                        specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Sterilization", AttrValue = "Autoclave 134°C" });
                        specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Length", AttrValue = length.HasValue ? $"{length} cm" : "Standard" });
                        foreach (var s in specs.TakeLast(3)) s.MarkAsCreated(Marker);
                        media.Add(new ProductMedia { Id = Guid.NewGuid(), ProductId = product.Id, Type = ProductMediaType.Image, Url = $"demo/products/{slug}.jpg", SortOrder = 0 });
                        media.Last().MarkAsCreated(Marker);
                        if (faker.Random.Bool(0.3f))
                        {
                            media.Add(new ProductMedia { Id = Guid.NewGuid(), ProductId = product.Id, Type = ProductMediaType.Image, Url = $"demo/products/{slug}-2.jpg", SortOrder = 1 });
                            media.Last().MarkAsCreated(Marker);
                        }
                        foreach (var tag in faker.PickRandom(ProcedureTags, faker.Random.Int(1, 2)).Distinct())
                        {
                            var t = new ProductProcedureTag { Id = Guid.NewGuid(), ProductId = product.Id, Label = tag };
                            t.MarkAsCreated(Marker);
                            tags.Add(t);
                        }
                    }
                }
                await db.Products.AddRangeAsync(products, ct);
                await db.ProductSpecifications.AddRangeAsync(specs, ct);
                await db.ProductMedias.AddRangeAsync(media, ct);
                await db.ProductProcedureTags.AddRangeAsync(tags, ct);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Bogus seeded {Count} products (+specs/media/tags).", products.Count);
                }

                List<Company> companies;
                if (await db.Companies.AnyAsync(c => !c.IsDeleted, ct))
                {
                    logger.LogInformation("Companies already present, updating existing data with actual images and ensuring IsProvider = true.");
                    companies = await db.Companies.Where(c => !c.IsDeleted).Take(20).ToListAsync(ct);
                    var updatedExisting = false;
                    for (var idx = 0; idx < companies.Count; idx++)
                    {
                        var comp = companies[idx];
                        if (!comp.IsProvider)
                        {
                            comp.IsProvider = true;
                            updatedExisting = true;
                        }
                        if (string.IsNullOrWhiteSpace(comp.ImageName))
                        {
                            var seedMatch = CompanySeedList.FirstOrDefault(s => s.Name.Equals(comp.Name, StringComparison.OrdinalIgnoreCase));
                            comp.ImageName = seedMatch.ImageName ?? CompanySeedList[idx % CompanySeedList.Length].ImageName;
                            updatedExisting = true;
                        }
                    }
                    if (updatedExisting)
                    {
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Successfully updated existing companies with actual images and provider access.");
                    }
                }
                else
                {
                    companies = new List<Company>();
                    for (var i = 0; i < CompanySeedList.Length; i++)
                    {
                        var seed = CompanySeedList[i];
                        var country = faker.PickRandom(countries);
                        var status = i == CompanySeedList.Length - 1 ? CompanyStatus.Pending : CompanyStatus.Approved;
                        var company = Company.Create(
                            seed.Name, seed.Type, country.Id,
                            status, null, Marker,
                            $"info@{Slugify(seed.Name)}.example.com",
                            seed.ImageName);
                        // All companies are providers and can upload products
                        company.IsProvider = true;
                        companies.Add(company);
                    }
                    await db.Companies.AddRangeAsync(companies, ct);
                    await db.SaveChangesAsync(ct);

                var addresses = new List<CompanyAddress>();
                foreach (var company in companies)
                {
                    var n = faker.Random.Int(1, 2);
                    for (var a = 0; a < n; a++)
                    {
                        var country = countries.FirstOrDefault(c => c.Id == company.CountryId) ?? faker.PickRandom(countries);
                        var city = await db.Cities.FirstOrDefaultAsync(c => !c.IsDeleted && c.CountryId == country.Id, ct)
                            ?? await db.Cities.FirstOrDefaultAsync(c => !c.IsDeleted, ct);
                        if (city == null) continue;
                        var zone = await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted && z.CityId == city.Id, ct)
                            ?? await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted, ct);
                        if (zone == null) continue;
                        addresses.Add(CompanyAddress.Create(
                            company.Id, country.Id, city.Id, zone.Id,
                            $"{faker.Random.Int(1, 200)} {faker.Address.StreetName()}",
                            faker.Random.Bool(0.5f) ? $"Bldg {faker.Random.Int(1, 50)}" : null,
                            faker.Random.Bool(0.4f) ? $"Fl {faker.Random.Int(1, 20)}" : null,
                            faker.Random.Bool(0.4f) ? $"Apt {faker.Random.Int(1, 100)}" : null,
                            Marker, isDefault: a == 0));
                    }
                }
                await db.CompanyAddresses.AddRangeAsync(addresses, ct);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Bogus seeded {Count} companies (+addresses).", companies.Count);
                }

                // ── 4. Users (via UserManager) ───────────────────────────────
                var password = DemoPassword(config);
                var staff = new List<ApplicationUser>();
                for (var i = 1; i <= 2; i++)
                {
                    var u = await EnsureUserAsync(userManager, logger, faker,
                        $"demo.staff{i:00}@welco.health", faker.Name.FullName(),
                        UserType.WelcoStaff, null, i % 2 == 0 ? AppLanguage.Ar : AppLanguage.En,
                        password, ct);
                    if (u != null) staff.Add(u);
                }
                var orgUsers = new List<(ApplicationUser User, Company Company)>();
                var approved = companies.Where(c => c.Status == CompanyStatus.Approved).ToList();
                for (var i = 0; i < approved.Count; i++)
                {
                    var u = await EnsureUserAsync(userManager, logger, faker,
                        $"demo.org{i + 1:00}@welco.health", faker.Name.FullName(),
                        UserType.OrganizationUser, approved[i].Id, AppLanguage.En, password, ct);
                    if (u != null) orgUsers.Add((u, approved[i]));

                    var u2 = await EnsureUserAsync(userManager, logger, faker,
                        $"demo.member{i + 1:00}@welco.health", faker.Name.FullName(),
                        UserType.OrganizationUser, approved[i].Id, i % 2 == 0 ? AppLanguage.Ar : AppLanguage.En, password, ct);
                    if (u2 != null) orgUsers.Add((u2, approved[i]));
                }

                // Migrate any legacy users with obsolete UserType (4) to OrganizationUser
                var legacyCustomers = await db.ApplicationUsers.Where(u => !u.IsDeleted && (int)u.UserType == 4).ToListAsync(ct);
                if (legacyCustomers.Count > 0 && approved.Count > 0)
                {
                    for (var i = 0; i < legacyCustomers.Count; i++)
                    {
                        var lc = legacyCustomers[i];
                        lc.UserType = UserType.OrganizationUser;
                        if (!lc.CompanyId.HasValue)
                            lc.CompanyId = approved[i % approved.Count].Id;
                    }
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Migrated {Count} legacy customer users to OrganizationUser.", legacyCustomers.Count);
                }

                logger.LogInformation("Bogus seeded {Staff} staff, {Org} org users.",
                    staff.Count, orgUsers.Count);

                // Fallbacks in case users were created on a prior run or pre-existing
                if (staff.Count == 0)
                    staff = await db.ApplicationUsers.Where(u => !u.IsDeleted && u.UserType == UserType.WelcoStaff).Take(5).ToListAsync(ct);
                if (orgUsers.Count == 0 && approved.Count > 0)
                {
                    var orgDbUsers = await db.ApplicationUsers.Where(u => !u.IsDeleted && u.CompanyId != null).ToListAsync(ct);
                    foreach (var ou in orgDbUsers)
                    {
                        var comp = approved.FirstOrDefault(c => c.Id == ou.CompanyId);
                        if (comp != null) orgUsers.Add((ou, comp));
                    }
                }

                var activeMembers = orgUsers.Select(x => x.User).ToList();
                if (activeMembers.Count == 0)
                    activeMembers = await db.ApplicationUsers.Where(u => !u.IsDeleted && u.UserType == UserType.OrganizationUser).Take(20).ToListAsync(ct);
                if (activeMembers.Count == 0)
                    activeMembers = await db.ApplicationUsers.Where(u => !u.IsDeleted).Take(20).ToListAsync(ct);

                // default address for active members (checkout needs one) —
                // skip members that already have one so re-runs stay no-ops.
                var membersWithAddress = new HashSet<Guid>(
                    await db.UserAddresses.Where(a => !a.IsDeleted).Select(a => a.UserId).ToListAsync(ct));
                var memberAddresses = new List<UserAddress>();
                foreach (var c in activeMembers)
                {
                    if (!membersWithAddress.Contains(c.Id))
                    {
                        var country = faker.PickRandom(countries);
                        var city = await db.Cities.FirstOrDefaultAsync(x => !x.IsDeleted && x.CountryId == country.Id, ct)
                            ?? await db.Cities.FirstOrDefaultAsync(x => !x.IsDeleted, ct);
                        if (city == null) continue;
                        var zone = await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted && z.CityId == city.Id, ct)
                            ?? await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted, ct);
                        if (zone == null) continue;
                        memberAddresses.Add(UserAddress.Create(c.Id, country.Id, city.Id, zone.Id,
                            $"{faker.Random.Int(1, 200)} {faker.Address.StreetName()}", null, null, null, Marker, isDefault: true));
                    }
                }
                if (memberAddresses.Count > 0)
                {
                    await db.UserAddresses.AddRangeAsync(memberAddresses, ct);
                    await db.SaveChangesAsync(ct);
                }

                // ── 5. RFQ → Quote → Order chains (12, on approved companies) ─
                // Numbers are deterministic per year with unique indexes → seed
                // only when no chain for this year (or no demo chain) exists.
                var repId = staff.Count > 0 ? staff[0].Id : Guid.NewGuid();
                var chainNo = 0;
                var chainPrefix = $"WO-{year}-";
                if (await db.Orders.AnyAsync(o => !o.IsDeleted && (o.CreatedBy == Marker || o.OrderNumber.StartsWith(chainPrefix)), ct))
                {
                    logger.LogInformation("Orders already present, skipping Bogus RFQ→Quote→Order chains.");
                }
                else
                {
                    foreach (var (user, company) in orgUsers.Take(products.Count > 0 ? 12 : 0))
                {
                    chainNo++;
                    var items = PickSome(faker, products, 2, 5);
                    var rfq = new RFQ
                    {
                        Id = Guid.NewGuid(),
                        RFQNumber = $"RFQ-{year}-{chainNo:0000}",
                        CompanyId = company.Id,
                        Status = RFQStatus.Ordered,
                        AssignedSalesRepId = staff.Count > 0 ? repId : null,
                    };
                    rfq.MarkAsCreated(Marker);
                    foreach (var p in items)
                    {
                        var ri = new RFQItem
                        {
                            Id = Guid.NewGuid(), RFQId = rfq.Id, ProductId = p.Id,
                            Quantity = faker.Random.Int(5, 200), UnitPrice = p.Price,
                            Notes = faker.Random.Bool(0.3f) ? "Urgent delivery requested" : null,
                        };
                        ri.MarkAsCreated(Marker);
                        rfq.Items.Add(ri);
                    }
                    var quoteTotal = Math.Round(rfq.Items.Sum(i => i.Quantity * i.UnitPrice) * (decimal)faker.Random.Double(0.92, 1.05), 2);
                    var quote = new Quote
                    {
                        Id = Guid.NewGuid(),
                        QuoteNumber = $"QT-{year}-{chainNo:0000}",
                        RFQId = rfq.Id,
                        Amount = quoteTotal,
                        ValidUntil = DateTime.UtcNow.AddDays(30),
                        Status = QuoteStatus.Approved,
                        CreatedBySalesRepId = repId,
                    };
                    quote.MarkAsCreated(Marker);
                    foreach (var ri in rfq.Items)
                    {
                        var qi = new QuoteItem
                        {
                            Id = Guid.NewGuid(), QuoteId = quote.Id, ProductId = ri.ProductId,
                            Quantity = ri.Quantity, UnitPrice = ri.UnitPrice,
                        };
                        qi.MarkAsCreated(Marker);
                        quote.Items.Add(qi);
                    }
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = $"WO-{year}-{chainNo:0000}",
                        Status = (OrderStatus)faker.Random.Int(2, 4),
                        UserId = user.Id,
                        CompanyId = company.Id,
                        CurrencyId = usd?.Id,
                        QuoteId = quote.Id,
                        TotalAmount = quoteTotal,
                        SnapshotBaseCurrency = "USD",
                        SnapshotCurrencyCode = usd?.Code ?? "USD",
                        SnapshotRate = 1,
                        SnapshotRateDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        SnapshotSource = Marker,
                    };
                    order.MarkAsCreated(Marker);
                    foreach (var qi in quote.Items)
                    {
                        var oi = new OrderItem
                        {
                            Id = Guid.NewGuid(), OrderId = order.Id, ProductId = qi.ProductId,
                            Quantity = qi.Quantity, UnitPrice = qi.UnitPrice,
                        };
                        oi.MarkAsCreated(Marker);
                        order.Items.Add(oi);
                    }
                    var invoice = new Invoice
                    {
                        Id = Guid.NewGuid(), OrderId = order.Id,
                        InvoiceNumber = $"INV-{year}-{chainNo:0000}",
                        Amount = quoteTotal,
                        Status = faker.Random.Bool(0.6f) ? InvoiceStatus.Paid : InvoiceStatus.Issued,
                    };
                    invoice.MarkAsCreated(Marker);
                    order.Invoices.Add(invoice);

                    await db.RFQs.AddAsync(rfq, ct);
                    await db.Quotes.AddAsync(quote, ct);
                    await db.Orders.AddAsync(order, ct);
                    }
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} RFQ→Quote→Order chains.", chainNo);
                }

                // ── 6. Help content, shows, inquiries, tickets, notifications ─
                // Each group is seeded only into an empty table (re-runs stay
                // no-ops; slugs below are unique-indexed and deterministic).
                var catEntities = new List<HelpCategory>();
                if (!await db.HelpCategories.AnyAsync(c => !c.IsDeleted, ct))
                {
                    var helpCats = new[] { "Ordering", "Shipping & Incoterms", "Returns & RMA", "Sterilization", "Warranty" };
                    var icons = new[] { "shopping_cart", "local_shipping", "restart_alt", "cleaning_services", "verified" };
                    for (var i = 0; i < helpCats.Length; i++)
                    {
                        var hc = new HelpCategory { Id = Guid.NewGuid(), Name = helpCats[i], Icon = icons[i] };
                        hc.MarkAsCreated(Marker);
                        catEntities.Add(hc);
                    }
                    await db.HelpCategories.AddRangeAsync(catEntities, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} help categories.", catEntities.Count);
                }
                else
                {
                    catEntities = await db.HelpCategories.Where(c => !c.IsDeleted).Take(10).ToListAsync(ct);
                }
                if (!await db.HelpArticles.AnyAsync(a => !a.IsDeleted, ct) && catEntities.Count > 0)
                {
                    var articles = new List<HelpArticle>();
                    foreach (var hc in catEntities)
                        for (var i = 1; i <= 3; i++)
                        {
                            var title = $"{hc.Name} guide part {i}: {faker.Lorem.Sentence(4, 2).TrimEnd('.')}";
                            var ha = new HelpArticle
                            {
                                Id = Guid.NewGuid(), CategoryId = hc.Id, Title = title,
                                Body = faker.Lorem.Paragraphs(2, 3),
                                Slug = $"{Slugify(hc.Name)}-part-{i}",
                            };
                            ha.MarkAsCreated(Marker);
                            articles.Add(ha);
                        }
                    await db.HelpArticles.AddRangeAsync(articles, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} help articles.", articles.Count);
                }
                if (!await db.FAQItems.AnyAsync(f => !f.IsDeleted, ct))
                {
                    var faqs = Enumerable.Range(1, 10).Select(i =>
                    {
                        var f = new FAQItem
                        {
                            Id = Guid.NewGuid(),
                            Question = faker.Lorem.Sentence(6, 3).TrimEnd('.') + "?",
                            Answer = faker.Lorem.Paragraph(2),
                            SortOrder = i,
                        };
                        f.MarkAsCreated(Marker);
                        return f;
                    }).ToList();
                    await db.FAQItems.AddRangeAsync(faqs, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} FAQs.", faqs.Count);
                }
                if (!await db.TradeShowEvents.AnyAsync(t => !t.IsDeleted, ct))
                {
                    var shows = new[]
                    {
                        ("Arab Health 2025", "Dubai, UAE", -210, -206),
                        ("Medica 2025", "Düsseldorf, Germany", -120, -116),
                        ("Arab Health 2026", "Dubai, UAE", 25, 29),
                        ("FIME 2026", "Miami, USA", 150, 153),
                    }.Select(s =>
                    {
                        var t = new TradeShowEvent
                        {
                            Id = Guid.NewGuid(), Name = s.Item1, Location = s.Item2,
                            StartDate = DateTime.UtcNow.AddDays(s.Item3), EndDate = DateTime.UtcNow.AddDays(s.Item4),
                        };
                        t.MarkAsCreated(Marker);
                        return t;
                    }).ToList();
                    await db.TradeShowEvents.AddRangeAsync(shows, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} trade shows.", shows.Count);
                }
                if (!await db.ProductInquiries.AnyAsync(p => !p.IsDeleted, ct) && products.Count > 0)
                {
                    var inquiries = Enumerable.Range(1, 6).Select(_ =>
                    {
                        var p = faker.PickRandom(products);
                        var pi = new ProductInquiry
                        {
                            Id = Guid.NewGuid(), ProductId = p.Id, Name = faker.Name.FullName(),
                            Organization = faker.Company.CompanyName(), Message = faker.Lorem.Paragraph(1),
                            Email = faker.Internet.Email(),
                        };
                        pi.MarkAsCreated(Marker);
                        return pi;
                    }).ToList();
                    await db.ProductInquiries.AddRangeAsync(inquiries, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} product inquiries.", inquiries.Count);
                }
                if (!await db.OemInquiries.AnyAsync(o => !o.IsDeleted, ct))
                {
                    var oem = OemServices.Take(4).Select(s =>
                    {
                        var o = new OemInquiry
                        {
                            Id = Guid.NewGuid(), FullName = faker.Name.FullName(), Email = faker.Internet.Email(),
                            CompanyName = faker.Company.CompanyName(), ServiceType = s, Message = faker.Lorem.Paragraph(1),
                        };
                        o.MarkAsCreated(Marker);
                        return o;
                    }).ToList();
                    await db.OemInquiries.AddRangeAsync(oem, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} OEM inquiries.", oem.Count);
                }

                if (!await db.SupportTickets.AnyAsync(t => !t.IsDeleted, ct) && activeMembers.Count > 0)
                {
                    var ticketUsers = activeMembers.Take(8).ToList();
                    var statuses = new[] { "Open", "Answered", "Closed" };
                    var tickets = new List<SupportTicket>();
                    foreach (var tu in ticketUsers)
                    {
                        var st = faker.PickRandom(statuses);
                        var t = new SupportTicket
                        {
                            Id = Guid.NewGuid(), UserId = tu.Id,
                            Subject = faker.Lorem.Sentence(5, 2).TrimEnd('.'),
                            Message = faker.Lorem.Paragraph(1), Status = st,
                            Reply = st == "Open" ? null : faker.Lorem.Paragraph(1),
                            RepliedAt = st == "Open" ? null : DateTime.UtcNow.AddDays(-faker.Random.Int(1, 9)),
                            RepliedBy = st == "Open" || staff.Count == 0 ? null : repId,
                        };
                        t.MarkAsCreated(Marker);
                        tickets.Add(t);
                    }
                    await db.SupportTickets.AddRangeAsync(tickets, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} support tickets.", tickets.Count);
                }
                if (!await db.Notifications.AnyAsync(n => !n.IsDeleted, ct) && activeMembers.Count > 0)
                {
                    var notes = activeMembers.Take(12).Select(u =>
                    {
                        var n = new Notification
                        {
                            Id = Guid.NewGuid(), UserId = u.Id,
                            Type = faker.PickRandom(new[] { "QuoteReady", "RFQUpdate", "OrderShipped" }),
                            Message = faker.Lorem.Sentence(6, 3),
                            IsRead = faker.Random.Bool(0.5f),
                        };
                        n.MarkAsCreated(Marker);
                        return n;
                    }).ToList();
                    await db.Notifications.AddRangeAsync(notes, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} notifications.", notes.Count);
                }
                logger.LogInformation("Bogus seeded help content, shows, inquiries, tickets, notifications.");

                // ── 7. Remaining tables (every table gets demo rows) ─────────
                // Certifications (CertificateNumber is unique → insert missing only).
                var demoCerts = new[]
                {
                    ("ISO-13485-2024", "ISO 13485:2016 Quality Management", "BSI Group", 730),
                    ("CE-MDR-2024", "CE Mark — EU MDR 2017/745", "TÜV SÜD", 1095),
                    ("FDA-510K-2023", "FDA 510(k) Clearance", "U.S. Food & Drug Administration", 1825),
                    ("ISO-9001-2024", "ISO 9001:2015 Quality Management", "TÜV SÜD", 1095),
                    ("MDSAP-2024", "MDSAP Quality System Certificate", "BSI Group", 1095),
                    ("SFDA-2025", "Saudi FDA Medical Device Authorization", "Saudi Food & Drug Authority", 1095),
                };
                var existingCertNos = new HashSet<string>(
                    await db.Certifications.Where(c => !c.IsDeleted).Select(c => c.CertificateNumber).ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);
                var certsToAdd = new List<Certification>();
                foreach (var (no, title, issuer, validDays) in demoCerts)
                {
                    if (existingCertNos.Contains(no)) continue;
                    certsToAdd.Add(Certification.Create(
                        no, title, "Welco", issuer,
                        DateTime.UtcNow.AddDays(-180), DateTime.UtcNow.AddDays(validDays - 180),
                        $"Demo certification record: {title}.", $"demo/certs/{Slugify(no)}.jpg",
                        staff.Count > 0 ? staff[0].Id : null, Marker));
                }
                if (certsToAdd.Count > 0)
                {
                    await db.Certifications.AddRangeAsync(certsToAdd, ct);
                    await db.SaveChangesAsync(ct);
                }
                logger.LogInformation("Bogus seeded {Count} certifications.", certsToAdd.Count);

                // ExchangeRates (unique per base/target/date → insert missing only)
                // + ExchangeRateSyncLogs.
                var ratesAdded = 0;
                if (usd != null)
                {
                    var targetCodes = new[] { "AED", "EUR", "SAR", "EGP", "GBP", "PKR", "JPY", "CAD", "TRY", "QAR" };
                    var targets = await db.Currencies.Where(c => !c.IsDeleted && targetCodes.Contains(c.Code)).ToListAsync(ct);
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var existingPairs = new HashSet<Guid>(
                        await db.ExchangeRates
                            .Where(r => !r.IsDeleted && r.BaseCurrencyId == usd.Id && r.RateDate == today)
                            .Select(r => r.TargetCurrencyId).ToListAsync(ct));
                    var rates = new List<ExchangeRate>();
                    foreach (var t in targets)
                    {
                        if (t.Id == usd.Id || existingPairs.Contains(t.Id)) continue;
                        var r = new ExchangeRate
                        {
                            Id = Guid.NewGuid(), BaseCurrencyId = usd.Id, TargetCurrencyId = t.Id,
                            Rate = Math.Round(faker.Random.Decimal(0.05m, 400m), 6),
                            RateDate = today, Source = "FawazahmedCDN", FetchedAt = DateTime.UtcNow,
                        };
                        r.MarkAsCreated(Marker);
                        rates.Add(r);
                    }
                    if (rates.Count > 0)
                    {
                        await db.ExchangeRates.AddRangeAsync(rates, ct);
                        await db.SaveChangesAsync(ct);
                    }
                    ratesAdded = rates.Count;
                }
                if (!await db.ExchangeRateSyncLogs.AnyAsync(s => !s.IsDeleted, ct))
                {
                    var ok = new ExchangeRateSyncLog
                    {
                        Id = Guid.NewGuid(), StartedAt = DateTime.UtcNow.AddHours(-2),
                        CompletedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(3),
                        Status = ExchangeRateSyncStatus.Success, BaseCurrency = "USD",
                        RatesCount = ratesAdded, Source = "FawazahmedCDN",
                    };
                    ok.MarkAsCreated(Marker);
                    var failed = new ExchangeRateSyncLog
                    {
                        Id = Guid.NewGuid(), StartedAt = DateTime.UtcNow.AddDays(-1),
                        CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(5),
                        Status = ExchangeRateSyncStatus.Failed, BaseCurrency = "USD",
                        RatesCount = 0, Source = "FawazahmedCDN",
                        ErrorMessage = "Upstream provider timeout (demo entry).",
                    };
                    failed.MarkAsCreated(Marker);
                    await db.ExchangeRateSyncLogs.AddRangeAsync(new[] { ok, failed }, ct);
                    await db.SaveChangesAsync(ct);
                }
                logger.LogInformation("Bogus seeded {Count} exchange rates (+sync logs).", ratesAdded);

                // Carts + CartItems (user carts + guest session carts).
                if (!await db.Carts.AnyAsync(c => !c.IsDeleted && c.CreatedBy == Marker, ct) && products.Count > 0)
                {
                    var cartOwners = activeMembers.Take(6).ToList();
                    if (cartOwners.Count == 0)
                        cartOwners = await db.ApplicationUsers.Where(u => !u.IsDeleted).Take(6).ToListAsync(ct);
                    var carts = new List<Cart>();
                    foreach (var owner in cartOwners)
                    {
                        var cart = new Cart { Id = Guid.NewGuid(), UserId = owner.Id, CurrencyId = usd?.Id };
                        cart.MarkAsCreated(Marker);
                        foreach (var p in PickSome(faker, products, 1, 3))
                        {
                            var ci = new CartItem
                            {
                                Id = Guid.NewGuid(), CartId = cart.Id, ProductId = p.Id,
                                Quantity = faker.Random.Int(1, 5), UnitPriceSnapshot = p.Price,
                            };
                            ci.MarkAsCreated(Marker);
                            cart.Items.Add(ci);
                        }
                        carts.Add(cart);
                    }
                    for (var g = 0; g < 2; g++)
                    {
                        var guest = new Cart { Id = Guid.NewGuid(), SessionId = $"demo-session-{Guid.NewGuid():N}", CurrencyId = usd?.Id };
                        guest.MarkAsCreated(Marker);
                        foreach (var p in PickSome(faker, products, 1, 2))
                        {
                            var ci = new CartItem
                            {
                                Id = Guid.NewGuid(), CartId = guest.Id, ProductId = p.Id,
                                Quantity = faker.Random.Int(1, 4), UnitPriceSnapshot = p.Price,
                            };
                            ci.MarkAsCreated(Marker);
                            guest.Items.Add(ci);
                        }
                        carts.Add(guest);
                    }
                    if (carts.Count > 0)
                    {
                        await db.Carts.AddRangeAsync(carts, ct);
                        await db.SaveChangesAsync(ct);
                    }
                    logger.LogInformation("Bogus seeded {Count} carts (+items).", carts.Count);
                }

                // UserProductInteractions (unique per user/product/type → skip taken).
                var interactionUsers = activeMembers.Take(10).ToList();
                if (interactionUsers.Count == 0)
                    interactionUsers = await db.ApplicationUsers.Where(u => !u.IsDeleted).Take(10).ToListAsync(ct);
                if (interactionUsers.Count > 0 && products.Count > 0
                    && !await db.UserProductInteractions.AnyAsync(i => !i.IsDeleted && i.CreatedBy == Marker, ct))
                {
                    var interactionTypes = new[] { "Wishlist", "RecentlyViewed", "Compare" };
                    var existingTriples = (await db.UserProductInteractions.Where(i => !i.IsDeleted)
                            .Select(i => new { i.UserId, i.ProductId, i.Type }).ToListAsync(ct))
                        .Select(x => $"{x.UserId}|{x.ProductId}|{x.Type}").ToHashSet();
                    var interactions = new List<UserProductInteraction>();
                    var attempts = 0;
                    while (interactions.Count < 30 && attempts++ < 300)
                    {
                        var u = faker.PickRandom(interactionUsers);
                        var p = faker.PickRandom(products);
                        var t = faker.PickRandom(interactionTypes);
                        if (!existingTriples.Add($"{u.Id}|{p.Id}|{t}")) continue;
                        interactions.Add(new UserProductInteraction
                        {
                            Id = Guid.NewGuid(), UserId = u.Id, ProductId = p.Id, Type = t,
                            Timestamp = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 30)),
                        });
                    }
                    foreach (var i in interactions) i.MarkAsCreated(Marker);
                    if (interactions.Count > 0)
                    {
                        await db.UserProductInteractions.AddRangeAsync(interactions, ct);
                        await db.SaveChangesAsync(ct);
                    }
                    logger.LogInformation("Bogus seeded {Count} product interactions.", interactions.Count);
                }

                // DistributorApplications.
                if (!await db.DistributorApplications.AnyAsync(d => !d.IsDeleted, ct))
                {
                    var bands = new[] { "Under $100K", "$100K - $500K", "$500K - $1M", "$1M - $5M", "Over $5M" };
                    var appStatuses = new[]
                    {
                        DistributorApplicationStatus.Pending, DistributorApplicationStatus.Approved,
                        DistributorApplicationStatus.Pending, DistributorApplicationStatus.Rejected,
                        DistributorApplicationStatus.Approved,
                    };
                    var apps = new List<DistributorApplication>();
                    for (var i = 0; i < 5; i++)
                    {
                        var country = faker.PickRandom(countries);
                        var name = faker.Company.CompanyName();
                        var app = new DistributorApplication
                        {
                            Id = Guid.NewGuid(), CompanyName = name, CountryId = country.Id,
                            SalesVolumeBand = bands[i % bands.Length],
                            CategoryInterest = faker.PickRandom(Specialties).En,
                            Website = $"https://www.{Slugify(name)}.com",
                            ContactPerson = faker.Name.FullName(), ContactEmail = faker.Internet.Email(),
                            Phone = faker.Phone.PhoneNumber("+9715########"), Status = appStatuses[i],
                        };
                        app.MarkAsCreated(Marker);
                        apps.Add(app);
                    }
                    await db.DistributorApplications.AddRangeAsync(apps, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} distributor applications.", apps.Count);
                }

                // Documents (metadata + demo file URLs only, no blobs).
                if (!await db.Documents.AnyAsync(d => !d.IsDeleted, ct))
                {
                    var docTypes = new[] { "Catalog", "Brochure", "IFU", "Certificate" };
                    var docs = new List<Document>();
                    for (var i = 0; i < 8; i++)
                    {
                        var dt = docTypes[i % docTypes.Length];
                        var linked = products.Count > 0 && i % 2 == 0 ? faker.PickRandom(products) : null;
                        var doc = new Document
                        {
                            Id = Guid.NewGuid(),
                            Title = linked != null ? $"{linked.NameEn} — {dt}" : $"Welco {dt} {2024 + (i % 2)}",
                            DocType = dt, FileUrl = $"demo/docs/{Slugify(dt)}-{i + 1:00}.pdf",
                            FileSizeKB = faker.Random.Int(200, 15000), ProductId = linked?.Id,
                            PublishedDate = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 180)),
                        };
                        doc.MarkAsCreated(Marker);
                        docs.Add(doc);
                    }
                    await db.Documents.AddRangeAsync(docs, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} documents.", docs.Count);
                }

                // BlogPosts.
                if (!await db.BlogPosts.AnyAsync(b => !b.IsDeleted, ct))
                {
                    var titles = new[]
                    {
                        "How to choose the right surgical scissors",
                        "Autoclave sterilization at 134°C: best practices",
                        "German stainless steel vs titanium instruments",
                        "Setting up a distribution partnership",
                        "Understanding Incoterms for medical imports",
                        "Caring for precision instruments",
                    };
                    var posts = titles.Select((title, i) =>
                    {
                        var b = new BlogPost
                        {
                            Id = Guid.NewGuid(), Title = title, Body = faker.Lorem.Paragraphs(3, 4),
                            PublishedDate = DateTime.UtcNow.AddDays(-(i * 12 + 3)),
                        };
                        b.MarkAsCreated(Marker);
                        return b;
                    }).ToList();
                    await db.BlogPosts.AddRangeAsync(posts, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} blog posts.", posts.Count);
                }

                // SupportContacts (singleton row).
                if (!await db.SupportContacts.AnyAsync(s => !s.IsDeleted, ct))
                {
                    var contact = new SupportContact
                    {
                        Id = Guid.NewGuid(), SupportEmail = "support@welco.health",
                        PhoneNumber = "+971500000001", WhatsAppNumber = "+971500000001",
                        WorkingHours = "Mon - Fri: 8:00 AM - 6:00 PM (GST)",
                    };
                    contact.MarkAsCreated(Marker);
                    await db.SupportContacts.AddAsync(contact, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded support contact.");
                }

                // Extra landing pages (about-us comes from LandingPageSeeder).
                if (!await db.LandingPages.AnyAsync(l => !l.IsDeleted && l.CreatedBy == Marker, ct))
                {
                    var existingSlugs = new HashSet<string>(
                        await db.LandingPages.Where(l => !l.IsDeleted).Select(l => l.Slug).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var extras = new[]
                    {
                        ("Specialty", "dental-instruments", "Dental instruments, manufacturer-direct",
                            "Forceps, elevators and mirrors crafted from German stainless steel."),
                        ("Procedure", "sterilization-guide", "Sterilization guide for reusable instruments",
                            "Autoclave at 134°C, DIN-grade materials, validated reprocessing steps."),
                    };
                    var pagesToAdd = new List<LandingPage>();
                    foreach (var (type, slug, hero, body) in extras)
                    {
                        if (existingSlugs.Contains(slug)) continue;
                        var page = new LandingPage
                        {
                            Id = Guid.NewGuid(), Type = type, Slug = slug,
                            HeroTitle = hero, HeroBody = body, ContentBlock = body,
                        };
                        page.MarkAsCreated(Marker);
                        pagesToAdd.Add(page);
                    }
                    if (pagesToAdd.Count > 0)
                    {
                        await db.LandingPages.AddRangeAsync(pagesToAdd, ct);
                        await db.SaveChangesAsync(ct);
                    }
                    logger.LogInformation("Bogus seeded {Count} landing pages.", pagesToAdd.Count);
                }

                // UserRefreshTokens (one valid token per demo org member).
                var tokenUsers = activeMembers.Take(5).ToList();
                if (tokenUsers.Count == 0)
                    tokenUsers = await db.ApplicationUsers.Where(u => !u.IsDeleted).Take(5).ToListAsync(ct);
                if (tokenUsers.Count > 0 && !await db.UserRefreshTokens.AnyAsync(t => t.CreatedBy == Marker, ct))
                {
                    var tokens = tokenUsers.Select(u =>
                    {
                        var rt = UserRefreshToken.Create(u.Id, $"demo-{Guid.NewGuid():N}{Guid.NewGuid():N}", DateTime.UtcNow.AddDays(30));
                        rt.MarkAsCreated(Marker);
                        return rt;
                    }).ToList();
                    await db.UserRefreshTokens.AddRangeAsync(tokens, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} refresh tokens.", tokens.Count);
                }

                // AuditLogs are system-generated (captured on every SaveChanges above).
                logger.LogInformation("AuditLogs auto-captured: {Count}.", await db.AuditLogs.CountAsync(ct));
                logger.LogInformation("Bogus demo seeding complete.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bogus demo seeding failed.");
                throw;
            }
        }

        private static async Task<ApplicationUser?> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            Faker faker,
            string email,
            string fullName,
            UserType userType,
            Guid? companyId,
            AppLanguage language,
            string password,
            CancellationToken ct)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;
            var user = new ApplicationUser
            {
                FullName = fullName,
                Email = email,
                UserName = email,
                PhoneNumber = faker.Phone.PhoneNumber("+9715########"),
                UserType = userType,
                CompanyId = companyId,
                Language = language,
                EmailConfirmed = true,
                IsActive = true,
            };
            user.MarkAsCreated(Marker);
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Bogus user creation failed for {Email}: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }
            var roleResult = await userManager.AddToRoleAsync(user, userType.ToString());
            if (!roleResult.Succeeded)
                logger.LogWarning("Bogus role assignment failed for {Email}.", email);
            return user;
        }
    }
}
