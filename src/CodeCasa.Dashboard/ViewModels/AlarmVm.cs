using CodeCasa.CustomEntities.Core.GoogleHome;
using CodeCasa.Dashboard.Extensions;

namespace CodeCasa.Dashboard.ViewModels
{
    public class AlarmVm
    {
        private readonly Alarm _alarm;

        public AlarmVm(string deviceName, Alarm alarm)
        {
            DeviceDescription = GetTextBeforeSpeaker(deviceName);
            Label = alarm.Label?.ToTitleCase();
            _alarm = alarm;

            Update();
        }

        public TimeSpan TimeLeft { get; private set; }
        public bool HasTimeLeft => TimeLeft > TimeSpan.Zero;
        public string? Label { get; private set; }
        public string? DeviceDescription { get; private set; }
        public DateTime DueTime { get; private set; }

        public void Update()
        {
            DueTime = DateTime.Parse(_alarm.LocalTime);
            TimeLeft = DueTime - DateTime.Now;
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
