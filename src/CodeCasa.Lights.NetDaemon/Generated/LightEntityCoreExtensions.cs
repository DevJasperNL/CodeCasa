using NetDaemon.HassModel.Entities;

namespace CodeCasa.Lights.NetDaemon.Generated
{
    internal static class LightEntityCoreExtensions
    {
        ///<summary>Turns on one or more lights and adjusts their properties, even when they are turned on already.</summary>
        public static void TurnOn(this ILightEntityCore target, LightTurnOnParameters data)
        {
            target.CallService("turn_on", data);
        }

        ///<summary>Turns off one or more lights.</summary>
        public static void TurnOff(this ILightEntityCore target, LightTurnOffParameters data)
        {
            target.CallService("turn_off", data);
        }
    }
}
