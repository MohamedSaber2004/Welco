using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;

namespace Welco.Shared.Persistance.Seeding
{
    public static class CurrencySeeder
    {
        
        private static readonly (string Code, string NameEn, string NameAr, string Symbol, string SymbolNative, int Digits)[] Currencies =
        {
            ("AED","UAE Dirham","درهم إماراتي","AED","د.إ",2),
            ("AFN","Afghan Afghani","أفغاني","AFN","؋",2),
            ("ALL","Albanian Lek","ليك ألباني","ALL","L",2),
            ("AMD","Armenian Dram","درام أرميني","AMD","֏",2),
            ("ANG","Netherlands Antillean Guilder","جلدر هولندي","ANG","ƒ",2),
            ("AOA","Angolan Kwanza","كوانزا أنغولي","AOA","Kz",2),
            ("ARS","Argentine Peso","بيزو أرجنتيني","ARS","$",2),
            ("AUD","Australian Dollar","دولار أسترالي","AUD","A$",2),
            ("AZN","Azerbaijani Manat","مانات أذربيجاني","AZN","₼",2),
            ("BAM","Bosnia-Herzegovina Convertible Mark","مارك البوسنة","BAM","KM",2),
            ("BBD","Barbadian Dollar","دولار بربادوسي","BBD","Bds$",2),
            ("BDT","Bangladeshi Taka","تاكا بنغلاديشي","BDT","৳",2),
            ("BGN","Bulgarian Lev","ليف بلغاري","BGN","лв",2),
            ("BHD","Bahraini Dinar","دينار بحريني","BHD",".د.ب",3),
            ("BIF","Burundian Franc","فرنك بوروندي","BIF","FBu",0),
            ("BMD","Bermudian Dollar","دولار برمودي","BMD","BD$",2),
            ("BND","Brunei Dollar","دولار بروناي","BND","B$",2),
            ("BOB","Bolivian Boliviano","بوليفيانو بوليفي","BOB","Bs.",2),
            ("BRL","Brazilian Real","ريال برازيلي","BRL","R$",2),
            ("BSD","Bahamian Dollar","دولار باهامي","BSD","B$",2),
            ("BTN","Bhutanese Ngultrum","نغولتروم بوتاني","BTN","Nu.",2),
            ("BWP","Botswanan Pula","بولا بوتسواني","BWP","P",2),
            ("BYN","Belarusian Ruble","روبل بيلاروسي","BYN","Br",2),
            ("BZD","Belize Dollar","دولار بليزي","BZD","BZ$",2),
            ("CAD","Canadian Dollar","دولار كندي","CAD","C$",2),
            ("CDF","Congolese Franc","فرنك كونغولي","CDF","FC",2),
            ("CHF","Swiss Franc","فرنك سويسري","CHF","CHF",2),
            ("CLP","Chilean Peso","بيزو تشيلي","CLP","$",0),
            ("CNY","Chinese Yuan","يوان صيني","CNY","¥",2),
            ("COP","Colombian Peso","بيزو كولومبي","COP","$",2),
            ("CRC","Costa Rican Colón","كولون كوستاريكي","CRC","₡",2),
            ("CUP","Cuban Peso","بيزو كوبي","CUP","₱",2),
            ("CVE","Cape Verdean Escudo","إسكودو الرأس الأخضر","CVE","Esc",2),
            ("CZK","Czech Koruna","كورون تشيكي","CZK","Kč",2),
            ("DJF","Djiboutian Franc","فرنك جيبوتي","DJF","Fdj",0),
            ("DKK","Danish Krone","كرون دنماركي","DKK","kr",2),
            ("DOP","Dominican Peso","بيزو دومينيكاني","DOP","RD$",2),
            ("DZD","Algerian Dinar","دينار جزائري","DZD","دج",2),
            ("EGP","Egyptian Pound","جنيه مصري","EGP","E£",2),
            ("ERN","Eritrean Nakfa","ناكفا إريتري","ERN","Nfk",2),
            ("ETB","Ethiopian Birr","بير إثيوبي","ETB","Br",2),
            ("EUR","Euro","يورو","EUR","€",2),
            ("FJD","Fijian Dollar","دولار فيجي","FJD","FJ$",2),
            ("FKP","Falkland Islands Pound","جنيه جزر فوكلاند","FKP","£",2),
            ("GBP","British Pound","جنيه إسترليني","GBP","£",2),
            ("GEL","Georgian Lari","لاري جورجي","GEL","₾",2),
            ("GHS","Ghanaian Cedi","سيدي غاني","GHS","GH₵",2),
            ("GIP","Gibraltar Pound","جنيه جبل طارق","GIP","£",2),
            ("GMD","Gambian Dalasi","دالاسي غامبي","GMD","D",2),
            ("GNF","Guinean Franc","فرنك غيني","GNF","FG",0),
            ("GTQ","Guatemalan Quetzal","كيتزال غواتيمالي","GTQ","Q",2),
            ("GYD","Guyanese Dollar","دولار غياني","GYD","GY$",2),
            ("HKD","Hong Kong Dollar","دولار هونغ كونغ","HKD","HK$",2),
            ("HNL","Honduran Lempira","ليمبيرا هندوراسي","HNL","L",2),
            ("HTG","Haitian Gourde","غورد هايتي","HTG","G",2),
            ("HUF","Hungarian Forint","فورنت مجري","HUF","Ft",2),
            ("IDR","Indonesian Rupiah","روبية إندونيسية","IDR","Rp",0),
            ("ILS","Israeli New Shekel","شيكل إسرائيلي","ILS","₪",2),
            ("INR","Indian Rupee","روبية هندية","INR","₹",2),
            ("IQD","Iraqi Dinar","دينار عراقي","IQD","ع.د",3),
            ("IRR","Iranian Rial","ريال إيراني","IRR","﷼",2),
            ("ISK","Icelandic Króna","كرونة آيسلندية","ISK","kr",0),
            ("JMD","Jamaican Dollar","دولار جامايكي","JMD","J$",2),
            ("JOD","Jordanian Dinar","دينار أردني","JOD","JD",3),
            ("JPY","Japanese Yen","ين ياباني","JPY","¥",0),
            ("KES","Kenyan Shilling","شلن كيني","KES","KSh",2),
            ("KGS","Kyrgyzstani Som","سوم قيرغيزستاني","KGS","лв",2),
            ("KHR","Cambodian Riel","رييل كمبودي","KHR","៛",2),
            ("KMF","Comorian Franc","فرنك قمري","KMF","CF",0),
            ("KPW","North Korean Won","وون كوري شمالي","KPW","₩",2),
            ("KRW","South Korean Won","وون كوري","KRW","₩",0),
            ("KWD","Kuwaiti Dinar","دينار كويتي","KWD","KD",3),
            ("KYD","Cayman Islands Dollar","دولار جزر كايمان","KYD","CI$",2),
            ("KZT","Kazakhstani Tenge","تينغ كازاخستاني","KZT","₸",2),
            ("LAK","Lao Kip","كيب لاوسي","LAK","₭",2),
            ("LBP","Lebanese Pound","ليرة لبنانية","LBP","ل.ل",2),
            ("LKR","Sri Lankan Rupee","روبية سريلانكية","LKR","Rs",2),
            ("LRD","Liberian Dollar","دولار ليبيري","LRD","L$",2),
            ("LSL","Lesotho Loti","لوتي ليسوتو","LSL","L",2),
            ("LYD","Libyan Dinar","دينار ليبي","LYD","ل.د",3),
            ("MAD","Moroccan Dirham","درهم مغربي","MAD","MAD",2),
            ("MDL","Moldovan Leu","ليو مولدوفي","MDL","L",2),
            ("MGA","Malagasy Ariary","أرياري مدغشقري","MGA","Ar",2),
            ("MKD","Macedonian Denar","دينار مقدوني","MKD","ден",2),
            ("MMK","Myanmar Kyat","كيات ميانماري","MMK","K",0),
            ("MNT","Mongolian Tugrik","توغروغ منغولي","MNT","₮",2),
            ("MOP","Macanese Pataca","باتاكا ماكاوي","MOP","MOP$",2),
            ("MRU","Mauritanian Ouguiya","أوقية موريتانية","MRU","UM",2),
            ("MUR","Mauritian Rupee","روبية موريشيوسية","MUR","₨",2),
            ("MVR","Maldivian Rufiyaa","روفيا مالديفية","MVR","Rf",2),
            ("MWK","Malawian Kwacha","كواشا ملاوي","MWK","MK",2),
            ("MXN","Mexican Peso","بيزو مكسيكي","MXN","$",2),
            ("MYR","Malaysian Ringgit","رينغيت ماليزي","MYR","RM",2),
            ("MZN","Mozambican Metical","ميتيكال موزمبيقي","MZN","MT",2),
            ("NAD","Namibian Dollar","دولار ناميبي","NAD","N$",2),
            ("NGN","Nigerian Naira","نايرا نيجيري","NGN","₦",2),
            ("NIO","Nicaraguan Córdoba","كوردوبا نيكاراغوي","NIO","C$",2),
            ("NOK","Norwegian Krone","كرون نرويجي","NOK","kr",2),
            ("NPR","Nepalese Rupee","روبية نيبالية","NPR","₨",2),
            ("NZD","New Zealand Dollar","دولار نيوزيلندي","NZD","NZ$",2),
            ("OMR","Omani Rial","ريال عماني","OMR","﷼",3),
            ("PAB","Panamanian Balboa","بالبوا بنمي","PAB","B/.",2),
            ("PEN","Peruvian Sol","سول بيروفي","PEN","S/.",2),
            ("PGK","Papua New Guinean Kina","كينا بابوا","PGK","K",2),
            ("PHP","Philippine Peso","بيزو فلبيني","PHP","₱",2),
            ("PKR","Pakistani Rupee","روبية باكستانية","PKR","₨",2),
            ("PLN","Polish Zloty","زلوتي بولندي","PLN","zł",2),
            ("PYG","Paraguayan Guarani","غواراني باراغواي","PYG","Gs",0),
            ("QAR","Qatari Rial","ريال قطري","QAR","﷼",2),
            ("RON","Romanian Leu","ليو روماني","RON","lei",2),
            ("RSD","Serbian Dinar","دينار صربي","RSD","Дин.",2),
            ("RUB","Russian Ruble","روبل روسي","RUB","₽",2),
            ("RWF","Rwandan Franc","فرنك رواندي","RWF","FRw",0),
            ("SAR","Saudi Riyal","ريال سعودي","SAR","﷼",2),
            ("SBD","Solomon Islands Dollar","دولار جزر سليمان","SBD","SI$",2),
            ("SCR","Seychellois Rupee","روبية سيشلية","SCR","₨",2),
            ("SDG","Sudanese Pound","جنيه سوداني","SDG","ج.س.",2),
            ("SEK","Swedish Krona","كرون سويدي","SEK","kr",2),
            ("SGD","Singapore Dollar","دولار سنغافوري","SGD","S$",2),
            ("SHP","Saint Helena Pound","جنيه سانت هيلينا","SHP","£",2),
            ("SLE","Sierra Leonean Leone","ليون سيراليوني","SLE","Le",2),
            ("SOS","Somali Shilling","شلن صومالي","SOS","Sh",2),
            ("SRD","Surinamese Dollar","دولار سورينامي","SRD","Sr$",2),
            ("SSP","South Sudanese Pound","جنيه جنوب سوداني","SSP","£",2),
            ("STN","São Tomé and Príncipe Dobra","دوبرا ساو تومي","STN","Db",2),
            ("SVC","Salvadoran Colón","كولون سلفادوري","SVC","₡",2),
            ("SYP","Syrian Pound","ليرة سورية","SYP","£",2),
            ("SZL","Swazi Lilangeni","ليلانجيني سوازي","SZL","L",2),
            ("THB","Thai Baht","بات تايلندي","THB","฿",2),
            ("TJS","Tajikistani Somoni","سوموني طاجيكي","TJS","SM",2),
            ("TMT","Turkmenistani Manat","مانات تركمانستاني","TMT","T",2),
            ("TND","Tunisian Dinar","دينار تونسي","TND","د.ت",3),
            ("TOP","Tongan Paʻanga","بانغا تونغي","TOP","T$",2),
            ("TRY","Turkish Lira","ليرة تركية","TRY","₺",2),
            ("TTD","Trinidad and Tobago Dollar","دولار ترينيداد","TTD","TT$",2),
            ("TWD","New Taiwan Dollar","دولار تايواني","TWD","NT$",2),
            ("TZS","Tanzanian Shilling","شلن تنزاني","TZS","TSh",2),
            ("UAH","Ukrainian Hryvnia","هريفنيا أوكرانية","UAH","₴",2),
            ("UGX","Ugandan Shilling","شلن أوغندي","UGX","USh",0),
            ("USD","US Dollar","دولار أمريكي","USD","$",2),
            ("UYU","Uruguayan Peso","بيزو أوروغواي","UYU","$U",2),
            ("UZS","Uzbekistani Som","سوم أوزبكي","UZS","лв",2),
            ("VES","Venezuelan Bolívar","بوليفار فنزويلي","VES","Bs.",2),
            ("VND","Vietnamese Dong","دونغ فيتنامي","VND","₫",0),
            ("VUV","Vanuatu Vatu","فاتو فانواتو","VUV","VT",0),
            ("WST","Samoan Tala","تالا ساموي","WST","WS$",2),
            ("XAF","Central African CFA Franc","فرنك وسط أفريقي","XAF","FCFA",0),
            ("XCD","East Caribbean Dollar","دولار شرق الكاريبي","XCD","EC$",2),
            ("XDR","Special Drawing Rights","حقوق سحب خاصة","XDR","SDR",2),
            ("XOF","West African CFA Franc","فرنك غرب أفريقي","XOF","CFA",0),
            ("XPF","CFP Franc","فرنك باسيفيكي","XPF","₣",0),
            ("YER","Yemeni Rial","ريال يمني","YER","﷼",2),
            ("ZAR","South African Rand","راند جنوب أفريقي","ZAR","R",2),
            ("ZMW","Zambian Kwacha","كواشا زامبي","ZMW","ZK",2),
            ("ZWL","Zimbabwean Dollar","دولار زيمبابوي","ZWL","Z$",2),
        };

