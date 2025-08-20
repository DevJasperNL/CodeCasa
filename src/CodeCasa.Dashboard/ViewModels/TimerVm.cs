using CodeCasa.Dashboard.Extensions;
using Timer = CodeCasa.CustomEntities.Core.GoogleHome.Timer;

namespace CodeCasa.Dashboard.ViewModels
{
    public class TimerVm
    {
        private readonly Timer _timer;

        public TimerVm(string deviceName, Timer timer)
        {
            DeviceDescription = GetTextBeforeSpeaker(deviceName);
            Label = timer.Label?.ToTitleCase();
            _timer = timer;

            Update();
        }

        public TimeSpan TimeLeft { get; private set; }
        public double ProgressValue { get; private set; }
        public double ProgressMax { get; private set; }
        public bool HasTimeLeft => TimeLeft > TimeSpan.Zero;
        public string? Label { get; private set; }
        public string? DeviceDescription { get; private set; }

        public void Update()
        {
            var localTime = DateTime.Parse(_timer.LocalTime);
            TimeLeft = localTime - DateTime.Now;
            var duration = ParseDuration(_timer.Duration);

            ProgressMax = duration.TotalSeconds;
            ProgressValue = TimeLeft.TotalSeconds;
        }

        private static TimeSpan ParseDuration(string timeSpanFormat)
        {
            // Google sends something like "40 days, 6:57:13" or "1 day, 6:57:13".
            var normalized = timeSpanFormat.Replace(" days", "").Replace(" day", "").Replace(", ", ".");
            return TimeSpan.Parse(normalized);
        }

        private static string GetTextBeforeSpeaker(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var index = input.IndexOf("speaker", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return input;

            return input.Substring(0, index).Trim();
        }


    }
}
