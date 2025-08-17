using CodeCasa.NetDaemon.Utilities.Entities;

namespace CodeCasa.Dashboard.Resolvers
{
    public class WeatherIcons
    {
        public static string GetIcon(WeatherTypes type) => type switch
        {
            WeatherTypes.ClearNight => "nightlight",
            WeatherTypes.Cloudy => "cloud",
            WeatherTypes.Fog => "foggy",
            WeatherTypes.Hail => "weather_hail",
            WeatherTypes.Lightning => "flash_on",
            WeatherTypes.LightningRainy => "thunderstorm",
            WeatherTypes.PartlyCloudy => "partly_cloudy_day",
            WeatherTypes.Pouring => "rainy_heavy",
            WeatherTypes.Rainy => "rainy",
            WeatherTypes.Snowy => "snowing",
            WeatherTypes.SnowyRainy => "rainy_snow",
            WeatherTypes.Sunny => "sunny",
            WeatherTypes.Windy => "air",
            WeatherTypes.WindyCloudy => "air",
            WeatherTypes.Exceptional => "warning",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
