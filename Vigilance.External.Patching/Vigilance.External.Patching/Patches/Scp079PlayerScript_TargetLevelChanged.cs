using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Mirror;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.TargetLevelChanged))]
    public static class Scp079PlayerScript_TargetLevelChanged
    {
        public static bool Prefix(Scp079PlayerScript __instance, NetworkConnection conn, int newLvl)
        {
            try
            {
                Player player = PlayersList.GetPlayer(__instance.roles._hub);

                if (player == null)
                    return true;

                SCP079GainLvlEvent ev = new SCP079GainLvlEvent(player, newLvl, true);
                EventManager.Trigger<IHandlerScp079GainLvl>(ev);

                if (!ev.Allow)
                    return false;

                SendRpc(__instance, conn, ev.Level);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp079PlayerScript_TargetLevelChanged), e);
                return true;
            }
        }

        private static void SendRpc(Scp079PlayerScript script, NetworkConnection target, int lvl)
        {
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            writer.WritePackedInt32(lvl);
            script.SendTargetRPCInternal(target, typeof(Scp079PlayerScript), "TargetLevelChanged", writer, 0);
            NetworkWriterPool.Recycle(writer);
        }
    }
}