        public static async Task SeedAsync(WelcoDbContext db, ILogger? logger = null)
        {
            var existingCodes = await db.Currencies.AsNoTracking().Select(c => c.Code).ToListAsync();
            var existingSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
            var toAdd = new List<Currency>();
            foreach (var c in Currencies)
            {
                if (existingSet.Contains(c.Code)) continue;
                var cur = Currency.Create(c.NameEn, c.NameAr, c.Code, c.Symbol, "Seeder", c.SymbolNative, c.Digits);
                toAdd.Add(cur);
            }

            if (toAdd.Count == 0)
            {
                logger?.LogInformation("CurrencySeeder: all {Count} currencies already present", Currencies.Length);
                return;
            }

var existingEntities = await db.Currencies.Where(c => existingSet.Contains(c.Code)).ToListAsync();
            foreach (var e in existingEntities)
            {
                var meta = Currencies.FirstOrDefault(x => x.Code.Equals(e.Code, StringComparison.OrdinalIgnoreCase));
                if (meta == default) continue;
                bool changed = false;
                if (string.IsNullOrWhiteSpace(e.SymbolNative) || e.SymbolNative == e.Symbol)
                {
                    
                    if (e.SymbolNative != meta.SymbolNative) { e.SymbolNative = meta.SymbolNative; changed = true; }
                }
                if (e.DecimalDigits == 0 && meta.Digits != 0) { e.DecimalDigits = meta.Digits; changed = true; }
                else if (e.DecimalDigits != meta.Digits && (e.Code != "USD" || e.DecimalDigits != 2)) {  }
                if (changed) e.MarkAsUpdated("Seeder");
            }

            if (toAdd.Count > 0)
            {
                await db.Currencies.AddRangeAsync(toAdd);
                logger?.LogInformation("CurrencySeeder: adding {Count} currencies", toAdd.Count);
            }

            await db.SaveChangesAsync();
            logger?.LogInformation("CurrencySeeder: seeded, total {Total} currencies", await db.Currencies.CountAsync());
        }
    }
}
