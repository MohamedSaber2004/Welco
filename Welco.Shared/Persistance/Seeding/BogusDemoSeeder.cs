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

        private static readonly SurgicalItemSeed[] RealSurgicalCatalog =
        {
            new(
                "Mayo Dissecting Scissors 17cm Straight",
                "مقص مايو للتشريح الجراحي 17 سم مستقيم",
                "Plastic Surgery",
                "German Stainless Steel (AISI 420)",
                17.0m,
                145.00m,
                "Precision beveled blades engineered for cutting tough connective tissue, fascia, and deep surgical sutures. Hot-drop forged German steel with satin anti-glare finish.",
                "شفرات مشطوفة فائقة الدقة لقطع الأنسجة الضامة السميكة واللفافات الجراحية. مصنوعة من الفولاذ الألماني المشكل حرارياً بتشطيب مطفأ مانع للتوهج."
            ),
            new(
                "Metzenbaum Delicate Dissecting Scissors Curved 18cm",
                "مقص ميتزنباوم الدقيق للتشريح الجراحي منحني 18 سم",
                "Plastic Surgery",
                "Tungsten Carbide / AISI 420",
                18.0m,
                185.00m,
                "Gold-handled tungsten carbide curved blades designed specifically for blunt and sharp atraumatic dissection of delicate vascular and visceral tissues.",
                "شفرات منحنية مدعمة بكربيد التنجستن ومقابض مذهبة مخصصة للتشريح الدقيق للأنسجة الرقيقة والأوعية الدموية دون إحداث تمزق رضحي."
            ),
            new(
                "Adson Micro Tissue Forceps 1x2 Teeth 12cm",
                "ملقط أنسجة أدسون الجراحي الدقيق 1×2 سن 12 سم",
                "Plastic Surgery",
                "German Stainless Steel (AISI 410)",
                12.0m,
                58.00m,
                "Fine atraumatic interdigitating 1x2 teeth providing secure grip on fragile dermis and subcuticular flaps during precision cosmetic and reconstructive closure.",
                "أسنان دقيقة 1×2 متداخلة توفر إمساكاً محكماً بحواف الجلد والأنسجة الدقيقة أثناء إجراءات الخياطة الترميمية والتجميلية."
            ),
            new(
                "DeBakey Atraumatic Vascular Forceps 20cm",
                "ملقط ديبيكي الجراحي للأوعية الدموية 20 سم",
                "Orthopedic",
                "Titanium Alloy / AISI 420",
                20.0m,
                210.00m,
                "Patented non-crushing longitudinal ribbing designed to stabilize delicate arterial walls and venous grafts without intima disruption during cardiovascular and trauma surgery.",
                "تضليع طولي غير راضخ ومصمم لتثبيت جدران الشرايين والأوردة الحساسة دون تلف بطانة الأوعية في الجراحات الوعائية."
            ),
            new(
                "Hegar Needle Holder Tungsten Carbide Jaws 16cm",
                "حامل إبر هيغار مع فكوك كربيد التنجستن 16 سم",
                "Plastic Surgery",
                "Tungsten Carbide / AISI 420",
                16.0m,
                175.00m,
                "Cross-serrated tungsten carbide jaw inserts (0.4mm pitch) engineered to eliminate needle slippage and twisting under high tension wound approximation.",
                "فكوك كربيد التنجستن بتسنين متقاطع يمنع تماماً انزلاق الإبرة أو دورانها أثناء إغلاق طبقات الجروح المشدودة."
            ),
            new(
                "Castroviejo Micro Needle Holder Straight with Lock 14cm",
                "حامل إبر كاستروفيجو الدقيق مع قفل 14 سم",
                "Plastic Surgery",
                "Titanium",
                14.0m,
                320.00m,
                "Precision calibrated spring-action handle with ultra-smooth latching mechanism engineered for micro-vascular and ophthalmic sutures (6-0 through 9-0).",
                "مقبض نوابضي فائق الدقة مع آلية قفل ناعمة مصمم خصيصاً للغرز الجراحية المجهرية والأوعية الدموية الدقيقة."
            ),
            new(
                "Crile Hemostatic Forceps Curved 14cm",
                "ملقط كرايل القاطع للنزيف منحني 14 سم",
                "Dental",
                "German Stainless Steel (AISI 410)",
                14.0m,
                62.00m,
                "Transversely serrated along entire jaw length with interlocking ratchets for rapid secure hemostasis of medium vascular pedicles and tissue bleeders.",
                "مسنن عرضياً بالكامل مع محبس قفل تدريجي لتأمين الإرقاء الفوري وإيقاف النزيف في الأوعية الدموية المتوسطة."
            ),
            new(
                "Kelly Hemostatic Clamps Straight 14cm",
                "ملقط كيلي الجراحي المستقيم 14 سم",
                "Dental",
                "German Stainless Steel (AISI 410)",
                14.0m,
                59.00m,
                "Serrated on distal half of jaws allowing selective clamping of bleeding subcutaneous vessels without damaging adjacent healthy anatomical structures.",
                "مسنن في النصف الطرفي فقط للتحكم في الأوعية النازفة بدقة متناهية دون الضغط على الأنسجة السليمة المجاورة."
            ),
            new(
                "Babcock Intestinal Tissue Grasping Forceps 16cm",
                "ملقط بابكوك الجراحي للإمساك بالأنسجة والأمعاء 16 سم",
                "Orthopedic",
                "German Stainless Steel (AISI 410)",
                16.0m,
                125.00m,
                "Atraumatic fenestrated triangular loops with smooth rounded borders for holding tubular organs, ureters, and intestinal loops without ischemic trauma.",
                "فكوك مثلثة مفرغة وناعمة الحواف مصممة لمسك الأعضاء الأنبوبية والأمعاء والحالب دون إحداث نقص تروية أو رضخ موضعي."
            ),
            new(
                "Allis Tissue Grasping Clamps 5x6 Teeth 15cm",
                "ملقط أليس الجراحي لمسك الأنسجة 5×6 أسنان 15 سم",
                "Orthopedic",
                "German Stainless Steel (AISI 410)",
                15.0m,
                88.00m,
                "Multiple interlocking fine teeth delivering uniform traction on fascial edges and fibrous tendon sheaths without slippage.",
                "أسنان متعددة متداخلة 5×6 توفر قوة شد متجانسة على اللفافات والأغماد الليفية دون انزلاق."
            ),
            new(
                "Weitlaner Self-Retaining Retractor Sharp 3x4 Teeth 13cm",
                "مبعد ويتلانر الجراحي ذاتي التثبيت 13 سم",
                "Orthopedic",
                "German Stainless Steel",
                13.0m,
                245.00m,
                "Cam-ratchet self-locking arms with curved sharp 3x4 prongs maintaining deep wound cavity visualization hands-free during orthopedics and neurosurgery.",
                "ذراعان بمحبس مسنن ومخالب حادة 3×4 تضمن بقاء المجال الجراحي مفتوحاً بثبات ودون إجهاد للأيدي المساعدة."
            ),
            new(
                "Richardson-Eastman Retractor Handheld Set of 2",
                "طقم مبعدات ريتشاردسون-إيستمان الجراحية المزدوجة",
                "Orthopedic",
                "German Stainless Steel (AISI 420)",
                26.0m,
                195.00m,
                "Ergonomically contoured broad concave lateral blades providing deep pelvic and abdominal muscular wall exposure with minimal pressure necrosis.",
                "شفرات مقعرة عريضة بتصميم مريح مخصصة لإبعاد الجدار العضلي للبطن والحوض مع تقليل ضغط النخر الموضعي."
            ),
            new(
                "Liston High-Leverage Bone Cutting Forceps Angled 19cm",
                "ملقط ليستون لقطع العظام بزاوية وقوة مضاعفة 19 سم",
                "Orthopedic",
                "German High-Carbon Steel",
                19.0m,
                365.00m,
                "Compound double-action pivot system delivering high mechanical force to shear through dense cortical bone cleanly during joint reconstruction and amputations.",
                "نظام مفصل مزدوج مضاعف القوة يتيح قص العظام القشرية الصلبة بقطع نظيف وأملس أثناء جراحات المفاصل وبتر الأطراف."
            ),
            new(
                "Stille-Luer Double-Action Bone Rongeur Curved 22cm",
                "قارضة عظام ستيل-لوير منحنية بمفصل مزدوج 22 سم",
                "Orthopedic",
                "German Stainless Steel (AISI 420)",
                22.0m,
                395.00m,
                "Heavy-duty curved scoop jaws designed for rapid, controlled excision of thick articular bone fragments, bone spikes, and periosteal margins.",
                "فكوك غائرة قوية ومنحنية مصممة للاستئصال السريع والمنضبط للنتوءات العظمية والحواف المفصلية الكثيفة."
            ),
            new(
                "Kerrison Spinal Laminectomy Punch 40° Up-Biting 3mm",
                "مثقاب كيريسون لاستئصال الصفيحة الفقرية 40 درجة 3 مم",
                "Spine",
                "German High-Tensile Steel",
                20.0m,
                520.00m,
                "Thin-footplate laminectomy punch featuring 40-degree upward cutting action with ejector groove for precise spinal canal decompression.",
                "مثقاب رفيع بزاوية صاعدة 40 درجة ومزود بمجرى تفريغ لضمان إزالة الضغط عن القناة الشوكية بدقة بالغة."
            ),
            new(
                "Killian Nasal Speculum with Set Screw Adjuster",
                "منظار كيليان للأنف مع برغي ضبط دقيق",
                "ENT",
                "German Stainless Steel",
                14.0m,
                165.00m,
                "Serrated thin parallel blades with knurled thumbscrew lock mechanism providing stable, glare-free dilation of anterior nasal cavity.",
                "شفرات متوازية رقيقة مسننة مع برغي تثبيت محزز يضمن اتساعاً ثابتاً ومستقراً لتجويف الأنف دون انعكاسات ضوئية مزعجة."
            ),
            new(
                "Frazier Neuro Suction Cannula 10 French Angled",
                "أنبوب شفط فريزر لجراحة الأعصاب قياس 10 فرنش",
                "Spine",
                "Medical Stainless Steel",
                18.0m,
                75.00m,
                "Graduated thin cannula with angled shaft and ergonomic teardrop thumb vent for fingertip regulation of surgical vacuum pressure.",
                "قنية رفيعة مدرجة بزاوية مريحة وفتحة إبهام دمعية للتحكم اللحظي الدقيق بقوة الشفط داخل الجروح العميقة."
            ),
            new(
                "Dental Extracting Forceps #150 Universal Upper",
                "ملقط خلع الأسنان رقم 150 العالمي للفك العلوي",
                "Dental",
                "AISI 420 Stainless",
                17.5m,
                110.00m,
                "Universal anatomical beaks with textured non-slip grip handles designed for atraumatic luxation of upper incisors, canines, and bicuspids.",
                "مناقير مصممة تشريحياً مع مقابض غير قابلة للانزلاق لخلع القواطع والأنياب والضواحك العلوية بدون كسر الحافة السنخية."
            ),
            new(
                "Coupland Dental Root Elevator Straight Set (3-Piece)",
                "طقم روافع كوبلاند لجذور الأسنان مستقيم 3 قطع",
                "Dental",
                "German Stainless Steel",
                15.0m,
                145.00m,
                "Gouge-shaped concave working ends with sharp lateral cutting edges designed to sever periodontal fibers and elevate retained dental roots smoothly.",
                "نهايات مقعرة وحواف جانبية حادة لقطع ألياف الرباط السني وخلخلة الجذور المتبقية بكل سلاسة."
            ),
            new(
                "Molt #9 Dual-End Periosteal Elevator 18cm",
                "رافعة السمحاق مولت رقم 9 مزدوجة الأطراف 18 سم",
                "CMF",
                "German Stainless Steel (AISI 420)",
                18.0m,
                72.00m,
                "Features a sharp pointed triangular blade on one side and a broad curved spatula on the other for rapid dissection of mucoperiosteal flaps.",
                "تحتوي على طرف مدبب حاد وطرف ملعقي مقوس لرفع الشرائح السمحاقية بسرعة وبأقل قدر من الرضخ للأنسجة."
            ),
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
                    products = await db.Products.Where(p => !p.IsDeleted).Take(200).ToListAsync(ct);
                    var updated = false;
                    for (var i = 0; i < products.Count; i++)
                    {
                        var p = products[i];
                        if (p.NameEn.Contains("Standard") || p.NameEn.Contains("Deluxe") || p.NameAr.Any(char.IsDigit) || (p.Description != null && p.Description.StartsWith("Lorem")))
                        {
                            var real = RealSurgicalCatalog[i % RealSurgicalCatalog.Length];
                            p.NameEn = real.NameEn;
                            p.NameAr = real.NameAr;
                            p.Description = real.DescEn;
                            p.Material = real.Material;
                            if (real.LengthCm > 0) p.LengthCm = real.LengthCm;
                            p.Price = real.Price;
                            p.Specifications = $"Autoclave 134°C; DIN 1.4021; {p.Material}; CE Class IIa; Length {p.LengthCm} cm";
                            updated = true;
                        }
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
                var unlinkedProducts = products.Where(p => !p.IsDeleted && p.CompanyId == null).ToList();
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
                    const string sharedSku = "WL-SHARED-001";
                    var sharedExists = await db.Products.AnyAsync(p => !p.IsDeleted && p.Sku == sharedSku, ct);
                    if (!sharedExists)
                    {
                        var demoCat = leaves[0];
                        var compA = providerPool[0];
                        var compB = providerPool[1];
                        db.Products.Add(Product.Create(
                            "Metzenbaum Dissecting Scissors", "مقص تشريح ميتزنباوم",
                            sharedSku, "metzenbaum-dissecting-scissors-a",
                            "Demo shared listing from provider A.", 310.80m, 24,
                            null, null, "German Stainless Steel", 18m, null,
                            demoCat.Id, compA.Id, Marker));
                        db.Products.Add(Product.Create(
                            "Metzenbaum Dissecting Scissors", "مقص تشريح ميتزنباوم",
                            sharedSku, "metzenbaum-dissecting-scissors-b",
                            "Demo shared listing from provider B.", 289.50m, 12,
                            null, null, "German Stainless Steel", 18m, null,
                            demoCat.Id, compB.Id, Marker));
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Bogus seeded shared-SKU demo listings for {A} and {B}.", compA.Name, compB.Name);
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
                    logger.LogInformation("Bogus seeded {Count} RFQ→Quote→Order chains with realistic hospital procurement volumes.", chainNo);
                }

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
