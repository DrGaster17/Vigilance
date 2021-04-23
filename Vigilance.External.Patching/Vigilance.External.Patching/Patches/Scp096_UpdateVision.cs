using System;
using Vigilance.API;
using Harmony;
using PlayableScps;
using UnityEngine;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.UpdateVision))]
    public static class Scp096_UpdateVision
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
				if (__instance._flash.Enabled)
					return false;

				Vector3 vector = __instance.Hub.transform.TransformPoint(Scp096._headOffset);

				foreach (Player player in PlayersList.List)
				{
					if (player.ReferenceHub == __instance.Hub)
						continue;

					if (player.IsSCP)
						continue;

					if (player.Role == RoleType.Tutorial && !PluginManager.Config.CanTutorialTriggerScp096)
						continue;

					if (player.Role != RoleType.Spectator && Vector3.Dot((player.PlayerCamera.position - vector).normalized, __instance.Hub.PlayerCameraReference.forward) >= 0.1f)
                    {
						VisionInformation vision = VisionInformation.GetVisionInformation(player.ReferenceHub, vector, -0.1f, 60f, true, true, __instance.Hub.localCurrentRoomEffects);

						if (vision.IsLooking)
                        {
							float delay = vision.LookingAmount / 0.25f * (vision.Distance * 0.1f);

							if (!__instance.Calming)
								__instance.AddTarget(player.GameObject);

							if (__instance.CanEnrage && player.GameObject != null)
								__instance.PreWindup(delay);
                        }
                    }
				}

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp096_UpdateVision), e);
                return true;
            }
        }
    }
}
