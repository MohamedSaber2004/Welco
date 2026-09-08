using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        private static string DemoPassword() =>
            Environment.GetEnvironmentVariable("SEED_DEMO_PASSWORD") is { Length: > 0 } pwd
                ? pwd
                : "Demo123!";

        public static async Task SeedDemoAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
        {
            try
            {
                var env = services.GetService<IHostEnvironment>();
                if (env != null && env.IsProduction())
                {
                    logger.LogWarning("BogusDemoSeeder refused: Production environment.");
                    return;
                }

                var db = services.GetRequiredService<WelcoDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                // Self-sufficient: ensure Identity roles (incl. Customer) exist
                // even when the host service never ran RoleSeeder (e.g. Product).
                var roleManager = services.GetService<RoleManager<IdentityRole<Guid>>>();
                if (roleManager != null)
                    await RoleSeeder.SeedRolesAsync(roleManager, logger);

                if (await db.Products.AnyAsync(p => !p.IsDeleted && p.CreatedBy == Marker, ct))
                {
                    logger.LogInformation("Bogus demo data already seeded, skipping.");
                    return;
                }

                Randomizer.Seed = new Random(FakerSeed);
                var faker = new Faker("en");
                var year = DateTime.UtcNow.Year;

                // ── 1. Categories (6 roots + 18 children) ────────────────────
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
                var leaves = categories.Where(c => c.ParentCategoryId.HasValue).ToList();
                logger.LogInformation("Bogus seeded {Count} categories.", categories.Count);

                // ── 2. Products + specs + media + tags (5 per leaf = 90) ─────
                var usd = await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted && c.Code == "USD", ct)
                    ?? await db.Currencies.FirstOrDefaultAsync(c => !c.IsDeleted, ct);
                var products = new List<Product>();
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

                // ── 3. Companies + addresses ─────────────────────────────────
                var countries = await db.Countries.Where(c => !c.IsDeleted).ToListAsync(ct);
                if (countries.Count == 0)
                {
                    logger.LogWarning("Bogus seeding stopped: no countries found (run WorldLocationSeeder first).");
                    return;
                }
                var types = new[] { CompanyType.Distributor, CompanyType.Distributor, CompanyType.Distributor, CompanyType.Distributor, CompanyType.Hospital, CompanyType.Hospital, CompanyType.Hospital, CompanyType.Hospital, CompanyType.Clinic, CompanyType.Clinic, CompanyType.Importer, CompanyType.Importer };
                var companies = new List<Company>();
                for (var i = 0; i < CompanyNames.Length; i++)
                {
                    var country = faker.PickRandom(countries);
                    var status = i == CompanyNames.Length - 1 ? CompanyStatus.Pending : CompanyStatus.Approved;
                    var company = Company.Create(
                        CompanyNames[i], types[i], country.Id,
                        faker.Random.Int(1, 3), status, null, Marker,
                        $"info@{Slugify(CompanyNames[i])}.example.com");
                    company.IsProvider = types[i] == CompanyType.Distributor;
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

                // ── 4. Users (via UserManager) ───────────────────────────────
                var password = DemoPassword();
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
                }
                var customers = new List<ApplicationUser>();
                for (var i = 1; i <= 14; i++)
                {
                    var u = await EnsureUserAsync(userManager, logger, faker,
                        $"demo.customer{i:00}@welco.health", faker.Name.FullName(),
                        UserType.Customer, null, i % 3 == 0 ? AppLanguage.Ar : AppLanguage.En,
                        password, ct);
                    if (u != null) customers.Add(u);
                }
                logger.LogInformation("Bogus seeded {Staff} staff, {Org} org users, {Cust} customers.",
                    staff.Count, orgUsers.Count, customers.Count);

                // default address for every customer (checkout needs one)
                var custAddresses = new List<UserAddress>();
                foreach (var c in customers)
                {
                    var country = faker.PickRandom(countries);
                    var city = await db.Cities.FirstOrDefaultAsync(x => !x.IsDeleted && x.CountryId == country.Id, ct)
                        ?? await db.Cities.FirstOrDefaultAsync(x => !x.IsDeleted, ct);
                    if (city == null) continue;
                    var zone = await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted && z.CityId == city.Id, ct)
                        ?? await db.Zones.FirstOrDefaultAsync(z => !z.IsDeleted, ct);
                    if (zone == null) continue;
                    custAddresses.Add(UserAddress.Create(c.Id, country.Id, city.Id, zone.Id,
                        $"{faker.Random.Int(1, 200)} {faker.Address.StreetName()}", null, null, null, Marker, isDefault: true));
                }
                if (custAddresses.Count > 0)
                {
                    await db.UserAddresses.AddRangeAsync(custAddresses, ct);
                    await db.SaveChangesAsync(ct);
                }

                // ── 5. RFQ → Quote → Order chains (12, on approved companies) ─
                var repId = staff.Count > 0 ? staff[0].Id : Guid.NewGuid();
                var chainNo = 0;
                foreach (var (user, company) in orgUsers.Take(12))
                {
                    chainNo++;
                    var items = faker.PickRandom(products, faker.Random.Int(2, 5)).Distinct().ToList();
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

                // ── 6. Help content, shows, inquiries, tickets, notifications ─
                var helpCats = new[] { "Ordering", "Shipping & Incoterms", "Returns & RMA", "Sterilization", "Warranty" };
                var icons = new[] { "shopping_cart", "local_shipping", "restart_alt", "cleaning_services", "verified" };
                var catEntities = new List<HelpCategory>();
                for (var i = 0; i < helpCats.Length; i++)
                {
                    var hc = new HelpCategory { Id = Guid.NewGuid(), Name = helpCats[i], Icon = icons[i] };
                    hc.MarkAsCreated(Marker);
                    catEntities.Add(hc);
                }
                await db.HelpCategories.AddRangeAsync(catEntities, ct);
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

                var ticketUsers = customers.Take(8).ToList();
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
                var notes = customers.Take(12).Select(u =>
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
                logger.LogInformation("Bogus seeded help content, shows, inquiries, tickets, notifications.");
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
