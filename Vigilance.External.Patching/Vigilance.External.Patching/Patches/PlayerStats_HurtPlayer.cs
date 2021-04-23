using System;
using Vigilance.API;
using Vigilance.Custom.Items.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Dissonance.Integrations.MirrorIgnorance;
using Respawning;
using CustomPlayerEffects;
using PlayableScps;
using PlayableScps.Interfaces;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    public static class PlayerStats_HurtPlayer
    {
        public static bool Prefix(PlayerStats __instance, PlayerStats.HitInfo info, GameObject go, ref bool __result, bool noTeamDamage = false, bool IsValidDamage = true)
        {
			try
			{
				Player attacker = CalculateAttacker(__instance, info);
				Player target = PlayersList.GetPlayer(go);

				if (attacker == null || target == null)
					return true;

				PatchData.Hits.Add(info);

				if (info.Amount < 0f)
				{
					if (target == null)
					{
						info.Amount = Mathf.Abs(999999f);
					}
					else
					{
						info.Amount = target.ReferenceHub.playerStats != null ? Mathf.Abs(target.ReferenceHub.playerStats.Health + target.ReferenceHub.playerStats.syncArtificialHealth + 10f) : Mathf.Abs(999999f);
					}
				}

				if (target != null)
				{
					Burned effect = target.GetEffect<Burned>();

					if (effect != null && effect.Enabled)
						info.Amount *= effect.DamageMult;
				}

				if (info.Amount > 2.1474836E+09f)
					info.Amount = 2.1474836E+09f;

				if (info.GetDamageType().isWeapon && target.ReferenceHub.characterClassManager.IsAnyScp() && info.GetDamageType() != DamageTypes.MicroHid)
					info.Amount *= __instance.weaponManager.weapons[(int)__instance.weaponManager.curWeapon].scpDamageMultiplier;

				if (target.GodMode)
					return false;

				if (__instance.ccm.CurRole.team == Team.SCP && __instance.ccm.Classes.SafeGet(target.Role).team == Team.SCP && target != attacker)
					return false;

				if (target.ReferenceHub.characterClassManager.SpawnProtected && !__instance._allowSPDmg)
					return false;

				bool isTk = !noTeamDamage && info.IsPlayer && target.ReferenceHub != info.RHub && target.ReferenceHub.characterClassManager.Fraction == info.RHub.characterClassManager.Fraction;

				if (isTk)
					info.Amount *= PlayerStats.FriendlyFireFactor;

				if (info.GetDamageType() == DamageTypes.Recontainment)
				{
					Scp079RecontainEvent ev = new Scp079RecontainEvent(target, true);
					EventManager.Trigger<IHandlerScp079Recontain>(ev);

					if (!ev.Allow)
						return false;
				}

				float health = target.Health;

				PlayerHurtEvent hurtEvent = new PlayerHurtEvent(target, attacker, info, true);
				CustomWeapon.EventHandler.OnHurt(hurtEvent);
				EventManager.Trigger<IHandlerPlayerHurt>(hurtEvent);

				if (!hurtEvent.Allow)
					return false;

				info = hurtEvent.HitInfo;

				if (__instance.lastHitInfo.Attacker == "ARTIFICIALDEGEN")
				{
					target.ReferenceHub.playerStats.unsyncedArtificialHealth -= info.Amount;

					if (target.ReferenceHub.playerStats.unsyncedArtificialHealth < 0f)
						target.ReferenceHub.playerStats.unsyncedArtificialHealth = 0f;
				}
				else
				{
					if (target.ReferenceHub.playerStats.unsyncedArtificialHealth > 0f)
					{
						float num = info.Amount * target.ReferenceHub.playerStats.artificialNormalRatio;
						float num2 = info.Amount - num;
						target.ReferenceHub.playerStats.unsyncedArtificialHealth -= num;

						if (target.ReferenceHub.playerStats.unsyncedArtificialHealth < 0f)
						{
							num2 += Mathf.Abs(target.ReferenceHub.playerStats.unsyncedArtificialHealth);
							target.ReferenceHub.playerStats.unsyncedArtificialHealth = 0f;
						}

						target.ReferenceHub.playerStats.Health -= num2;

						if (target.Health > 0 && target.Health - num <= 0 && target.ReferenceHub.characterClassManager.CurRole.team != Team.SCP)
							__instance.TargetAchieve(target.Connection, "didntevenfeelthat");
					}
					else
					{
						target.ReferenceHub.playerStats.Health -= info.Amount;
					}

					if (target.ReferenceHub.playerStats.Health < 0f)
						target.ReferenceHub.playerStats.Health = 0f;

					target.ReferenceHub.playerStats.lastHitInfo = info;
				}

				IDamagable damagable = target.CurrentScp as IDamagable;

				if (damagable != null)
					damagable.OnDamage(info);

				if (target.Health < 1 && target.Role != RoleType.Spectator)
				{
					PlayerDieEvent dieEvent = new PlayerDieEvent(target, attacker, info, true);
					CustomItem.EventHandling.OnPlayerDie(dieEvent);
					EventManager.Trigger<IHandlerPlayerDie>(dieEvent);

					if (!dieEvent.Allow)
						return false;

					target.Roles.Add(target.Role);

					info = dieEvent.HitInfo;

					IImmortalScp immortalScp = target.CurrentScp as IImmortalScp;

					if (immortalScp != null && !immortalScp.OnDeath(info, __instance.gameObject))
						return false;

					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						Scp079Interactable.ZoneAndRoom otherRoom = target.ReferenceHub.scp079PlayerScript.GetOtherRoom();

						foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, __instance._filters))
						{
							foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
							{
								if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
									scp079PlayerScript.RpcGainExp(ExpGainType.KillAssist, target.Role);
							}
						}
					}

					if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60 && IsValidDamage)
						__instance.TargetAchieve(target.Connection, "wowreally");

					if (__instance.isLocalPlayer && info.PlayerId != target.PlayerId)
						RoundSummary.Kills++;

					if (target.Role == RoleType.Scp096)
					{
						Scp096 scp096 = target.CurrentScp as Scp096;

						if (scp096 != null && scp096.PlayerState == Scp096PlayerState.Enraging)
							__instance.TargetAchieve(target.Connection, "unvoluntaryragequit");
					}

					if (info.GetDamageType() == DamageTypes.Pocket)
						__instance.TargetAchieve(target.Connection, "newb");

					if (info.GetDamageType() == DamageTypes.Scp173)
						__instance.TargetAchieve(target.Connection, "firsttime");

					if (info.GetDamageType() == DamageTypes.Grenade && info.PlayerId == target.PlayerId)
						__instance.TargetAchieve(target.Connection, "iwanttobearocket");

					if (info.GetDamageType().isWeapon)
					{
						if (target.Role == RoleType.Scientist)
						{
							Item itemByID = target.ReferenceHub.inventory.GetItemByID(target.ReferenceHub.inventory.curItem);

							if (itemByID != null && itemByID.itemCategory == ItemCategory.Keycard && attacker.Role == RoleType.ClassD)
								__instance.TargetAchieve(attacker.Connection, "betrayal");
						}

						if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
						{
							__instance._killStreak = 0;
							__instance._killStreakTime = Time.realtimeSinceStartup;
						}


						if (attacker.ReferenceHub.weaponManager.GetShootPermission(target.ReferenceHub.characterClassManager, true))
							__instance._killStreak++;

						if (__instance._killStreak >= 5)
							__instance.TargetAchieve(attacker.Connection, "pewpew");

						if ((attacker.IsNTF || attacker.Team == Team.RSC) && target.Role == RoleType.ClassD)
							__instance.TargetStats(attacker.Connection, "dboys_killed", "justresources", 50);
					}

					if (attacker.IsSCP && target.GetComponent<MicroHID>().CurrentHidState != MicroHID.MicroHidState.Idle)
						__instance.TargetAchieve(attacker.Connection, "illpassthanks");

					if (attacker.Team == Team.RSC && target.IsSCP)
						__instance.TargetAchieve(attacker.Connection, "timetodoitmyself");

					if (info.IsPlayer && target.ReferenceHub == info.RHub)
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, $"{target.ReferenceHub.LoggedNameFromRefHub()} playing as {target.Role} committed suicide using {info.GetDamageName()}.", ServerLogs.ServerLogType.Suicide);
					else
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, $"{target.ReferenceHub.LoggedNameFromRefHub()} playing as {target.Role} has been killed by {attacker.Nick} using {info.GetDamageName()}{(info.IsPlayer ? $" playing as {attacker.Role}." : ".")}", isTk ? ServerLogs.ServerLogType.Teamkill : ServerLogs.ServerLogType.KillLog);

					if (info.GetDamageType().isScp || info.GetDamageType() == DamageTypes.Pocket)
						RoundSummary.kills_by_scp++;

					if (info.GetDamageType() == DamageTypes.Grenade)
						RoundSummary.kills_by_frag++;

					if (!__instance._pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
					{
						if (PluginManager.Config.ShouldDropInventory)
							target.DropAllItems();

						if (target.ReferenceHub.characterClassManager.Classes.CheckBounds(target.Role) && info.GetDamageType() != DamageTypes.RagdollLess)
							__instance.GetComponent<RagdollManager>().SpawnRagdoll(target.Position, target.Rotations, target.ReferenceHub.playerMovementSync.PlayerVelocity, (int)target.Role, info, target.ReferenceHub.characterClassManager.CurRole.team > Team.SCP, go.GetComponent<MirrorIgnorancePlayer>().PlayerId, target.ReferenceHub.nicknameSync.DisplayName, target.PlayerId);
					}
					else
					{
						target.ClearInventory();
					}

					target.ReferenceHub.characterClassManager.NetworkDeathPosition = target.Position;

					if (target.IsSCP)
					{
						if (target.Role == RoleType.Scp0492)
						{
							NineTailedFoxAnnouncer.CheckForZombies(go);
						}
						else
						{
							if (info.IsPlayer && PlayersList.GetPlayer(info.PlayerId) != null)
							{
								NineTailedFoxAnnouncer.AnnounceScpTermination(target.ReferenceHub.characterClassManager.CurRole, info, string.Empty);
							}
							else
							{
								DamageTypes.DamageType damageType = info.GetDamageType();

								if (damageType == DamageTypes.Tesla)
									NineTailedFoxAnnouncer.AnnounceScpTermination(target.ReferenceHub.characterClassManager.CurRole, info, "TESLA");

								if (damageType == DamageTypes.Nuke)
									NineTailedFoxAnnouncer.AnnounceScpTermination(target.ReferenceHub.characterClassManager.CurRole, info, "WARHEAD");

								if (damageType == DamageTypes.Decont)
									NineTailedFoxAnnouncer.AnnounceScpTermination(target.ReferenceHub.characterClassManager.CurRole, info, "DECONTAMINATION");

								if (target.Role != RoleType.Scp079)
									NineTailedFoxAnnouncer.AnnounceScpTermination(target.ReferenceHub.characterClassManager.CurRole, info, "UNKNOWN");
							}
						}
					}

					target.ReferenceHub.playerStats.SetHPAmount(100);
					target.ReferenceHub.characterClassManager.SetClassID(RoleType.Spectator);
				}
				else
				{
					Vector3 pos = Vector3.zero;
					float num3 = 40f;

					if (info.GetDamageType().isWeapon)
					{
						Player playerOfId = PlayersList.GetPlayer(info.PlayerId);

						if (playerOfId != null)
						{
							pos = go.transform.InverseTransformPoint(playerOfId.Position).normalized;
							num3 = 100f;
						}
					}

					if (info.GetDamageType() == DamageTypes.Pocket)
					{
						if (attacker.ReferenceHub.playerMovementSync.RealModelPosition.y > -1900f)
							attacker.ReferenceHub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					}

					__instance.TargetBloodEffect(target.Connection, pos, Mathf.Clamp01(info.Amount / num3));
				}

				RespawnTickets singleton = RespawnTickets.Singleton;
				Team team = target.ReferenceHub.characterClassManager.CurRole.team;
				byte b = (byte)team;

				if (b != 0)
				{
					if (b == 3)
					{
						Team team2 = attacker.ReferenceHub.characterClassManager.CurRole.team;

						if (team2 == Team.CDP && team2 == Team.CHI)
							singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count);
					}
				}

				if (target.Role != RoleType.Scp0492)
				{
					for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
					{
						float num5 = target.ReferenceHub.playerStats.maxHP * num4;

						if (health > num5 && target.ReferenceHub.playerStats.Health < num5)
							singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
					}
				}

				IDamagable damagable2 = target.CurrentScp as IDamagable;

				if (damagable2 != null)
					damagable2.OnDamage(info);

				if (!isTk || FriendlyFireConfig.PauseDetector || PermissionsHandler.IsPermitted(info.RHub.serverRoles.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
				{
					__result = true;
					return false;
				}

				if (FriendlyFireConfig.IgnoreClassDTeamkills && target.ReferenceHub.characterClassManager.CurRole.team == Team.CDP && info.RHub.characterClassManager.CurRole.team == Team.CDP)
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Respawn.RegisterKill())
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Window.RegisterKill())
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Life.RegisterKill())
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Round.RegisterKill())
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Respawn.RegisterDamage(info.Amount))
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Window.RegisterDamage(info.Amount))
				{
					__result = true;
					return false;
				}

				if (info.RHub.FriendlyFireHandler.Life.RegisterDamage(info.Amount))
				{
					__result = true;
					return false;
				}

				info.RHub.FriendlyFireHandler.Round.RegisterDamage(info.Amount);
				__result = true;

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(PlayerStats_HurtPlayer), e);
				return true;
			}
        }

        public static Player CalculateAttacker(PlayerStats stats, PlayerStats.HitInfo info) => info.GetDamageName() == "GRENADE" ? PlayersList.GetPlayer(info.RHub) : PlayersList.GetPlayer(stats.ccm._hub);
    }
}
