using System.Globalization;

namespace CodeCasa.Dashboard.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToAlarmDueFormat(this DateTime alarm, CultureInfo culture)
        {
            var now = DateTime.Now;

            var timeFormat = alarm.Second == 0 ? "HH:mm" : "HH:mm:ss";
            var timePart = alarm.ToString(timeFormat, culture);

            if (alarm.Date == now.Date)
            {
                return timePart;
            }

            if (alarm.Date == now.Date.AddDays(1))
            {
                return $"Morgen, {timePart}";
            }

            var dayName = culture.DateTimeFormat.GetDayName(alarm.DayOfWeek);
            return $"{dayName}, {timePart}";
        }
    }
}
