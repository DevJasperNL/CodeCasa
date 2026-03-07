using CodeCasa.Lights.NetDaemon.Generated;

namespace CodeCasa.Lights.NetDaemon.Extensions;

internal static class LightTransitionExtensions
{
    public static LightTurnOnParameters ToLightTurnOnParameters(this LightTransition lightTransition)
    {
        return new LightTurnOnParameters
        {
            Transition = lightTransition.TransitionTime?.TotalSeconds,
            Brightness = lightTransition.LightParameters.Brightness,
            RgbColor = lightTransition.LightParameters.RgbColor?.ToRgbCollection(),
            ColorTempKelvin = lightTransition.LightParameters.ColorTempKelvin
        };
    }

    public static LightTurnOffParameters ToLightTurnOffParameters(this LightTransition lightTransition)
    {
        return new LightTurnOffParameters
        {
            Transition = lightTransition.TransitionTime?.TotalSeconds
        };
    }
}