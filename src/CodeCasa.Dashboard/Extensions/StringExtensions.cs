using System.Globalization;

namespace CodeCasa.Dashboard.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Words that are not capitalized unless they are first or last
            string[] smallWords = { "a", "an", "the", "and", "but", "or", "for", "nor", "on", "at", "to", "from", "by", "over", "in" };

            var words = input.Split(' ');
            for (var i = 0; i < words.Length; i++)
            {
                var lower = words[i].ToLower();
                if (i == 0 || i == words.Length - 1 || !smallWords.Contains(lower))
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lower);
                }
                else
                {
                    words[i] = lower;
                }
            }
            return string.Join(" ", words);
        }
    }
}
