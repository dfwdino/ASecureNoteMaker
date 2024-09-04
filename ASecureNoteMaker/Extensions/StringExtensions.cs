namespace ASecureNoteMaker.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static bool HasValue(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Length == 1) return str.ToLowerInvariant();
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string ToTitleCase(this string input)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}
