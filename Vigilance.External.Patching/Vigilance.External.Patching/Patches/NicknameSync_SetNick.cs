using System;
using System.Text.RegularExpressions;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static class NicknameSync_SetNick
    {
        public static bool Prefix(NicknameSync __instance, string nick)
        {
			try
			{
				__instance.MyNick = nick;

				string text;

				try
				{
					Regex nickFilter = __instance._nickFilter;
					text = (((nickFilter != null) ? nickFilter.Replace(nick, __instance._replacement) : null) ?? nick);
				}

				catch (Exception arg)
				{
					ServerConsole.AddLog(string.Format("Error when filtering nick {0}: {1}", nick, arg), ConsoleColor.Gray);
					text = "(filter failed)";
				}

				if (nick != text)
				{
					__instance.DisplayName = text;
				}

				if (__instance.isLocalPlayer && ServerStatic.IsDedicated)
				{
					return false;
				}

				Player player = PlayersList.Add(__instance._hub);

				if (player == null)
					return true;

				PlayerJoinEvent ev = new PlayerJoinEvent(player, true);
				EventManager.Trigger<IHandlerPlayerJoin>(ev);

				if (!ev.Allow)
				{
					ServerConsole.Disconnect(__instance.connectionToClient, $"Your connection was disallowed by a server modification.");
					return false;
				}

				ServerConsole.AddLog(string.Concat(new string[]
				{
					"Nickname of ",
					__instance._hub.characterClassManager.UserId,
					" is now ",
					nick,
					"."
				}), ConsoleColor.Gray);

				ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Concat(new string[]
				{
					"Nickname of ",
					__instance._hub.characterClassManager.UserId,
					" is now ",
					nick,
					"."
				}), ServerLogs.ServerLogType.ConnectionUpdate, false);

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(NicknameSync_SetNick), e);
				return true;
			}
        }
    }
}
