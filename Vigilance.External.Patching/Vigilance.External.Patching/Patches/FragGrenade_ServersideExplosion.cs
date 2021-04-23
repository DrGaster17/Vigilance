using System;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Custom.Items.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Grenades;
using GameCore;
using Interactables.Interobjects.DoorUtils;
using CustomPlayerEffects;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(FragGrenade), nameof(FragGrenade.ServersideExplosion))]
    public static class FragGrenade_ServersideExplosion
    {
        public static bool Prefix(FragGrenade __instance, ref bool __result)
        {
            try
            {
				GrenadeExplodeEvent ev = new GrenadeExplodeEvent(PlayersList.GetPlayer(__instance.thrower.hub), __instance, GrenadeType.Frag, true);
				CustomGrenade.Handler.OnExplode(ev);
				EventManager.Trigger<IHandlerGrenadeExplode>(ev);

				if (!ev.Allow)
					return false;

				bool result = PatchData.ServersideExplosion(__instance);

				Vector3 position = __instance.transform.position;

				int num = 0;

				foreach (Collider collider in Physics.OverlapSphere(position, __instance.chainTriggerRadius, __instance.damageLayerMask))
				{
					BreakableWindow component = collider.GetComponent<BreakableWindow>();

					if (component != null)
					{
						if ((component.transform.position - position).sqrMagnitude <= __instance.sqrChainTriggerRadius)
						{
							component.ServerDamageWindow(500f);
						}
					}
					else
					{
						DoorVariant door = collider.GetComponentInParent<DoorVariant>();

						if (door is IDamageableDoor damageable && damageable != null)
						{
							damageable.ServerDamage(__instance.damageOverDistance.Evaluate(Vector3.Distance(position, door.transform.position)), DoorDamageType.Grenade);
						}
						else if ((__instance.chainLengthLimit == -1 || __instance.chainLengthLimit > __instance.currentChainLength) && (__instance.chainConcurrencyLimit == -1 || __instance.chainConcurrencyLimit > num))
						{
							Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();

							if (componentInChildren != null && __instance.ChangeIntoGrenade(componentInChildren))
							{
								num++;
							}
						}
					}
				}

				foreach (KeyValuePair<GameObject, ReferenceHub> keyValuePair in ReferenceHub.GetAllHubs())
				{
					if (ServerConsole.FriendlyFire || !(keyValuePair.Key != __instance.thrower.gameObject) || (keyValuePair.Value.weaponManager.GetShootPermission(__instance.throwerTeam, false) && keyValuePair.Value.weaponManager.GetShootPermission(__instance.TeamWhenThrown, false)))
					{
						PlayerStats playerStats = keyValuePair.Value.playerStats;

						if (!(playerStats == null) && playerStats.ccm.InWorld)
						{
							float num2 = __instance.damageOverDistance.Evaluate(Vector3.Distance(position, playerStats.transform.position)) * (playerStats.ccm.IsHuman() ? ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f) : ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f));

							if (num2 > __instance.absoluteDamageFalloff)
							{
								foreach (Transform transform in playerStats.grenadePoints)
								{
									if (!Physics.Linecast(position, transform.position, __instance.hurtLayerMask))
									{
										playerStats.HurtPlayer(new PlayerStats.HitInfo(num2, (__instance.thrower != null) ? __instance.thrower.hub.LoggedNameFromRefHub() : "(UNKNOWN)", DamageTypes.Grenade, __instance.thrower.hub.queryProcessor.PlayerId), keyValuePair.Key, false, true);
										break;
									}
								}

								if (!playerStats.ccm.IsAnyScp())
								{
									float duration = __instance.statusDurationOverDistance.Evaluate(Vector3.Distance(position, playerStats.transform.position));

									if (PluginManager.Config.FragGrenadeBurnedEffect)
										keyValuePair.Value.playerEffectsController.EnableEffect(keyValuePair.Value.playerEffectsController.GetEffect<Burned>(), duration, false);

									if (PluginManager.Config.FragGrenadeConcussedEffect)
										keyValuePair.Value.playerEffectsController.EnableEffect(keyValuePair.Value.playerEffectsController.GetEffect<Concussed>(), duration, false);
								}
							}
						}
					}
				}

				__result = result;
				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(FragGrenade_ServersideExplosion), e);
                __result = false;
                return true;
            }
        }
    }
}
