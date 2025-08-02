using CodeCasa.Shared.Extensions;

namespace CodeCasa.CustomEntities.People;

internal static class PersonStateMappings
{// todo: use type-safe entities.
    public static readonly Dictionary<PersonStates, string> PeopleStatesToStateValues = new()
    {
        { PersonStates.Awake , "Awake" },
        { PersonStates.Asleep , "Asleep" },
        { PersonStates.Away , "Away" },
    };
    public static readonly Dictionary<string, PersonStates> StateValuesToPeopleStates = PeopleStatesToStateValues.Inverse(StringComparer.OrdinalIgnoreCase);

    public static readonly Dictionary<PersonEntityStates, string> PeopleEntityStatesToStateValues = new()
    {
        { PersonEntityStates.Home , "home" },
        { PersonEntityStates.NotHome , "not_home" }
    };
    public static readonly Dictionary<string, PersonEntityStates> StateValuesToPeopleEntityStates = PeopleEntityStatesToStateValues.Inverse(StringComparer.OrdinalIgnoreCase);
}