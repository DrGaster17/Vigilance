using System;
using System.Collections.Generic;
using System.Linq;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Respawning;
using Respawning.NamingRules;
using NorthwoodLib.Pools;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
    public static class RespawnManager_Spawn
    {
        public static bool Prefix(RespawnManager __instance)
        {
            try
            {
				if (!RespawnWaveGenerator.SpawnableTeams.TryGetValue(__instance.NextKnownTeam, out SpawnableTeam spawnableTeam) || __instance.NextKnownTeam == SpawnableTeamType.None)
				{
					Patcher.LogWarn(typeof(RespawnManager_Spawn), $"SpawnableTeam ({__instance.NextKnownTeam}) is undefined.");
					return false;
				}

				List<Player> players = PlayersList.List.Where(x => !x.IsInOverwatch && !x.IsAlive).ToList();

				if (__instance._prioritySpawn)
				{
					players = players.OrderBy(x => x.ReferenceHub.characterClassManager.DeathTime).ToList();
				}
				else
				{
					players.ShuffleList();
				}

				int tickets = RespawnTickets.Singleton.GetAvailableTickets(__instance.NextKnownTeam);

				if (tickets == 0)
				{
					tickets = 5;
					RespawnTickets.Singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, 5, true);
				}

				int spawnSize = Mathf.Min(tickets, spawnableTeam.MaxWaveSize);

				while (players.Count > spawnSize)
				{
					players.RemoveAt(players.Count - 1);
				}

				TeamRespawnEvent ev = new TeamRespawnEvent(players, __instance.NextKnownTeam, true);
				EventManager.Trigger<IHandlerTeamRespawn>(ev);

				if (!ev.Allow)
					return false;

				players = ev.Players;

				List<Player> newList = ListPool<Player>.Shared.Rent();

				foreach (Player player in players)
				{
					RoleType classid = spawnableTeam.ClassQueue[Mathf.Min(newList.Count, spawnableTeam.ClassQueue.Length - 1)];
					player.ReferenceHub.characterClassManager.SetPlayersClass(classid, player.ReferenceHub.gameObject, false, false);
					newList.Add(player);
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, $"Player {player.Nick} ({player.UserId}) respawned as {classid}.", ServerLogs.ServerLogType.GameEvent);
				}

				if (newList.Count > 0)
				{
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, $"RespawnManager has succesfully respawned {newList.Count} players as {__instance.NextKnownTeam}.", ServerLogs.ServerLogType.GameEvent);
					RespawnTickets.Singleton.GrantTickets(__instance.NextKnownTeam, -newList.Count * spawnableTeam.TicketRespawnCost, false);

					if (UnitNamingRules.TryGetNamingRule(__instance.NextKnownTeam, out UnitNamingRule unitNamingRule))
					{
						unitNamingRule.GenerateNew(__instance.NextKnownTeam, out string text);

						foreach (Player player in newList)
						{
							player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = (byte)__instance.NextKnownTeam;
							player.ReferenceHub.characterClassManager.NetworkCurUnitName = text;
						}

						unitNamingRule.PlayEntranceAnnouncement(text);
					}

					RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, __instance.NextKnownTeam);
				}

				ListPool<Player>.Shared.Return(newList);

				__instance.NextKnownTeam = SpawnableTeamType.None;

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(RespawnManager_Spawn), e);
                return true;
            }
        }
    }
}
