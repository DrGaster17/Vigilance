using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using MEC;
using Assets._Scripts.Dissonance;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Intercom), nameof(Intercom.RequestTransmission))]
    public static class Intercom_RequestTransmission
    {
        public static bool Prefix(Intercom __instance, GameObject spk)
        {
            try
            {
                if (spk == null)
                {
                    __instance.Networkspeaker = null;
                    return false;
                }

                Player speaker = PlayersList.GetPlayer(spk);

                if (speaker == null)
                    return true;

				if (speaker.PlayerLock)
					return false;

                if ((__instance.remainingCooldown <= 0f && !__instance._inUse) || (speaker.BypassMode && !__instance.speaking))
                {
                    __instance.speaking = true;
                    __instance.remainingCooldown = -1f;
                    __instance._inUse = true;

					IntercomSpeakEvent ev = new IntercomSpeakEvent(speaker, true);
					EventManager.Trigger<IHandlerIntercomSpeak>(ev);

					if (!ev.Allow)
						return false;

                    Timing.RunCoroutine(_CustomProcess(__instance, speaker), Segment.FixedUpdate);
                }
                
                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Intercom_RequestTransmission), e);
                return true;
            }
        }

		public static bool AllowToSpeak(Player player, Intercom intercom, DissonanceUserSetup setup)
        {
			try
			{
				IntercomCheckSpeakAllowedEvent ev = new IntercomCheckSpeakAllowedEvent(player, setup,
					Vector3.Distance(player.Position, intercom._area.position) < intercom.triggerDistance && !player.IsSCP && player.IsAlive);
				EventManager.Trigger<IHandlerIntercomCheckAllowedSpeak>(ev);
				return ev.Allow;
			}
			catch (Exception e)
            {
				// If an exception occurs revert to base-game's check
				Patcher.Log(typeof(Intercom_RequestTransmission), e);
				return Vector3.Distance(player.Position, intercom._area.position) < intercom.triggerDistance && !player.IsSCP && player.IsAlive;
			}
        }

		// Use a custom process to override Intercom's speak check
        private static IEnumerator<float> _CustomProcess(Intercom instance, Player speaker)
        {
			// whatever this does
			if (!instance._intercomSupported)
				yield break;

			instance.speaking = true;

			if (speaker.IsIntercomMuted || speaker.IsMuted || MuteHandler.QueryPersistentMute(speaker.UserId))
			{
				instance.Muted = true;
				instance.remainingCooldown = 3f;

				while (instance.remainingCooldown >= 0f)
				{
					instance.remainingCooldown -= Time.deltaTime;
					yield return float.NegativeInfinity;
				}

				instance.Muted = false;
				instance.speaking = false;
				instance._inUse = false;

				yield break;
			}

            instance.RpcPlaySound(true, speaker.PlayerId);

            for (byte i = 0; i < 100; i++)
				yield return 0f;

			instance.Networkspeaker = speaker.GameObject;

			DissonanceUserSetup uservc = speaker.GameObject.GetComponentInChildren<DissonanceUserSetup>();
			Intercom userIcom = speaker.GetComponent<Intercom>();

			if (uservc != null)
				uservc.IntercomAsHuman = true;

			bool wasAdmin = Intercom.AdminSpeaking;

			if (Intercom.AdminSpeaking)
			{
				while (instance.Networkspeaker != null)
				{
					yield return float.NegativeInfinity;
				}
			}
			else if (speaker.BypassMode)
			{
				instance.bypassSpeaking = true;
				while (instance.Networkspeaker != null)
				{
					if (!AllowToSpeak(speaker, userIcom, uservc))
						break;

					yield return float.NegativeInfinity;
				}
			}
			else
			{
				instance.speechRemainingTime = instance._speechTime;
				instance.bypassSpeaking = false;

				while (instance.speechRemainingTime > 0f && instance.Networkspeaker != null && speaker.GetComponent<Intercom>().ServerAllowToSpeak())
				{
					instance.speechRemainingTime -= Timing.DeltaTime;
					yield return float.NegativeInfinity;
				}
			}

			if (instance.isLocalPlayer && uservc != null)
				uservc.IntercomAsHuman = false;

			instance.Networkspeaker = null;
			instance.RpcPlaySound(false, 0);
			instance.speaking = false;

			if (!wasAdmin)
			{
				instance.remainingCooldown = instance._cooldownAfter;
				while (instance.remainingCooldown >= 0f)
				{
					instance.remainingCooldown -= Time.deltaTime;
					yield return float.NegativeInfinity;
				}
			}

			if (!instance.speaking)
				instance._inUse = false;

			yield break;
		}
    }
}
