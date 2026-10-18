using System.Text.RegularExpressions;

namespace Welco.Shared.Common.Helpers
{
    public static class PhoneNumberNormalizer
    {
        private static readonly Regex NonDigits = new(@"[^\d+]", RegexOptions.Compiled);

        private static readonly string[] KnownTrunkDialPrefixes = new[]
        {
            "971", "966", "965", "974", "973", "968", "962", "961", "963", "964",
            "970", "972", "20", "92", "44", "49", "33", "39", "34", "91", "90"
        };

        public static string? Normalize(string? phoneNumber, string? phoneCode = null)
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

            // If phoneCode is explicitly supplied (e.g. "+92" or "92" or "+20")
            if (!string.IsNullOrWhiteSpace(phoneCode))
            {
                var cleanCode = NonDigits.Replace(phoneCode.Trim(), string.Empty).Replace("+", string.Empty);
                if (cleanCode.Length > 0)
                {
                    if (digits.StartsWith(cleanCode, StringComparison.Ordinal))
                    {
                        var national = digits[cleanCode.Length..];
                        if (national.StartsWith('0'))
                            national = national.TrimStart('0');
                        return $"+{cleanCode}{national}";
                    }
                    else
                    {
                        var national = digits.TrimStart('0');
                        return $"+{cleanCode}{national}";
                    }
                }
            }

            if (hasPlus)
            {
                // Edge case: mistakenly stored as +1 03xx (e.g. +1 03000959336).
                // In North American NANP (+1), valid area codes are [2-9][0-8][0-9] and never start with 0.
                // 03xx (11 digits) is standard Pakistan mobile prefix.
                if (digits.StartsWith("103", StringComparison.Ordinal) && digits.Length == 12)
                {
                    var pkNational = digits[2..]; // "300..."
                    return $"+92{pkNational}";
                }

                foreach (var prefix in KnownTrunkDialPrefixes)
                {
                    if (digits.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        var rest = digits[prefix.Length..];
                        if (rest.StartsWith('0'))
                        {
                            rest = rest.TrimStart('0');
                            return $"+{prefix}{rest}";
                        }
                        break;
                    }
                }

                return $"+{digits}";
            }

            return digits;
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