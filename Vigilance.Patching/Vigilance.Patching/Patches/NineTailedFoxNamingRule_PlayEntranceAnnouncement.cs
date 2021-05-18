using System;
using System.Linq;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Harmony;
using Respawning.NamingRules;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(NineTailedFoxNamingRule), nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement))]
    public static class NineTailedFoxNamingRule_PlayEntranceAnnouncement
    {
        public static bool Prefix(NineTailedFoxNamingRule __instance, ref string regular)
        {
            try
            {
                int scpsLeft = PlayersList.GetPlayers(Team.SCP).Where(x => x.Role != RoleType.Scp0492).Count();

                string[] unitInformations = regular.Split('-');

                AnnounceNTFEntranceEvent ev = new AnnounceNTFEntranceEvent(scpsLeft, unitInformations[0], int.Parse(unitInformations[1]), true);
                EventManager.Trigger<IHandlerAnnounceNtfEntrance>(ev);

                regular = $"{ev.Unit}-{ev.Number}";

                return ev.Allow;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(NineTailedFoxNamingRule_PlayEntranceAnnouncement), e);
                return true;
            }
        }
    }
}
