using System.Drawing;
using CodeCasa.Lights.NetDaemon.Generated;

namespace CodeCasa.Lights.NetDaemon.Extensions
{
    internal static class LightAttributesExtensions
    {
        public static LightParameters ToLightParameters(this LightAttributes lightAttributes)
        {
            if (lightAttributes.ColorMode == null)
            {
                return LightParameters.Off();
            }
            if (string.Equals(lightAttributes.ColorMode, ColorModes.Xy, StringComparison.OrdinalIgnoreCase))
            {
                return new LightParameters { RgbColor = lightAttributes.RgbColor?.ToColor() ?? Color.White, Brightness = lightAttributes.Brightness };
            }
            if (string.Equals(lightAttributes.ColorMode, ColorModes.ColorTemp, StringComparison.OrdinalIgnoreCase))
            {
                return new LightParameters { ColorTempKelvin = (int?)lightAttributes.ColorTempKelvin, Brightness = lightAttributes.Brightness };
            }
            if (string.Equals(lightAttributes.ColorMode, ColorModes.Brightness, StringComparison.OrdinalIgnoreCase))
            {
                return new LightParameters { Brightness = lightAttributes.Brightness };
            }
            if (string.Equals(lightAttributes.ColorMode, ColorModes.OnOff, StringComparison.OrdinalIgnoreCase))
            {
                return new LightParameters { Brightness = byte.MaxValue };
            }

            throw new InvalidOperationException($"Could not convert {lightAttributes} to LightParameters.");
        }
    }
}
