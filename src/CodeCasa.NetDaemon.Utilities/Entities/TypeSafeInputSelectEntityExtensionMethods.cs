using CodeCasa.NetDaemon.Utilities.Entities;

namespace CodeCasa.CustomEntities.InputSelect;

public static class TypeSafeInputSelectEntityExtensionMethods
{
    ///<summary>Selects an option.</summary>
    public static void SelectOption<TEnum>(this TypeSafeInputSelectEntity<TEnum> target, TEnum option) where TEnum : struct, Enum
    {
        target.CallService("select_option", new {option = target.ConvertEnumToState(option)});
    }
}