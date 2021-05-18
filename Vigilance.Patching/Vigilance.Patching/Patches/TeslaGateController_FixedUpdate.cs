using System;
using Harmony;

using Vigilance.API;

using Vigilance.Extensions;

using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.EventSystem;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(TeslaGateController), nameof(TeslaGateController.FixedUpdate))]
    public static class TeslaGateController_FixedUpdate
    {
        public static bool Prefix(TeslaGateController __instance)
        {
            try
            {
                foreach (TeslaGate teslaGate in __instance.TeslaGates)
                {
                    Tesla tesla = teslaGate.GetTesla();

                    if (tesla == null)
                        continue;

                    foreach (Player player in PlayersList.List)
                    {
                        if (Tesla.GatesDisabled || tesla.IsDisabled || !player.IsAlive)
                            continue;

                        if ((player.GodMode && PluginManager.Config.TeslaIgnoreGodMode) || (player.BypassMode && PluginManager.Config.TeslaIgnoreBypassMode) 
                            || (player.ReferenceHub.characterClassManager.NetworkNoclipEnabled && PluginManager.Config.TeslaIgnoreNoClip))
                            continue;

                        if (!PluginManager.Config.TeslaTriggerableRoles.Contains(player.Role))
                            continue;

                        if (!teslaGate.PlayerInRange(player.ReferenceHub) || teslaGate.InProgress)
                            continue;

                        TriggerTeslaEvent ev = new TriggerTeslaEvent(player, tesla, true);
                        EventManager.Trigger<IHandlerTriggerTesla>(ev);

                        if (!ev.Allow)
                            continue;

                        teslaGate.ServerSideCode();
                    }

                    teslaGate.ClientSideCode();
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(TeslaGateController_FixedUpdate), e);
                return true;
            }
        }
    }

}