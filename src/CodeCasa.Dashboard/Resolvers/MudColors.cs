using MudBlazor;

namespace CodeCasa.Dashboard.Resolvers
{
    public static class MudColors
    {
        public static Color? TryResolveMudColor(string colorName)
        {
            if (string.IsNullOrWhiteSpace(colorName))
                return null;

            // Normalize input
            colorName = colorName.Trim().ToLowerInvariant();

            if (Enum.TryParse(typeof(Color), colorName, ignoreCase: true, out var result))
            {
                return (Color)result;
            }

            return null;
        }
    }
}
