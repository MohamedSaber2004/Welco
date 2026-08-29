namespace Welco.Shared.Common.Options
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string[]? ValidIssuers { get; set; }
        public string[]? ValidAudiences { get; set; }
        public int ExpiryInMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 30;
        public string Secret { get; set; } = string.Empty;

        public IEnumerable<string> GetAllValidIssuers()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Issuer))
            {
                list.Add(Issuer);
                list.Add(Issuer.TrimEnd('/'));
                list.Add(Issuer.TrimEnd('/') + "/");
            }
            if (ValidIssuers != null)
            {
                foreach (var iss in ValidIssuers.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    list.Add(iss);
                    list.Add(iss.TrimEnd('/'));
                    list.Add(iss.TrimEnd('/') + "/");
                }
            }
            return list.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetAllValidAudiences()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Audience))
            {
                list.Add(Audience);
                list.Add(Audience.TrimEnd('/'));
                list.Add(Audience.TrimEnd('/') + "/");
            }
            if (ValidAudiences != null)
            {
                foreach (var aud in ValidAudiences.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    list.Add(aud);
                    list.Add(aud.TrimEnd('/'));
                    list.Add(aud.TrimEnd('/') + "/");
                }
            }
            return list.Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}

