using Harmony;
using UnityEngine;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
    public static class Intercom_UpdateText
    {
        public static bool Prefix(Intercom __instance)
        {
			if (!string.IsNullOrEmpty(__instance.CustomContent))
			{
				__instance.IntercomState = Intercom.State.Custom;
				__instance.Network_intercomText = __instance.CustomContent;
				return false;
			}
			
			if (__instance.Muted)
			{
				__instance.IntercomState = Intercom.State.Muted;
				API.Intercom.SetContent(__instance, Intercom.State.Muted, PluginManager.Config.IntercomMuted);
			}
			else if (Intercom.AdminSpeaking)
			{
				__instance.IntercomState = Intercom.State.AdminSpeaking;
				API.Intercom.SetContent(__instance, Intercom.State.AdminSpeaking, PluginManager.Config.IntercomAdmin);
			}
			else if (__instance.remainingCooldown > 0f)
			{
				int num = Mathf.CeilToInt(__instance.remainingCooldown);
				__instance.IntercomState = Intercom.State.Restarting;
				__instance.NetworkIntercomTime = (ushort)(num >= 0 ? num : 0);
				API.Intercom.SetContent(__instance, Intercom.State.Restarting, PluginManager.Config.IntercomRestart);
			}
			else if (__instance.Networkspeaker != null)
			{
				if (__instance.bypassSpeaking)
				{
					__instance.IntercomState = Intercom.State.TransmittingBypass;
					API.Intercom.SetContent(__instance, Intercom.State.TransmittingBypass, PluginManager.Config.IntercomBypass);
				}
				else
				{
					int num2 = Mathf.CeilToInt(__instance.speechRemainingTime);
					__instance.IntercomState = Intercom.State.Transmitting;
					__instance.NetworkIntercomTime = (ushort)(num2 >= 0 ? num2 : 0);
					API.Intercom.SetContent(__instance, Intercom.State.Transmitting, PluginManager.Config.IntercomTransmit);
				}
			}
			else
			{
				__instance.IntercomState = Intercom.State.Ready;
				API.Intercom.SetContent(__instance, Intercom.State.Ready, PluginManager.Config.IntercomReady);
			}

			if (Intercom.LastState != Intercom.AdminSpeaking)
			{
				Intercom.LastState = Intercom.AdminSpeaking;
				__instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
			}

			return false;
		}
    }
}
