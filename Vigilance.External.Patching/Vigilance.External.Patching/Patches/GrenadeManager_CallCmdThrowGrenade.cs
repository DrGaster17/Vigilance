using System;

using Vigilance.API;
using Vigilance.Custom.Items.API;

using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;

using UnityEngine;
using Harmony;
using Mirror;
using Grenades;
using MEC;

namespace Vigilance.External.Patching.Patches
{
	[HarmonyPatch(typeof(GrenadeManager), nameof(GrenadeManager.CallCmdThrowGrenade))]
	public static class GrenadeManager_CallCmdThrowGrenade
	{
		public static bool Prefix(GrenadeManager __instance, int id, bool slowThrow, double time)
		{
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true) || id < 0 || __instance.availableGrenades.Length <= id)
					return false;

				GrenadeSettings grenadeSettings = __instance.availableGrenades[id];

				if (__instance.hub.inventory.curItem != grenadeSettings.inventoryID)
					return false;

				Player player = PlayersList.GetPlayer(__instance.hub);

				if (player == null)
					return true;

				float delay = Mathf.Clamp((float)(time - NetworkTime.time), 0f, grenadeSettings.throwAnimationDuration);
				float forceMultiplier = slowThrow ? 0.5f : 1f;

				ThrowGrenadeEvent ev = new ThrowGrenadeEvent(player, grenadeSettings, forceMultiplier, delay, player.CurrentItemIndex, true);
				CustomGrenade.Handler.OnThrow(ev);
				EventManager.Trigger<IHandlerThrowGrenade>(ev);

				if (!ev.Allow)
					return false;

				Timing.RunCoroutine(__instance._ServerThrowGrenade(ev.Settings, ev.ForceMultiplier, ev.ItemIndex, ev.Delay), Segment.FixedUpdate);

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(GrenadeManager_CallCmdThrowGrenade), e);
				return true;
			}
		}
	}
}