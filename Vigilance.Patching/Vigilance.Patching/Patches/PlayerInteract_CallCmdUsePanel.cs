using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUsePanel))]
    public static class PlayerInteract_CallCmdUsePanel
    {
        public static bool Prefix(PlayerInteract __instance, PlayerInteract.AlphaPanelOperations n)
        {
            try
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUsePanel), $"Received a command from client ({__instance._hub.nicknameSync.MyNick} - {__instance._hub.characterClassManager.UserId}): {n}");

				if (!__instance._playerInteractRateLimit.CanExecute(true) || __instance._hc.CufferId > 0 
					|| (__instance._hc.ForceCuff && !PlayerInteract.CanDisarmedInteract)
					|| !__instance.ChckDis(AlphaWarhead.NukesitePanel.transform.position))
				{
					return false;
				}

				Player player = PlayersList.GetPlayer(__instance._hub);

				if (player == null)
					return true;

				if (n == PlayerInteract.AlphaPanelOperations.Cancel)
				{
					__instance.OnInteract();

					AlphaWarheadController.Host.CancelDetonation(__instance.gameObject);

					ServerLogs.AddLog(ServerLogs.Modules.Warhead, player.ReferenceHub.LoggedNameFromRefHub() + " cancelled the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent, false);

					return false;
				}

				if (n == PlayerInteract.AlphaPanelOperations.Lever)
				{
					if (!AlphaWarhead.NukesitePanel.AllowChangeLevelState())
						return false;

					WarheadLeverSwitchEvent ev = new WarheadLeverSwitchEvent(player, !AlphaWarhead.NukesitePanel.enabled, AlphaWarhead.NukesitePanel.enabled, true);

					EventManager.Trigger<IHandlerWarheadLeverSwitch>(ev);

					if (!ev.Allow)
						return false;

					AlphaWarhead.NukesitePanel.Networkenabled = ev.NewState;

					__instance.OnInteract();
					__instance.RpcLeverSound();

					ServerLogs.AddLog(ServerLogs.Modules.Warhead, $"{player.ReferenceHub.LoggedNameFromRefHub()} set the Alpha Warhead status to {ev.NewState.ToString().ToLower()}", ServerLogs.ServerLogType.GameEvent);

					return false;
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(PlayerInteract_CallCmdUsePanel), e);
                return true;
            }
        }
    }
}
