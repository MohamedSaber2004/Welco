using System.Text.RegularExpressions;

namespace Welco.Shared.Common.Helpers
{
    public static class PhoneNumberNormalizer
    {
        private static readonly Regex NonDigits = new(@"[^\d+]", RegexOptions.Compiled);

        public static string? Normalize(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            var raw = NonDigits.Replace(phoneNumber.Trim(), string.Empty);

            var hasPlus = raw.StartsWith('+');
            var digits = raw.Replace("+", string.Empty);

            if (digits.Length == 0)
                return null;

            if (digits.StartsWith("00", StringComparison.Ordinal))
            {
                digits = digits[2..];
                hasPlus = true;
            }

            return hasPlus ? $"+{digits}" : digits;
        }

        public static bool AreEquivalent(string? a, string? b)
        {
            var left = Normalize(a);
            var right = Normalize(b);

            if (left is null || right is null)
                return left is null && right is null;

            return string.Equals(left, right, StringComparison.Ordinal);
        }
    }
}
