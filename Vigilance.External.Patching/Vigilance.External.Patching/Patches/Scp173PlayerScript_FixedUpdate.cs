using System;
using Harmony;
using Mirror;
using Vigilance.API;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.FixedUpdate))]
    public static class Scp173PlayerScript_FixedUpdate
    {
        public static bool Prefix(Scp173PlayerScript __instance)
        {
            try
            {
                __instance.DoBlinkingSequence();

                if (!__instance.iAm173 || (!__instance.isLocalPlayer && !NetworkServer.active))
                    return false;

                __instance.AllowMove = true;

                foreach (Player player in PlayersList.List)
                {
                    ReferenceHub hub = player.ReferenceHub;

                    if (hub.characterClassManager.CurRole.roleId == RoleType.Tutorial && !PluginManager.Config.CanTutorialBlockScp173)
                        continue;

                    if (!hub.characterClassManager.Scp173.SameClass && hub.characterClassManager.Scp173.LookFor173(__instance.gameObject, true) && __instance.LookFor173(hub.gameObject, false))
                    {
                        __instance.AllowMove = false;
                        return false;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp173PlayerScript_FixedUpdate), e);
                return true;
            }
        }
    }
}
