using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCasa.CustomEntities.Weather
{
    public enum WeatherTypes
    {
        ClearNight,       // The sky is clear during the night
        Cloudy,           // There are many clouds in the sky
        Fog,              // There is a thick mist or fog reducing visibility
        Hail,             // Hailstones are falling
        Lightning,        // Lightning/thunderstorms are occurring
        LightningRainy,   // Lightning/thunderstorm is occurring along with rain
        PartlyCloudy,     // The sky is partially covered with clouds
        Pouring,          // It is raining heavily
        Rainy,            // It is raining
        Snowy,            // It is snowing
        SnowyRainy,       // It is snowing and raining at the same time
        Sunny,            // The sky is clear and the sun is shining
        Windy,            // It is windy
        WindyCloudy,      // It is windy and cloudy
        Exceptional       // Exceptional weather conditions are occurring
    }
}
