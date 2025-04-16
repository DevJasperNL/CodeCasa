﻿using CodeCasa.AutoGenerated;
using CodeCasa.AutoGenerated.Extensions;

namespace CodeCasa.NetDaemon.Utilities.Extensions;

public static class LightEntityExtensions
{
    private const int RelaxTemp = 500;

    public static void ExecuteLightParameters(this LightEntity lightEntity, LightParameters lightParameters)
    {
        if (lightParameters.Brightness == 0)
        {
            lightEntity.TurnOff(lightParameters.ToLightTurnOffParameters());
            return;
        }

        lightEntity.TurnOn(lightParameters.ToLightTurnOnParameters());
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the Relax scene of any given light entity.
    /// </summary>
    public static LightParameters GetRelaxSceneParameters(this LightEntity lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(RelaxTemp, 142) ??
               lightEntity.TryGetBrightnessParameters(142) ?? // Note: This will also turn on white lights that only support brightness, but we don't have those.
               LightParameters.Off();
    }

    private static LightParameters? TryGetColorTempParameters(this LightEntity lightEntity, int desiredTemp, int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.ColorTemp))
        {
            return null;
        }
        var colorTemp = lightEntity.GetColorTempWithinRange(desiredTemp);
        if (colorTemp == null)
        {
            return null;
        }

        return new() { ColorTemp = colorTemp, Brightness = brightness };
    }

    public static bool SupportsColorMode(this LightEntity lightEntity, string colorMode)
    {
        var attributes = lightEntity.Attributes;
        if (attributes?.SupportedColorModes == null)
        {
            return false;
        }

        return attributes.SupportedColorModes.Any(mode =>
            string.Equals(mode, colorMode, StringComparison.OrdinalIgnoreCase));
    }

    private static LightParameters? TryGetBrightnessParameters(this LightEntity lightEntity, int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.Brightness))
        {
            return null;
        }
        return new() { Brightness = brightness };
    }

    public static int? GetColorTempWithinRange(this LightEntity lightEntity, int desiredMireds)
    {
        var attributes = lightEntity.Attributes;
        if (attributes == null)
        {
            return null;
        }
        if (attributes.MinMireds == null || attributes.MaxMireds == null)
        {
            return null;
        }

        return Math.Min((int)attributes.MaxMireds, Math.Max((int)attributes.MinMireds, desiredMireds));
    }
}