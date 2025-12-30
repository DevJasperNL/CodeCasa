using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights
{
    internal static class CompositeHelper
    {
        public static string[] ValidateLightsSupported(IEnumerable<string> lightIds, IEnumerable<string> supportedLightIds)
        {
            var supportedLightIdsArray = supportedLightIds.ToArray();
            var lightIdsArray = lightIds.ToArray();
            if (!lightIdsArray.Any())
            {
                throw new ArgumentException("At least one id should be provided.", nameof(lightIdsArray));
            }

            var unsupportedLightIds = lightIdsArray
                .Where(id => !supportedLightIdsArray.Contains(id))
                .ToArray();

            if (unsupportedLightIds.Any())
            {
                throw new InvalidOperationException(
                    $"The following lights are not supported: {string.Join(", ", unsupportedLightIds)}.");
            }

            return lightIdsArray.ToArray();
        }

        public static void ValidateLightSupported(IEnumerable<string> lights, string supportedLightId)
        {
            var lightsArray = lights.ToArray();
            if (!lightsArray.Any())
            {
                throw new ArgumentException("At least one id should be provided.", nameof(lightsArray));
            }

            var unsupportedLightIds = lightsArray
                .Where(id => id != supportedLightId)
                .ToArray();

            if (unsupportedLightIds.Any())
            {
                throw new InvalidOperationException(
                    $"The following lights are not supported: {string.Join(", ", unsupportedLightIds)}.");
            }
        }

        public static string[] ResolveGroupsAndValidateLightsSupported(IEnumerable<ILight> lights, IEnumerable<string> supportedLightIds)
        {
            return ValidateLightsSupported(lights.SelectMany(le => le.Flatten()).Select(l => l.Id).Distinct(), supportedLightIds);
        }


        public static void ResolveGroupsAndValidateLightSupported(IEnumerable<ILight> lights, string supportedLightId)
        {
            ValidateLightSupported(lights.SelectMany(le => le.Flatten()).Select(l => l.Id).Distinct(), supportedLightId);
        }
    }
}
