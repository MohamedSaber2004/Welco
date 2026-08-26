namespace Welco.Shared.Common.Extensions
{
    public static class GuidExtensions
    {
        public static Guid ToGuid(this string? value, Guid defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Guid.TryParse(value.Trim(), out var result) ? result : defaultValue;
        }

        public static Guid? ToGuidOrNull(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Guid.TryParse(value.Trim(), out var result) ? result : null;
        }

        public static bool TryToGuid(this string? value, out Guid result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = Guid.Empty;
                return false;
            }

            return Guid.TryParse(value.Trim(), out result);
        }

        public static bool IsGuid(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Guid.TryParse(value.Trim(), out _);
        }

        public static string ToStringGuid(this Guid value, string format = "D")
        {
            return value.ToString(format);
        }

        public static string ToStringGuid(this Guid? value, string format = "D", string defaultValue = "")
        {
            return value.HasValue ? value.Value.ToString(format) : defaultValue;
        }

        public static List<Guid> ToGuidList(this IEnumerable<string>? values)
        {
            if (values == null)
                return new List<Guid>();

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v) && Guid.TryParse(v.Trim(), out _))
                .Select(v => Guid.Parse(v.Trim()))
                .ToList();
        }

        public static List<string> ToStringList(this IEnumerable<Guid>? values, string format = "D")
        {
            if (values == null)
                return new List<string>();

            return values
                .Select(v => v.ToString(format))
                .ToList();
        }
    }
}
