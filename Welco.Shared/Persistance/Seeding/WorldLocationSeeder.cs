using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Domain.Models;
using Welco.Shared.Persistance;

namespace Welco.Shared.Persistance.Seeding
{
    public static class WorldLocationSeeder
    {
        
        private static readonly (string Code, string NameEn, string NameAr, string PhoneCode)[] Countries =
        {
            ("AF","Afghanistan","أفغانستان","+93"),("AX","Åland Islands","جزر أولاند","+358"),("AL","Albania","ألبانيا","+355"),("DZ","Algeria","الجزائر","+213"),("AS","American Samoa","ساموا الأمريكية","+1684"),("AD","Andorra","أندورا","+376"),("AO","Angola","أنغولا","+244"),("AI","Anguilla","أنغويلا","+1264"),("AQ","Antarctica","أنتاركتيكا","+672"),("AG","Antigua and Barbuda","أنتيغوا وبربودا","+1268"),("AR","Argentina","الأرجنتين","+54"),("AM","Armenia","أرمينيا","+374"),("AW","Aruba","أروبا","+297"),("AU","Australia","أستراليا","+61"),("AT","Austria","النمسا","+43"),("AZ","Azerbaijan","أذربيجان","+994"),("BS","Bahamas","جزر البهاما","+1242"),("BH","Bahrain","البحرين","+973"),("BD","Bangladesh","بنغلاديش","+880"),("BB","Barbados","بربادوس","+1246"),("BY","Belarus","بيلاروسيا","+375"),("BE","Belgium","بلجيكا","+32"),("BZ","Belize","بليز","+501"),("BJ","Benin","بنين","+229"),("BM","Bermuda","برمودا","+1441"),("BT","Bhutan","بوتان","+975"),("BO","Bolivia","بوليفيا","+591"),("BQ","Bonaire","بونير","+599"),("BA","Bosnia and Herzegovina","البوسنة والهرسك","+387"),("BW","Botswana","بوتسوانا","+267"),("BV","Bouvet Island","جزيرة بوفيه","+47"),("BR","Brazil","البرازيل","+55"),("IO","British Indian Ocean Territory","إقليم المحيط الهندي البريطاني","+246"),("BN","Brunei","بروناي","+673"),("BG","Bulgaria","بلغاريا","+359"),("BF","Burkina Faso","بوركينا فاسو","+226"),("BI","Burundi","بوروندي","+257"),("CV","Cabo Verde","الرأس الأخضر","+238"),("KH","Cambodia","كمبوديا","+855"),("CM","Cameroon","الكاميرون","+237"),("CA","Canada","كندا","+1"),("KY","Cayman Islands","جزر كايمان","+1345"),("CF","Central African Republic","جمهورية أفريقيا الوسطى","+236"),("TD","Chad","تشاد","+235"),("CL","Chile","تشيلي","+56"),("CN","China","الصين","+86"),("CX","Christmas Island","جزيرة الكريسماس","+61"),("CC","Cocos Islands","جزر كوكوس","+61"),("CO","Colombia","كولومبيا","+57"),("KM","Comoros","جزر القمر","+269"),("CG","Congo","الكونغو","+242"),("CD","Congo, Democratic Republic","الكونغو الديمقراطية","+243"),("CK","Cook Islands","جزر كوك","+682"),("CR","Costa Rica","كوستاريكا","+506"),("CI","Côte d'Ivoire","ساحل العاج","+225"),("HR","Croatia","كرواتيا","+385"),("CU","Cuba","كوبا","+53"),("CW","Curaçao","كوراساو","+599"),("CY","Cyprus","قبرص","+357"),("CZ","Czech Republic","التشيك","+420"),("DK","Denmark","الدنمارك","+45"),("DJ","Djibouti","جيبوتي","+253"),("DM","Dominica","دومينيكا","+1767"),("DO","Dominican Republic","الدومينيكان","+1809"),("EC","Ecuador","الإكوادور","+593"),("EG","Egypt","مصر","+20"),("SV","El Salvador","السلفادور","+503"),("GQ","Equatorial Guinea","غينيا الاستوائية","+240"),("ER","Eritrea","إريتريا","+291"),("EE","Estonia","إستونيا","+372"),("SZ","Eswatini","إسواتيني","+268"),("ET","Ethiopia","إثيوبيا","+251"),("FK","Falkland Islands","جزر فوكلاند","+500"),("FO","Faroe Islands","جزر فارو","+298"),("FJ","Fiji","فيجي","+679"),("FI","Finland","فنلندا","+358"),("FR","France","فرنسا","+33"),("GF","French Guiana","غويانا الفرنسية","+594"),("PF","French Polynesia","بولينيزيا الفرنسية","+689"),("TF","French Southern Territories","المناطق الجنوبية الفرنسية","+262"),("GA","Gabon","الغابون","+241"),("GM","Gambia","غامبيا","+220"),("GE","Georgia","جورجيا","+995"),("DE","Germany","ألمانيا","+49"),("GH","Ghana","غانا","+233"),("GI","Gibraltar","جبل طارق","+350"),("GR","Greece","اليونان","+30"),("GL","Greenland","غرينلاند","+299"),("GD","Grenada","غرينادا","+1473"),("GP","Guadeloupe","غوادلوب","+590"),("GU","Guam","غوام","+1671"),("GT","Guatemala","غواتيمالا","+502"),("GG","Guernsey","غيرنسي","+44"),("GN","Guinea","غينيا","+224"),("GW","Guinea-Bissau","غينيا بيساو","+245"),("GY","Guyana","غيانا","+592"),("HT","Haiti","هايتي","+509"),("HM","Heard Island","جزيرة هيرد","+672"),("VA","Holy See","الفاتيكان","+379"),("HN","Honduras","هندوراس","+504"),("HK","Hong Kong","هونغ كونغ","+852"),("HU","Hungary","المجر","+36"),("IS","Iceland","آيسلندا","+354"),("IN","India","الهند","+91"),("ID","Indonesia","إندونيسيا","+62"),("IR","Iran","إيران","+98"),("IQ","Iraq","العراق","+964"),("IE","Ireland","أيرلندا","+353"),("IM","Isle of Man","جزيرة مان","+44"),("IL","Israel","إسرائيل","+972"),("IT","Italy","إيطاليا","+39"),("JM","Jamaica","جامايكا","+1876"),("JP","Japan","اليابان","+81"),("JE","Jersey","جيرسي","+44"),("JO","Jordan","الأردن","+962"),("KZ","Kazakhstan","كازاخستان","+7"),("KE","Kenya","كينيا","+254"),("KI","Kiribati","كيريباتي","+686"),("KP","North Korea","كوريا الشمالية","+850"),("KR","South Korea","كوريا الجنوبية","+82"),("KW","Kuwait","الكويت","+965"),("KG","Kyrgyzstan","قيرغيزستان","+996"),("LA","Laos","لاوس","+856"),("LV","Latvia","لاتفيا","+371"),("LB","Lebanon","لبنان","+961"),("LS","Lesotho","ليسوتو","+266"),("LR","Liberia","ليبيريا","+231"),("LY","Libya","ليبيا","+218"),("LI","Liechtenstein","ليختنشتاين","+423"),("LT","Lithuania","ليتوانيا","+370"),("LU","Luxembourg","لوكسمبورغ","+352"),("MO","Macao","ماكاو","+853"),("MK","North Macedonia","مقدونيا الشمالية","+389"),("MG","Madagascar","مدغشقر","+261"),("MW","Malawi","مالاوي","+265"),("MY","Malaysia","ماليزيا","+60"),("MV","Maldives","المالديف","+960"),("ML","Mali","مالي","+223"),("MT","Malta","مالطا","+356"),("MH","Marshall Islands","جزر مارشال","+692"),("MQ","Martinique","مارتينيك","+596"),("MR","Mauritania","موريتانيا","+222"),("MU","Mauritius","موريشيوس","+230"),("YT","Mayotte","مايوت","+262"),("MX","Mexico","المكسيك","+52"),("FM","Micronesia","ميكرونيزيا","+691"),("MD","Moldova","مولدوفا","+373"),("MC","Monaco","موناكو","+377"),("MN","Mongolia","منغوليا","+976"),("ME","Montenegro","الجبل الأسود","+382"),("MS","Montserrat","مونتسرات","+1664"),("MA","Morocco","المغرب","+212"),("MZ","Mozambique","موزمبيق","+258"),("MM","Myanmar","ميانمار","+95"),("NA","Namibia","ناميبيا","+264"),("NR","Nauru","ناورو","+674"),("NP","Nepal","نيبال","+977"),("NL","Netherlands","هولندا","+31"),("NC","New Caledonia","كاليدونيا الجديدة","+687"),("NZ","New Zealand","نيوزيلندا","+64"),("NI","Nicaragua","نيكاراغوا","+505"),("NE","Niger","النيجر","+227"),("NG","Nigeria","نيجيريا","+234"),("NU","Niue","نيوي","+683"),("NF","Norfolk Island","جزيرة نورفولك","+672"),("MP","Northern Mariana Islands","جزر ماريانا الشمالية","+1670"),("NO","Norway","النرويج","+47"),("OM","Oman","عمان","+968"),("PK","Pakistan","باكستان","+92"),("PW","Palau","بالاو","+680"),("PS","Palestine","فلسطين","+970"),("PA","Panama","بنما","+507"),("PG","Papua New Guinea","بابوا غينيا الجديدة","+675"),("PY","Paraguay","باراغواي","+595"),("PE","Peru","بيرو","+51"),("PH","Philippines","الفلبين","+63"),("PN","Pitcairn","جزر بيتكيرن","+64"),("PL","Poland","بولندا","+48"),("PT","Portugal","البرتغال","+351"),("PR","Puerto Rico","بورتوريكو","+1787"),("QA","Qatar","قطر","+974"),("RE","Réunion","ريونيون","+262"),("RO","Romania","رومانيا","+40"),("RU","Russia","روسيا","+7"),("RW","Rwanda","رواندا","+250"),("BL","Saint Barthélemy","سان بارتيلمي","+590"),("SH","Saint Helena","سانت هيلينا","+290"),("KN","Saint Kitts and Nevis","سانت كيتس ونيفيس","+1869"),("LC","Saint Lucia","سانت لوسيا","+1758"),("MF","Saint Martin","سان مارتن","+590"),("PM","Saint Pierre and Miquelon","سان بيير وميكلون","+508"),("VC","Saint Vincent and the Grenadines","سانت فنسنت","+1784"),("WS","Samoa","ساموا","+685"),("SM","San Marino","سان مارينو","+378"),("ST","Sao Tome and Principe","ساو تومي","+239"),("SA","Saudi Arabia","السعودية","+966"),("SN","Senegal","السنغال","+221"),("RS","Serbia","صربيا","+381"),("SC","Seychelles","سيشل","+248"),("SL","Sierra Leone","سيراليون","+232"),("SG","Singapore","سنغافورة","+65"),("SX","Sint Maarten","سانت مارتن","+1721"),("SK","Slovakia","سلوفاكيا","+421"),("SI","Slovenia","سلوفينيا","+386"),("SB","Solomon Islands","جزر سليمان","+677"),("SO","Somalia","الصومال","+252"),("ZA","South Africa","جنوب أفريقيا","+27"),("GS","South Georgia","جورجيا الجنوبية","+500"),("SS","South Sudan","جنوب السودان","+211"),("ES","Spain","إسبانيا","+34"),("LK","Sri Lanka","سريلانكا","+94"),("SD","Sudan","السودان","+249"),("SR","Suriname","سورينام","+597"),("SJ","Svalbard and Jan Mayen","سفالبارد","+47"),("SE","Sweden","السويد","+46"),("CH","Switzerland","سويسرا","+41"),("SY","Syria","سوريا","+963"),("TW","Taiwan","تايوان","+886"),("TJ","Tajikistan","طاجيكستان","+992"),("TZ","Tanzania","تنزانيا","+255"),("TH","Thailand","تايلاند","+66"),("TL","Timor-Leste","تيمور الشرقية","+670"),("TG","Togo","توغو","+228"),("TK","Tokelau","توكيلاو","+690"),("TO","Tonga","تونغا","+676"),("TT","Trinidad and Tobago","ترينيداد وتوباغو","+1868"),("TN","Tunisia","تونس","+216"),("TR","Turkey","تركيا","+90"),("TM","Turkmenistan","تركمانستان","+993"),("TC","Turks and Caicos","جزر تركس وكايكوس","+1649"),("TV","Tuvalu","توفالو","+688"),("UG","Uganda","أوغندا","+256"),("UA","Ukraine","أوكرانيا","+380"),("AE","United Arab Emirates","الإمارات","+971"),("GB","United Kingdom","المملكة المتحدة","+44"),("US","United States","الولايات المتحدة","+1"),("UM","United States Minor Outlying Islands","جزر الولايات البعيدة","+1"),("UY","Uruguay","أوروغواي","+598"),("UZ","Uzbekistan","أوزبكستان","+998"),("VU","Vanuatu","فانواتو","+678"),("VE","Venezuela","فنزويلا","+58"),("VN","Vietnam","فيتنام","+84"),("VG","British Virgin Islands","جزر العذراء البريطانية","+1284"),("VI","U.S. Virgin Islands","جزر العذراء الأمريكية","+1340"),("WF","Wallis and Futuna","واليس وفوتونا","+681"),("EH","Western Sahara","الصحراء الغربية","+212"),("YE","Yemen","اليمن","+967"),("ZM","Zambia","زامبيا","+260"),("ZW","Zimbabwe","زيمبابوي","+263"),
        };

private static readonly Dictionary<string, string[]> CapitalMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AF"] = new[] { "Kabul", "Kandahar" },
            ["AL"] = new[] { "Tirana" },
            ["DZ"] = new[] { "Algiers", "Oran" },
            ["AD"] = new[] { "Andorra la Vella" },
            ["AO"] = new[] { "Luanda" },
            ["AR"] = new[] { "Buenos Aires", "Córdoba" },
            ["AM"] = new[] { "Yerevan" },
            ["AU"] = new[] { "Canberra", "Sydney", "Melbourne" },
            ["AT"] = new[] { "Vienna" },
            ["AZ"] = new[] { "Baku" },
            ["BH"] = new[] { "Manama" },
            ["BD"] = new[] { "Dhaka", "Chittagong" },
            ["BY"] = new[] { "Minsk" },
            ["BE"] = new[] { "Brussels" },
            ["BZ"] = new[] { "Belmopan" },
            ["BJ"] = new[] { "Porto-Novo" },
            ["BT"] = new[] { "Thimphu" },
            ["BO"] = new[] { "Sucre", "La Paz" },
            ["BA"] = new[] { "Sarajevo" },
            ["BW"] = new[] { "Gaborone" },
            ["BR"] = new[] { "Brasília", "São Paulo", "Rio de Janeiro" },
            ["BN"] = new[] { "Bandar Seri Begawan" },
            ["BG"] = new[] { "Sofia" },
            ["BF"] = new[] { "Ouagadougou" },
            ["BI"] = new[] { "Gitega" },
            ["KH"] = new[] { "Phnom Penh" },
            ["CM"] = new[] { "Yaoundé" },
            ["CA"] = new[] { "Ottawa", "Toronto", "Vancouver" },
            ["CL"] = new[] { "Santiago" },
            ["CN"] = new[] { "Beijing", "Shanghai", "Shenzhen" },
            ["CO"] = new[] { "Bogotá" },
            ["CR"] = new[] { "San José" },
            ["HR"] = new[] { "Zagreb" },
            ["CU"] = new[] { "Havana" },
            ["CY"] = new[] { "Nicosia" },
            ["CZ"] = new[] { "Prague" },
            ["DK"] = new[] { "Copenhagen" },
            ["DJ"] = new[] { "Djibouti" },
            ["DO"] = new[] { "Santo Domingo" },
            ["EC"] = new[] { "Quito" },
            ["EG"] = new[] { "Cairo", "Alexandria", "Giza", "Luxor" },
            ["SV"] = new[] { "San Salvador" },
            ["EE"] = new[] { "Tallinn" },
            ["ET"] = new[] { "Addis Ababa" },
            ["FI"] = new[] { "Helsinki" },
            ["FR"] = new[] { "Paris", "Lyon", "Marseille" },
            ["DE"] = new[] { "Berlin", "Munich", "Hamburg" },
            ["GH"] = new[] { "Accra" },
            ["GR"] = new[] { "Athens" },
            ["GT"] = new[] { "Guatemala City" },
            ["HN"] = new[] { "Tegucigalpa" },
            ["HK"] = new[] { "Hong Kong" },
            ["HU"] = new[] { "Budapest" },
            ["IS"] = new[] { "Reykjavík" },
            ["IN"] = new[] { "New Delhi", "Mumbai", "Bangalore", "Kolkata" },
            ["ID"] = new[] { "Jakarta", "Surabaya" },
            ["IR"] = new[] { "Tehran" },
            ["IQ"] = new[] { "Baghdad", "Basra" },
            ["IE"] = new[] { "Dublin" },
            ["IL"] = new[] { "Jerusalem", "Tel Aviv" },
            ["IT"] = new[] { "Rome", "Milan" },
            ["JP"] = new[] { "Tokyo", "Osaka", "Kyoto" },
            ["JO"] = new[] { "Amman" },
            ["KZ"] = new[] { "Astana" },
            ["KE"] = new[] { "Nairobi" },
            ["KR"] = new[] { "Seoul", "Busan" },
            ["KW"] = new[] { "Kuwait City" },
            ["KG"] = new[] { "Bishkek" },
            ["LA"] = new[] { "Vientiane" },
            ["LB"] = new[] { "Beirut" },
            ["LY"] = new[] { "Tripoli" },
            ["MA"] = new[] { "Rabat", "Casablanca", "Marrakech" },
            ["MX"] = new[] { "Mexico City", "Guadalajara" },
            ["MY"] = new[] { "Kuala Lumpur" },
            ["NL"] = new[] { "Amsterdam" },
            ["NZ"] = new[] { "Wellington", "Auckland" },
            ["NG"] = new[] { "Abuja", "Lagos" },
            ["NO"] = new[] { "Oslo" },
            ["PK"] = new[] { "Islamabad", "Karachi", "Lahore" },
            ["PS"] = new[] { "Ramallah" },
            ["PA"] = new[] { "Panama City" },
            ["PE"] = new[] { "Lima" },
            ["PH"] = new[] { "Manila", "Cebu" },
            ["PL"] = new[] { "Warsaw" },
            ["PT"] = new[] { "Lisbon" },
            ["QA"] = new[] { "Doha" },
            ["RO"] = new[] { "Bucharest" },
            ["RU"] = new[] { "Moscow", "Saint Petersburg" },
            ["SA"] = new[] { "Riyadh", "Jeddah", "Dammam", "Mecca" },
            ["SN"] = new[] { "Dakar" },
            ["RS"] = new[] { "Belgrade" },
            ["SG"] = new[] { "Singapore" },
            ["ZA"] = new[] { "Pretoria", "Cape Town", "Johannesburg" },
            ["ES"] = new[] { "Madrid", "Barcelona" },
            ["SE"] = new[] { "Stockholm" },
            ["CH"] = new[] { "Bern", "Zurich" },
            ["SY"] = new[] { "Damascus" },
            ["TW"] = new[] { "Taipei" },
            ["TH"] = new[] { "Bangkok" },
            ["TR"] = new[] { "Ankara", "Istanbul" },
            ["UA"] = new[] { "Kyiv" },
            ["AE"] = new[] { "Abu Dhabi", "Dubai", "Sharjah" },
            ["GB"] = new[] { "London", "Manchester", "Birmingham" },
            ["US"] = new[] { "Washington", "New York", "Los Angeles", "Chicago", "Houston" },
            ["UY"] = new[] { "Montevideo" },
            ["VE"] = new[] { "Caracas" },
            ["VN"] = new[] { "Hanoi", "Ho Chi Minh City" },
            ["YE"] = new[] { "Sanaa", "Aden" },
            ["ZM"] = new[] { "Lusaka" },
            ["ZW"] = new[] { "Harare" },
        };

