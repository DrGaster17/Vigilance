using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]

    public static class CharacterClassManager_AllowContain
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {

                for (int i = 0; i < PlayersList.List.Count; i++)
                {
                    Player player = PlayersList.List[i];

                    if (!player.ReferenceHub.isDedicatedServer && player.ReferenceHub.Ready 
                        && Vector3.Distance(player.Position, MapCache.LureSubjectContainer.transform.position) < 1.97f && !player.PlayerLock)
                    {
                        FemurEnterEvent ev = new FemurEnterEvent(player, !player.IsSCP && player.Role != RoleType.Spectator && !player.GodMode);
                        EventManager.Trigger<IHandlerFemurEnter>(ev);

                        if (ev.Allow)
                        {
                            player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), player.ReferenceHub.gameObject, true, true);
                            __instance._lureSpj.SetState(true);
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CharacterClassManager_AllowContain), e);
                return true;
            }
        }
    }
}
