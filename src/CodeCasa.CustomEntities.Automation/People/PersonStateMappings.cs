using CodeCasa.Shared.Extensions;

namespace CodeCasa.CustomEntities.Automation.People;

internal static class PersonStateMappings
{// todo: use type-safe entities.
    public static readonly Dictionary<PersonEntityStates, string> PeopleEntityStatesToStateValues = new()
    {
        { PersonEntityStates.Home , "home" },
        { PersonEntityStates.NotHome , "not_home" }
    };
    public static readonly Dictionary<string, PersonEntityStates> StateValuesToPeopleEntityStates = PeopleEntityStatesToStateValues.Inverse(StringComparer.OrdinalIgnoreCase);
}