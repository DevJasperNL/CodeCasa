using System.Text;

namespace CodeCasa.Dashboard.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToShortDisplayFormat(this TimeSpan timeSpan)
        {
            var years = timeSpan.Days / 365;
            var days = timeSpan.Days % 365;
            var hours = timeSpan.Hours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            var sb = new StringBuilder();

            if (years > 0)
                sb.Append($"{years}y ");
            if (days > 0)
                sb.Append($"{days}d ");

            if (hours > 0 || years > 0 || days > 0)
                sb.Append($"{hours:D2}:{minutes:D2}:{seconds:D2}");
            else
                sb.Append($"{minutes:D2}:{seconds:D2}");

            return sb.ToString().Trim();
        }
    }
}
