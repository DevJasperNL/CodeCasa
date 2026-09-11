using System.Drawing;
using CodeCasa.Lights.NetDaemon.Generated;

namespace CodeCasa.Lights.NetDaemon.Extensions;

internal static class LightAttributesExtensions
{
    public static LightParameters ToLightParameters(this LightAttributes lightAttributes)
    {
        var colorMode = lightAttributes.ColorMode;
        if (colorMode == null)
        {
            return LightParameters.Off();
        }
        if (ColorModes.RgbBased.Contains(colorMode, StringComparer.OrdinalIgnoreCase))
        {
            return new LightParameters { RgbColor = lightAttributes.RgbColor?.ToColor() ?? Color.White, Brightness = lightAttributes.Brightness };
        }
        if (string.Equals(colorMode, ColorModes.ColorTemp, StringComparison.OrdinalIgnoreCase))
        {
            return new LightParameters { ColorTempKelvin = (int?)lightAttributes.ColorTempKelvin, Brightness = lightAttributes.Brightness };
        }
        if (string.Equals(colorMode, ColorModes.Brightness, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(colorMode, ColorModes.White, StringComparison.OrdinalIgnoreCase))
        {
            return new LightParameters { Brightness = lightAttributes.Brightness };
        }
        if (string.Equals(colorMode, ColorModes.OnOff, StringComparison.OrdinalIgnoreCase))
        {
            return new LightParameters { Brightness = byte.MaxValue };
        }

        // Unknown or future colour modes: use whatever colour information is present rather than failing every state read.
        if (lightAttributes.RgbColor != null)
        {
            return new LightParameters { RgbColor = lightAttributes.RgbColor.ToColor(), Brightness = lightAttributes.Brightness };
        }
        if (lightAttributes.ColorTempKelvin != null)
        {
            return new LightParameters { ColorTempKelvin = (int?)lightAttributes.ColorTempKelvin, Brightness = lightAttributes.Brightness };
        }
        return new LightParameters { Brightness = lightAttributes.Brightness };
    }
}