        public static async Task SeedAsync(WelcoDbContext db, ILogger? logger = null)
        {
            
            var existing = await db.Countries.AsNoTracking().Select(c => new { c.Code, c.NameEn }).ToListAsync();
            var existSet = new HashSet<string>(existing.Select(e => $"{e.Code}|{e.NameEn}"), StringComparer.OrdinalIgnoreCase);
            var toAddCountries = new List<Country>();
            foreach (var c in Countries)
            {
                var key = $"{c.Code}|{c.NameEn}";
                if (existSet.Contains(key)) continue;
                
                var existsSameCodeName = existing.Any(e => string.Equals(e.Code, c.Code, StringComparison.OrdinalIgnoreCase) && string.Equals(e.NameEn, c.NameEn, StringComparison.OrdinalIgnoreCase));
                if (existsSameCodeName) continue;
                var ent = Country.Create(c.NameEn, c.NameAr, c.Code, c.PhoneCode, "Seeder");
                toAddCountries.Add(ent);
            }
            if (toAddCountries.Count > 0)
            {
                await db.Countries.AddRangeAsync(toAddCountries);
                await db.SaveChangesAsync();
                logger?.LogInformation("WorldLocationSeeder: added {Count} countries", toAddCountries.Count);
            }

var allCountries = await db.Countries.AsNoTracking().ToListAsync();
            var countryByCode = allCountries.GroupBy(c => c.Code ?? "").ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

var existingCities = await db.Cities.AsNoTracking().Select(c => new { c.CountryId, c.NameEn }).ToListAsync();
            var citySet = new HashSet<string>(existingCities.Select(e => $"{e.CountryId}|{e.NameEn}"), StringComparer.OrdinalIgnoreCase);
            var toAddCities = new List<City>();
            foreach (var c in Countries)
            {
                if (!countryByCode.TryGetValue(c.Code, out var countryEnt)) continue;
                var cityNames = CapitalMap.TryGetValue(c.Code, out var arr) ? arr : new[] { c.NameEn + " City" };
                foreach (var cityName in cityNames)
                {
                    var cityKey = $"{countryEnt.Id}|{cityName}";
                    if (citySet.Contains(cityKey)) continue;
                    var city = City.Create(countryEnt.Id, cityName, cityName, "Seeder");
                    toAddCities.Add(city);
                    citySet.Add(cityKey);
                }
            }
            if (toAddCities.Count > 0)
            {
                await db.Cities.AddRangeAsync(toAddCities);
                await db.SaveChangesAsync();
                logger?.LogInformation("WorldLocationSeeder: added {Count} cities", toAddCities.Count);
            }

var allCities = await db.Cities.AsNoTracking().ToListAsync();
            var existingZones = await db.Zones.AsNoTracking().Select(z => new { z.CityId, z.NameEn }).ToListAsync();
            var zoneSet = new HashSet<string>(existingZones.Select(e => $"{e.CityId}|{e.NameEn}"), StringComparer.OrdinalIgnoreCase);
            var toAddZones = new List<Zone>();
            var zoneNames = new[] { "Central", "North", "South", "East", "West" };
            foreach (var city in allCities)
            {
                
                for (int i = 0; i < 2; i++)
                {
                    var zName = $"{city.NameEn} - {zoneNames[i % zoneNames.Length]}";
                    var key = $"{city.Id}|{zName}";
                    if (zoneSet.Contains(key)) continue;
                    var zone = Zone.Create(city.Id, zName, zName, "Seeder");
                    toAddZones.Add(zone);
                    zoneSet.Add(key);
                }
            }
            if (toAddZones.Count > 0)
            {
                await db.Zones.AddRangeAsync(toAddZones);
                await db.SaveChangesAsync();
                logger?.LogInformation("WorldLocationSeeder: added {Count} zones", toAddZones.Count);
            }

            logger?.LogInformation("WorldLocationSeeder done: Countries {C} Cities {Ci} Zones {Z}", await db.Countries.CountAsync(), await db.Cities.CountAsync(), await db.Zones.CountAsync());
        }
    }
}
