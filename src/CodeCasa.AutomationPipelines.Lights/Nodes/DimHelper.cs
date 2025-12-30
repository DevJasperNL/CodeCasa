using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class DimHelper(
    ILight subject,
    IEnumerable<ILight> lightsInDimOrder,
    int minBrightness,
    int brightnessStep)
{
    private readonly ILight[] _lightsInDimOrder = lightsInDimOrder.ToArray();

    public LightTransition? DimStep()
    {
        var lightToTurnOff = ShouldTurnOffToDim();
        if (lightToTurnOff?.Id == subject.Id)
        {
            return LightTransition.Off();
        }
        if (lightToTurnOff != null)
        {
            // Another node should act. We shouldn't do anything.
            return null;
        }

        var parameters = subject.GetParameters();
        var oldBrightness = parameters.Brightness ?? 0;
        if (oldBrightness == 0)
        {
            return null;
        }
        var newBrightness = Math.Max(minBrightness, oldBrightness - brightnessStep);
        if (newBrightness == oldBrightness)
        {
            return null;
        }

        return (parameters with { Brightness = newBrightness }).AsTransition();
    }

    public LightTransition? BrightenStep()
    {
        var lightToTurnOn = ShouldTurnOnToBrighten();
        if (lightToTurnOn?.Id == subject.Id)
        {
            return (subject.GetParameters() with { Brightness = minBrightness }).AsTransition();
        }
        if (lightToTurnOn != null)
        {
            // Another node should act. We shouldn't do anything.
            return null;
        }

        var parameters = subject.GetParameters();
        var oldBrightness = parameters.Brightness ?? 0;
        if (oldBrightness == 0)
        {
            return null;
        }
        var newBrightness = Math.Min(255, oldBrightness + brightnessStep);
        if (newBrightness == oldBrightness)
        {
            return null;
        }

        return (parameters with { Brightness = newBrightness }).AsTransition();
    }

    private ILight? ShouldTurnOffToDim()
    {
        // todo: filter on availability
        ILight? lightToTurnOff = null;
        foreach (var light in _lightsInDimOrder)
        {
            var brightness = light.GetParameters().Brightness ?? 0;
            if (brightness == 0)
            {
                continue;
            }
            if (brightness > minBrightness)
            {
                // If any light is brighter than MinBrightness, we don't turn anything off, we need to dim.
                return null;
            }

            lightToTurnOff ??= light;
        }

        return lightToTurnOff;
    }

    private ILight? ShouldTurnOnToBrighten()
    {
        Console.WriteLine($"What node for {subject.Id} sees:");
        foreach (var light in _lightsInDimOrder.Reverse())
        {
            Console.WriteLine($"{light.Id}: {light.GetParameters()}");
        }

        // This method is a bit more specific: it will only return true if lights are turned on in the correct order. If not, we want to keep the lights that are off, off.
        ILight? lightToTurnOn = null;
        foreach (var light in _lightsInDimOrder.Reverse())
        {
            var brightness = light.GetParameters().Brightness ?? 0;
            if (brightness >= minBrightness) // On
            {
                if (lightToTurnOn != null || brightness > minBrightness)
                {
                    return null;
                }
                continue;
            }

            lightToTurnOn ??= light;
        }

        return lightToTurnOn;
    }
}