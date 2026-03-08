using System.Drawing;
using ColorConverter = CodeCasa.Lights.Utils.ColorConverter;

namespace CodeCasa.Lights.Extensions;

/// <summary>
/// Provides extension methods for <see cref="LightParameters"/> to support light transitions and interpolation.
/// </summary>
public static class LightParametersExtensions
{
    /// <summary>
    /// Interpolates between two <see cref="LightParameters"/> objects to create an intermediate light state.
    /// Uses gamma-corrected blending for RGB colors and linear interpolation for color temperature and brightness.
    /// </summary>
    /// <param name="fromLightParameters">The starting light parameters.</param>
    /// <param name="toLightParameters">The ending light parameters.</param>
    /// <param name="progress">The interpolation progress between 0 and 1, where 0 returns fromLightParameters and 1 returns toLightParameters.</param>
    /// <returns>A new <see cref="LightParameters"/> object representing the interpolated light state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the parameters cannot be interpolated due to missing color or color temperature values.</exception>
    public static LightParameters Interpolate(this LightParameters fromLightParameters,
            LightParameters toLightParameters, double progress)
    {
        if (fromLightParameters.Brightness == 0 && toLightParameters.Brightness == 0)
        {
            return LightParameters.Off();
        }

        // If one of the parameters have a brightness of 0 and no color values, we copy the color values from the other parameters.
        if (fromLightParameters.Brightness == 0)
        {
            if (fromLightParameters.ColorTempKelvin == null && fromLightParameters.RgbColor == null)
            {
                fromLightParameters = toLightParameters with { Brightness = 0 };
            }
        }
        else if (toLightParameters.Brightness == 0)
        {
            if (toLightParameters.ColorTempKelvin == null && toLightParameters.RgbColor == null)
            {
                toLightParameters = fromLightParameters with { Brightness = 0 };
            }
        }

        var result = fromLightParameters with
        {
            Brightness = InterpolateBrightness(fromLightParameters.Brightness, toLightParameters.Brightness, progress)
        };

        // If any of parameters has a rgb color, we use that for the result.
        if (fromLightParameters.RgbColor != null || toLightParameters.RgbColor != null)
        {
            var fromRgbColor = GetRgbColor(fromLightParameters);
            var toRgbColor = GetRgbColor(toLightParameters);
            var blendedColor = fromRgbColor.BlendWithGammaCorrection(toRgbColor, progress);

            return result with
            {
                ColorTempKelvin = null,
                RgbColor = blendedColor
            };
        }

        if (fromLightParameters.ColorTempKelvin == null || toLightParameters.ColorTempKelvin == null)
        {
            throw new InvalidOperationException("Interpolation requires either RGB or ColorTempKelvin value");
        }

        var fromColorTemp = fromLightParameters.ColorTempKelvin.Value;
        var toColorTemp = toLightParameters.ColorTempKelvin.Value;
        // Note: This blending is done in Mired, which can be blended linearly. If we ever want to do this using Kelvin, this is no longer the case, and the blending should convert first.
        var blendedTemp = (int)Math.Round(fromColorTemp + (toColorTemp - fromColorTemp) * progress);

        return result with
        {
            ColorTempKelvin = blendedTemp
        };
    }

    /// <summary>
    /// Interpolates the brightness value between two brightness values based on the specified progress.
    /// </summary>
    /// <param name="from">The starting brightness value, or <c>null</c> if not specified.</param>
    /// <param name="to">The ending brightness value, or <c>null</c> if not specified.</param>
    /// <param name="progress">The interpolation progress between 0 and 1.</param>
    /// <returns>The interpolated brightness value, or <c>null</c> if both values are <c>null</c>.</returns>
    private static double? InterpolateBrightness(double? from, double? to, double progress)
    {
        if (from != null && to != null)
        {
            return Math.Round(from.Value + (to.Value - from.Value) * progress);
        }
        return from ?? to;
    }

    /// <summary>
    /// Retrieves the RGB color from the specified <see cref="LightParameters"/>.
    /// If an RGB color is directly specified, it is returned. Otherwise, the RGB color is derived from the color temperature.
    /// </summary>
    /// <param name="lightParameters">The light parameters from which to extract the RGB color.</param>
    /// <returns>The RGB color as a <see cref="Color"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown when neither RGB color nor color temperature is specified.</exception>
    private static Color GetRgbColor(LightParameters lightParameters)
    {
        if (lightParameters.RgbColor != null)
        {
            return lightParameters.RgbColor.Value;
        }
        if (lightParameters.ColorTempKelvin == null)
        {
            throw new ArgumentException("Rgb color conversion requires either RGB or ColorTempKelvin value");
        }

        return ColorConverter.KelvinToRgb(Convert.ToDouble(lightParameters.ColorTempKelvin));
    }
}