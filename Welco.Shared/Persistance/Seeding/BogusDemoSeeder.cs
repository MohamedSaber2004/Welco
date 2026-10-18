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

        public record SurgicalItemSeed(
            string NameEn,
            string NameAr,
            string Specialty,
            string Material,
            decimal LengthCm,
            decimal Price,
            string DescEn,
            string DescAr
        );

        private static readonly SurgicalItemSeed[] RealSurgicalCatalog = ComprehensiveSurgicalCatalog.Catalog94;

        private static readonly (string Subject, string Message, string Reply)[] RealisticMedicalTickets =
        {
            (
                "CE MDR Class IIa Certification Documents Request",
                "Hello Welco Support, could you please provide the updated CE MDR Class IIa certificate and Declaration of Conformity for Hegar Needle Holders (batch #WH-2024)? Our hospital procurement compliance audit is scheduled for next week.",
                "Dear Customer, we have attached the verified CE MDR Class IIa Certificate and manufacturer Declaration of Conformity for the Hegar Needle Holder series. All instruments comply with EN ISO 13485:2016."
            ),
            (
                "Bulk Order Lead Time and Custom Laser Marking",
                "We are planning an annual procurement of 150 units of DeBakey Vascular Forceps. Can we request custom hospital department laser marking, and what is the estimated freight lead time to Riyadh?",
                "Thank you for reaching out. Department-specific laser etching is supported for orders above 50 units at no additional cost. Delivery lead time to Riyadh is 5-7 business days via temperature-controlled air freight."
            ),
            (
                "Autoclave Reprocessing and Sterilization Protocol Query",
                "Could your biomedical engineering team confirm whether the tungsten carbide inserts in Metzenbaum scissors can withstand high-vacuum steam sterilization cycles at 134°C for 18 minutes?",
                "Yes, our tungsten carbide series is rated for steam sterilization at 134°C (273°F) for up to 20 minutes under DIN 58953 / ISO 17665 standards. Please ensure pH-neutral enzymatic detergent is used during pre-cleaning."
            ),
            (
                "Inquiry on Micro-Vascular Castroviejo Jaw Replacement",
                "We have two Castroviejo micro needle holders with worn latching teeth. Does Welco offer precision recalibration and jaw insert replacement services under warranty?",
                "Our regional service depot provides precision refurbishment and spring tension calibration. Please generate an RMA shipping slip from your provider portal and our courier will collect the instruments."
            ),
            (
                "VAT TRN Update and Consolidated Monthly Invoicing",
                "We recently updated our tax registration number. We need all pending orders and historical invoices for Q3 to reflect our updated VAT TRN for local tax authority submission.",
                "Your organization VAT TRN has been successfully updated in our financial ledger. Revised consolidated tax invoices for Q3 have been re-issued and sent to your billing email."
            ),
            (
                "Expedited Shipping for Emergency Trauma Retractor Kit",
                "We have an urgent orthopedic requirement for Weitlaner and Richardson retractor sets. Is same-day dispatch available to our medical complex in Dammam?",
                "Emergency dispatch has been arranged with priority logistics. Tracking reference has been generated and courier pickup is scheduled for 14:00 today."
            ),
            (
                "Biocompatibility Test Reports for Titanium Osteotomes",
                "Our regulatory committee requires cytotoxicity and biocompatibility reports (ISO 10993) for the titanium surgical osteotomes prior to approving hospital formulary inclusion.",
                "The complete ISO 10993 biological evaluation summary and material test certificates from accredited testing laboratories have been dispatched to your committee liaison."
            ),
            (
                "Request for Quote: Annual Surgical Scissors Maintenance",
                "Please provide a commercial quote for annual preventative maintenance and micro-hone sharpening of 80 surgical scissors across our outpatient surgical suites.",
                "Our commercial team has prepared a tailored maintenance agreement proposal. The detailed quotation with tier discounts has been uploaded to your account dashboard."
            )
        };

        private static readonly string[] Materials =
            { "German Stainless Steel (AISI 420)", "Titanium Alloy", "Tungsten Carbide / Steel", "AISI 410 Surgical Steel" };

        private static readonly string[] ProcedureTags =
        {
            "General Surgery", "Cardiovascular", "Orthopedic Trauma", "Spine Fusion",
            "Plastic & Reconstructive", "Maxillofacial", "Dental Implantology", "ENT & Otology",
        };

        private static readonly (string Name, string ImageName, CompanyType Type)[] CompanySeedList =
        {
            ("Apex Surgical Supplies", "apex-surgical.svg", CompanyType.Distributor),
            ("MedCore Distributors", "medcore.svg", CompanyType.Distributor),
            ("Gulf Medical Trading", "gulf-medical.svg", CompanyType.Distributor),
            ("CityCare Hospitals Group", "citycare.svg", CompanyType.Hospital),
            ("Nova Health Clinic", "nova-health.svg", CompanyType.Clinic),
            ("PrimeCare Medical", "primecare.svg", CompanyType.Clinic),
            ("Sahara Med Import", "sahara-med.svg", CompanyType.Supplier),
            ("Delta Surgical Co.", "delta-surgical.svg", CompanyType.Supplier),
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

var roleManager = services.GetService<RoleManager<IdentityRole<Guid>>>();
                if (roleManager != null)
                    await RoleSeeder.SeedRolesAsync(roleManager, logger);

                Randomizer.Seed = new Random(FakerSeed);
                var faker = new Faker("en");
                var year = DateTime.UtcNow.Year;

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
                    logger.LogInformation("Products already present, updating existing data to realistic surgical catalog...");
                    products = await db.Products.Where(p => !p.IsDeleted).OrderBy(p => p.CreatedAt).Take(200).ToListAsync(ct);
                    var updated = false;
                    for (var i = 0; i < products.Count; i++)
                    {
                        var p = products[i];
                        var real = RealSurgicalCatalog[i % RealSurgicalCatalog.Length];
                        var expectedName = i < RealSurgicalCatalog.Length ? real.NameEn : $"{real.NameEn} - Mod {i + 1}";
                        var expectedNameAr = i < RealSurgicalCatalog.Length ? real.NameAr : $"{real.NameAr} - إصدار {i + 1}";
                        var expectedSku = $"WL-{100001 + i}";
                        var slug = $"{Slugify(expectedName)}-{i + 1}";

                        if (p.NameEn != expectedName) { p.NameEn = expectedName; updated = true; }
                        if (p.NameAr != expectedNameAr) { p.NameAr = expectedNameAr; updated = true; }
                        if (p.Sku != expectedSku) { p.Sku = expectedSku; updated = true; }
                        if (string.IsNullOrWhiteSpace(p.ImageName) || p.ImageName == "null") { p.ImageName = $"demo/products/{slug}.jpg"; updated = true; }
                        if (p.CurrencyId == null && usd != null) { p.CurrencyId = usd.Id; updated = true; }
                        if (p.Material != real.Material) { p.Material = real.Material; updated = true; }
                        if (real.LengthCm > 0 && p.LengthCm != real.LengthCm) { p.LengthCm = real.LengthCm; updated = true; }
                        if (string.IsNullOrWhiteSpace(p.Description) || p.Description.StartsWith("Lorem")) { p.Description = real.DescEn; updated = true; }
                        if (string.IsNullOrWhiteSpace(p.Specifications)) { p.Specifications = $"Autoclave 134°C; DIN 1.4021; {p.Material}; CE Class IIa; Length {p.LengthCm} cm"; updated = true; }
                    }
                    if (updated)
                    {
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Bogus updated existing products with realistic surgical data.");
                    }
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
                            var real = RealSurgicalCatalog[seq % RealSurgicalCatalog.Length];
                            seq++;
                            var nameEn = seq <= RealSurgicalCatalog.Length
                                ? real.NameEn
                                : $"{real.NameEn} - Mod {seq}";
                            var nameAr = seq <= RealSurgicalCatalog.Length
                                ? real.NameAr
                                : $"{real.NameAr} - إصدار {seq}";
                            var sku = $"WL-{100000 + seq}";
                            var slug = $"{Slugify(nameEn)}-{seq}";
                            var price = real.Price;
                            var length = real.LengthCm > 0 ? (decimal?)real.LengthCm : null;
                            var material = real.Material;
                            var desc = real.DescEn;
                            var product = Product.Create(
                                nameEn,
                                nameAr,
                                sku, slug,
                                desc,
                                price,
                                faker.Random.Int(25, 350),
                                $"Autoclave 134°C; DIN 1.4021; {material}; CE Class IIa{(length.HasValue ? $"; {length} cm" : string.Empty)}",
                                $"demo/products/{slug}.jpg",
                                material, length,
                                usd?.Id,
                                leaf.Id, null, Marker);
                            products.Add(product);
                            specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Material", AttrValue = material });
                            specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Sterilization", AttrValue = "Autoclave 134°C / Chemical Vapor" });
                            specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Compliance", AttrValue = "ISO 13485; CE Mark MDR Class IIa" });
                            if (length.HasValue) specs.Add(new ProductSpecification { Id = Guid.NewGuid(), ProductId = product.Id, AttrName = "Length", AttrValue = $"{length} cm" });
                            foreach (var s in specs.TakeLast(length.HasValue ? 4 : 3)) s.MarkAsCreated(Marker);
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
                        var seedMatch = CompanySeedList.FirstOrDefault(s => s.Name.Equals(comp.Name, StringComparison.OrdinalIgnoreCase));
                        if (seedMatch != default && comp.Type != seedMatch.Type)
                        {
                            comp.Type = seedMatch.Type;
                            updatedExisting = true;
                        }
                        if (string.IsNullOrWhiteSpace(comp.ImageName))
                        {
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

                // Mediator model linkage: distribute catalog items that have no
                // owner across approved provider companies (round-robin), so
                // provider storefronts, category providers, and the shared-SKU
                // (multi-supplier) scenarios are testable from seed data.
                var providerPool = companies
                    .Where(c => c.Status == CompanyStatus.Approved && c.IsProvider)
                    .ToList();
                if (providerPool.Count == 0)
                    providerPool = companies.Where(c => c.Status == CompanyStatus.Approved).ToList();
                var unlinkedProducts = await db.Products.Where(p => !p.IsDeleted && p.CompanyId == null).ToListAsync(ct);
                for (var pi = 0; pi < unlinkedProducts.Count; pi++)
                {
                    if (providerPool.Count == 0) break;
                    unlinkedProducts[pi].CompanyId = providerPool[pi % providerPool.Count].Id;
                }
                if (unlinkedProducts.Count > 0 && providerPool.Count > 0)
                {
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus linked {Count} products to {Providers} provider companies.", unlinkedProducts.Count, providerPool.Count);
                }

                // Shared-SKU demo: the same instrument supplied by two providers
                // at different prices (drives the product "Offered by" scenario).
                if (providerPool.Count >= 2 && leaves.Count > 0)
                {
                    const string demoSkuA = "WL-100093";
                    const string demoSkuB = "WL-100094";
                    var sharedExists = await db.Products.AnyAsync(p => !p.IsDeleted && (p.Sku == demoSkuA || p.Sku == demoSkuB), ct);
                    if (!sharedExists)
                    {
                        var demoCat = leaves[0];
                        var compA = providerPool[0];
                        var compB = providerPool[1];
                        db.Products.Add(Product.Create(
                            "Metzenbaum Dissecting Scissors Curv 18cm (Provider A)", "مقص تشريح ميتزنباوم 18 سم (المورد أ)",
                            demoSkuA, "metzenbaum-dissecting-scissors-provider-a",
                            "Demo listing from provider A with CE Class IIa certification.", 310.80m, 24,
                            "Autoclave 134°C; DIN 1.4021; Tungsten Carbide / AISI 420; CE Class IIa; Length 18 cm",
                            "demo/products/metzenbaum-dissecting-scissors-provider-a.jpg",
                            "Tungsten Carbide / AISI 420", 18m, usd?.Id,
                            demoCat.Id, compA.Id, Marker));
                        db.Products.Add(Product.Create(
                            "Metzenbaum Dissecting Scissors Curv 18cm (Provider B)", "مقص تشريح ميتزنباوم 18 سم (المورد ب)",
                            demoSkuB, "metzenbaum-dissecting-scissors-provider-b",
                            "Demo listing from provider B with fast regional fulfillment.", 289.50m, 12,
                            "Autoclave 134°C; DIN 1.4021; Tungsten Carbide / AISI 420; CE Class IIa; Length 18 cm",
                            "demo/products/metzenbaum-dissecting-scissors-provider-b.jpg",
                            "Tungsten Carbide / AISI 420", 18m, usd?.Id,
                            demoCat.Id, compB.Id, Marker));
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Bogus seeded demo listings for {A} and {B}.", compA.Name, compB.Name);
                    }
                }

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



                logger.LogInformation("Bogus seeded {Staff} staff, {Org} org users.",
                    staff.Count, orgUsers.Count);

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

                var repId = staff.Count > 0 ? staff[0].Id : Guid.NewGuid();
                var chainNo = 0;
                var chainPrefix = $"WO-{year}-";
                var topProducts = products.Take(12).ToList();
                var tieredQuantities = new[] { 320, 275, 240, 195, 160, 135, 110, 85, 70, 55, 45, 35 };

                if (await db.Orders.AnyAsync(o => !o.IsDeleted && (o.CreatedBy == Marker || o.OrderNumber.StartsWith(chainPrefix)), ct))
                {
                    logger.LogInformation("Orders already present, checking if top surgical products have order items...");
                    var topIds = topProducts.Select(p => p.Id).ToList();
                    var existingTopItemsCount = await db.OrderItems.CountAsync(oi => !oi.IsDeleted && topIds.Contains(oi.ProductId), ct);
                    if (existingTopItemsCount == 0 && topProducts.Count > 0)
                    {
                        var primaryUser = orgUsers.Count > 0 ? orgUsers[0].User : staff.FirstOrDefault();
                        var primaryCompany = orgUsers.Count > 0 ? orgUsers[0].Company : approved.FirstOrDefault();
                        if (primaryUser != null && primaryCompany != null)
                        {
                            var bulkOrder = new Order
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = $"WO-{year}-BULK-001",
                                Status = OrderStatus.Completed,
                                UserId = primaryUser.Id,
                                CompanyId = primaryCompany.Id,
                                CurrencyId = usd?.Id,
                                SnapshotBaseCurrency = "USD",
                                SnapshotCurrencyCode = usd?.Code ?? "USD",
                                SnapshotRate = 1,
                                SnapshotRateDate = DateOnly.FromDateTime(DateTime.UtcNow),
                                SnapshotSource = Marker,
                            };
                            bulkOrder.MarkAsCreated(Marker);

                            decimal bulkTotal = 0;
                            for (var t = 0; t < topProducts.Count; t++)
                            {
                                var p = topProducts[t];
                                var qty = t < tieredQuantities.Length ? tieredQuantities[t] : 30;
                                var oi = new OrderItem
                                {
                                    Id = Guid.NewGuid(),
                                    OrderId = bulkOrder.Id,
                                    ProductId = p.Id,
                                    Quantity = qty,
                                    UnitPrice = p.Price,
                                };
                                oi.MarkAsCreated(Marker);
                                bulkOrder.Items.Add(oi);
                                bulkTotal += qty * p.Price;
                            }
                            bulkOrder.TotalAmount = bulkTotal;

                            var bulkInvoice = new Invoice
                            {
                                Id = Guid.NewGuid(),
                                OrderId = bulkOrder.Id,
                                InvoiceNumber = $"INV-{year}-BULK-001",
                                Amount = bulkTotal,
                                Status = InvoiceStatus.Paid,
                            };
                            bulkInvoice.MarkAsCreated(Marker);
                            bulkOrder.Invoices.Add(bulkInvoice);

                            await db.Orders.AddAsync(bulkOrder, ct);
                            await db.SaveChangesAsync(ct);
                            logger.LogInformation("Bogus seeded hospital bulk procurement order with {Count} realistic surgical items.", topProducts.Count);
                        }
                    }
                }
                else
                {
                    var topProductIdx = 0;
                    foreach (var (user, company) in orgUsers.Take(products.Count > 0 ? 12 : 0))
                    {
                        chainNo++;
                        var items = new List<Product>();

                        // Inject top surgical products with realistic tiered sales volumes in the first few hospital orders
                        if (topProductIdx < topProducts.Count)
                        {
                            var batchSize = Math.Min(3, topProducts.Count - topProductIdx);
                            items.AddRange(topProducts.Skip(topProductIdx).Take(batchSize));
                            topProductIdx += batchSize;
                        }

                        // Pick additional products to diversify the order
                        var extraItems = PickSome(faker, products.Except(items).ToList(), 2, 4);
                        items.AddRange(extraItems);

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
                            var topIdx = topProducts.IndexOf(p);
                            var qty = topIdx >= 0 && topIdx < tieredQuantities.Length
                                ? tieredQuantities[topIdx]
                                : faker.Random.Int(10, 45);

                            var ri = new RFQItem
                            {
                                Id = Guid.NewGuid(), RFQId = rfq.Id, ProductId = p.Id,
                                Quantity = qty, UnitPrice = p.Price,
                                Notes = faker.Random.Bool(0.3f) ? "Urgent hospital clinical demand" : null,
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
                            Status = faker.PickRandom(new[]
                        {
                            OrderStatus.Confirmed,
                            OrderStatus.Processing,
                            OrderStatus.Ready,
                            OrderStatus.Shipped,
                            OrderStatus.Delivered,
                            OrderStatus.Completed
                        }),
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
                    logger.LogInformation("Bogus seeded {Count} RFQ→Quote→Order chains with realistic hospital procurement volumes.", chainNo);
                }

var testCats = await db.HelpCategories.Where(c => !c.IsDeleted && (c.Name.ToLower() == "test" || c.Name.ToLower().Contains("Ø§Ø®ØªØ¨Ø§Ø±"))).ToListAsync(ct);
                if (testCats.Count > 0)
                {
                    foreach (var tc in testCats) tc.MarkAsDeleted("SeedCleanup");
                    await db.SaveChangesAsync(ct);
                }

                var catEntities = new List<HelpCategory>();
                if (!await db.HelpCategories.AnyAsync(c => !c.IsDeleted && c.Name.ToLower() != "test", ct))
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
                    for (var ti = 0; ti < ticketUsers.Count; ti++)
                    {
                        var tu = ticketUsers[ti];
                        var tmpl = RealisticMedicalTickets[ti % RealisticMedicalTickets.Length];
                        var st = statuses[ti % statuses.Length];
                        var createdDaysAgo = faker.Random.Int(3, 14);
                        var createdAt = DateTime.UtcNow.AddDays(-createdDaysAgo);
                        DateTime? repliedAt = null;
                        if (st != "Open")
                        {
                            var replyTime = createdAt.AddHours(faker.Random.Int(2, 24));
                            if (replyTime > DateTime.UtcNow) replyTime = DateTime.UtcNow;
                            repliedAt = replyTime;
                        }
                        var t = new SupportTicket
                        {
                            Id = Guid.NewGuid(),
                            UserId = tu.Id,
                            Subject = tmpl.Subject,
                            Message = tmpl.Message,
                            Status = st,
                            Reply = st == "Open" ? null : tmpl.Reply,
                            RepliedAt = repliedAt,
                            RepliedBy = st == "Open" || staff.Count == 0 ? null : repId,
                        };
                        t.MarkAsCreated(Marker, createdAt);
                        tickets.Add(t);
                    }
                    await db.SupportTickets.AddRangeAsync(tickets, ct);
                    await db.SaveChangesAsync(ct);
                    logger.LogInformation("Bogus seeded {Count} support tickets.", tickets.Count);
                }
                else
                {
                    var latinTickets = await db.SupportTickets
                        .Where(t => !t.IsDeleted && (t.Subject.Contains("Lorem") || t.Message.Contains("Lorem") || t.Message.Contains("ipsum") || t.Message.Contains("dolor")))
                        .ToListAsync(ct);
                    if (latinTickets.Count > 0)
                    {
                        for (var i = 0; i < latinTickets.Count; i++)
                        {
                            var tmpl = RealisticMedicalTickets[i % RealisticMedicalTickets.Length];
                            latinTickets[i].Subject = tmpl.Subject;
                            latinTickets[i].Message = tmpl.Message;
                            if (latinTickets[i].Status != "Open")
                            {
                                latinTickets[i].Reply = tmpl.Reply;
                            }
                        }
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Bogus sanitized {Count} support tickets with realistic medical B2B interactions.", latinTickets.Count);
                    }

                    if (await db.SupportTickets.AnyAsync(t => !t.IsDeleted && t.RepliedAt != null && t.RepliedAt < t.CreatedAt, ct))
                    {
                        var invalidTickets = await db.SupportTickets
                            .Where(t => !t.IsDeleted && t.RepliedAt != null && t.RepliedAt < t.CreatedAt)
                            .ToListAsync(ct);
                        foreach (var it in invalidTickets)
                        {
                            var healed = it.CreatedAt.AddHours(2);
                            if (healed > DateTime.UtcNow) healed = DateTime.UtcNow;
                            it.RepliedAt = healed;
                        }
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Bogus healed {Count} support tickets with replied date before created date.", invalidTickets.Count);
                    }
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
            if (existing != null)
            {
                if (companyId.HasValue && existing.CompanyId == null)
                {
                    existing.CompanyId = companyId;
                    existing.MarkAsUpdated(Marker);
                    await userManager.UpdateAsync(existing);
                }
                return existing;
            }
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
