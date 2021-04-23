using System;
using System.Collections.Generic;
using System.Threading;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Extensions;
using UnityEngine;
using Harmony;
using Cryptography;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.CallCmdReport))]
    public static class CheaterReport_CallCmdReport
    {
        public static bool Prefix(CheaterReport __instance, int playerId, string reason, byte[] signature, bool notifyGm)
        {
            try
            {
				if (!__instance._commandRateLimit.CanExecute(true) || string.IsNullOrEmpty(reason))
					return false;

				Player player = playerId.GetPlayer();
				Player myPlayer = __instance.gameObject.GetPlayer();
				float num = Time.time - __instance._lastReport;

				if (player == null || myPlayer == null)
					return true;

				GameConsoleTransmission gct = player.GetComponent<GameConsoleTransmission>();

				if (player == myPlayer)
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] You can't report yourself.", "red");
					return false;
				}

				if (num < 2f)
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded.", "red");
					return false;
				}

				if (num > 60f)
					__instance._reportedPlayersAmount = 0;

				if (__instance._reportedPlayersAmount > 5)
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded.", "red");
					return false;
				}

				if (notifyGm && (!ServerStatic.PermissionsHandler.IsVerified || string.IsNullOrEmpty(ServerConsole.Password)))
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Server is not verified - you can't use report feature on this server.", "red");
					return false;
				}

				if (__instance._reportedPlayers == null)
					__instance._reportedPlayers = new HashSet<int>();

				if (__instance._reportedPlayers.Contains(playerId))
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] You have already reported that player.", "red");
					return false;
				}

				string reporterNickname = myPlayer.Nick;
				string reportedNickname = player.Nick;

				if (!notifyGm)
				{
					LocalReportEvent ev = new LocalReportEvent(reason, myPlayer, player, true);
					EventManager.Trigger<IHandlerLocalReport>(ev);

					if (!ev.Allow)
                    {
						gct.SendToClient(__instance.connectionToClient, "[REPORTING] Your report was disallowed by a server plugin.", "red");
						return false;
                    }

					GameCore.Console.AddLog($"[LOCAL REPORT] Player {myPlayer.UserId} reported {player.UserId} for {reason}.", Color.white);

					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Player report successfully sent to local administrators.", "green");

					if (CheaterReport.SendReportsByWebhooks)
					{
						new Thread(delegate ()
						{
							__instance.LogReport(gct, myPlayer.UserId, player.UserId, ref reason, playerId, false, reporterNickname, reportedNickname);
						})
						{
							Priority = System.Threading.ThreadPriority.Lowest,
							IsBackground = true,
							Name = "Reporting player (locally) - " + player.UserId + " by " + myPlayer.UserId
						}.Start();
					}

					return false;
				}

				if (signature == null)
					return false;

				if (!ECDSA.VerifyBytes(player.ReferenceHub.characterClassManager.SyncedUserId + ";" + reason, signature, myPlayer.ReferenceHub.serverRoles.PublicKey))
				{
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Invalid report signature.", "red");
					return false;
				}

				GlobalReportEvent gEv = new GlobalReportEvent(reason, myPlayer, player, true);
				EventManager.Trigger<IHandlerGlobalReport>(gEv);

				if (!gEv.Allow)
                {
					gct.SendToClient(__instance.connectionToClient, "[REPORTING] Your report was disallowed by a server plugin.", "red");
					return false;
                }

				__instance._lastReport = Time.time;
				__instance._reportedPlayersAmount++;
				GameCore.Console.AddLog($"[GLOBAL REPORT] Player {myPlayer.UserId} reported {player.UserId} for {reason} | Sending to the global moderation team.", Color.green);

				new Thread(delegate ()
				{
					__instance.IssueReport(gct, myPlayer.UserId, player.UserId, player.ReferenceHub.characterClassManager.AuthToken, player.Connection.address, myPlayer.ReferenceHub.characterClassManager.AuthToken, myPlayer.Connection.address, ref reason, ref signature, ECDSA.KeyToString(myPlayer.ReferenceHub.serverRoles.PublicKey), playerId, reporterNickname, reportedNickname);
				})
				{
					Priority = System.Threading.ThreadPriority.Lowest,
					IsBackground = true,
					Name = "Reporting player - " + player.UserId + " by " + myPlayer.UserId
				}.Start();

				return false;
            }
            catch (Exception e)
            {
                Log.Add("CheaterReport.CallCmdReport", e);
                return true;
            }
        }
    }
}
