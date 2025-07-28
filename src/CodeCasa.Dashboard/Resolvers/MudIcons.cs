using System.Reflection;

namespace CodeCasa.Dashboard.Icons
{
    public class MudIcons
    {
        public static string? TryResolveMudIcon(string path)
        {
            var currentType = typeof(MudBlazor.Icons);

            var parts = path.Split('.');
            if (!parts.Any())
            {
                return null;
            }

            var startIndex = parts.First() == "Icons" ? 1 : 0;
            for (var i = startIndex; i < parts.Length; i++)
            {
                var part = parts[i];

                if (i == parts.Length - 1)
                {
                    var field = currentType.GetField(part, BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        return (string?)field.GetValue(null);
                    }
                    return null;
                }

                var nested = currentType.GetNestedType(part, BindingFlags.Public | BindingFlags.Static);
                if (nested == null)
                {
                    return null;
                }
                currentType = nested;
            }

            return null;
        }
    }
}
