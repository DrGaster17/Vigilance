﻿using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using RemoteAdmin;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetGroup))]
    public static class ServerRoles_SetGroup
    {
        public static bool Prefix(ServerRoles __instance, UserGroup group, bool ovr, bool byAdmin = false, bool disp = false)
        {
            try
            {
				Player player = PlayersList.GetPlayer(__instance._hub);

				if (player == null)
					return true;

				SetGroupEvent ev = new SetGroupEvent(player, group, true);
				EventManager.Trigger<IHandlerSetGroup>(ev);

				group = ev.Group;

				if (!ev.Allow)
					return false;

				if (group == null)
				{
					if (__instance.RaEverywhere && __instance._globalPerms == ServerStatic.PermissionsHandler.FullPerm)
					{
						Patcher.LogWarn(typeof(ServerRoles_SetGroup), $"Failed while revoking permissions for {player.UserId}: this player is a Northwood Staff member.");
						return false;
					}

					__instance.RemoteAdmin = (__instance._globalPerms > 0UL);
					__instance.Permissions = __instance._globalPerms;
					__instance.RemoteAdminMode = ((__instance._globalPerms == 0UL) ? ServerRoles.AccessMode.LocalAccess : ServerRoles.AccessMode.GlobalAccess);
					__instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
					__instance.Group = null;
					__instance.SetColor(null);
					__instance.SetText(null);
					__instance._badgeCover = false;

					if (!string.IsNullOrEmpty(__instance.PrevBadge))
						__instance.NetworkGlobalBadge = __instance.PrevBadge;

					__instance.TargetCloseRemoteAdmin(__instance.connectionToClient);
					__instance.SendRealIds();
					__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your permissions have been revoked by the server.", "red");
					return false;
				}
				else
				{
					__instance.Group = group;
					__instance._badgeCover = group.Cover;

					if (!__instance.OverwatchPermitted && PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
						__instance.OverwatchPermitted = true;

					if ((group.Permissions | __instance._globalPerms) > 0UL && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions | __instance._globalPerms))
					{
						__instance.RemoteAdmin = true;
						__instance.Permissions = (group.Permissions | __instance._globalPerms);
						__instance.RemoteAdminMode = ((__instance._globalPerms > 0UL) ? ServerRoles.AccessMode.GlobalAccess : (ovr ? ServerRoles.AccessMode.PasswordOverride : ServerRoles.AccessMode.LocalAccess));
						__instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
						__instance.GetComponent<QueryProcessor>().PasswordTries = 0;
						__instance.TargetOpenRemoteAdmin(__instance.connectionToClient, ovr);
						__instance._ccm.TargetConsolePrint(__instance.connectionToClient, (!byAdmin) ? "Your remote admin access has been granted (local permissions)." : "Your remote admin access has been granted (set by server administrator).", "cyan");
					}
					else
					{
						__instance.RemoteAdmin = false;
						__instance.Permissions = (group.Permissions | __instance._globalPerms);
						__instance.RemoteAdminMode = ((__instance._globalPerms > 0UL) ? ServerRoles.AccessMode.GlobalAccess : ServerRoles.AccessMode.LocalAccess);
						__instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
						__instance.TargetCloseRemoteAdmin(__instance.connectionToClient);
					}

					__instance.SendRealIds();

					bool flag = __instance.Staff || PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.ViewHiddenBadges);
					bool flag2 = __instance.Staff || PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.ViewHiddenGlobalBadges);

					if (flag || flag2)
					{
						foreach (ReferenceHub hub in ReferenceHub.Hubs.Values)
						{
							ServerRoles component = hub.serverRoles;

							if (!string.IsNullOrEmpty(component.HiddenBadge) && (!component.GlobalHidden || flag2) && (component.GlobalHidden || flag))
								component.TargetSetHiddenRole(__instance.connectionToClient, component.HiddenBadge);
						}

						if (flag && flag2)
							__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (local and global) have been displayed for you (if there are any).", "gray");
						else if (flag)
							__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (local only) have been displayed for you (if there are any).", "gray");
						else
							__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (global only) have been displayed for you (if there are any).", "gray");
					}

					ServerLogs.AddLog(ServerLogs.Modules.Permissions, __instance._hub.LoggedNameFromRefHub() + " has been assigned to group " + group.BadgeText + ".", ServerLogs.ServerLogType.ConnectionUpdate, false);

					if (group.BadgeColor == "none")
						return false;

					if (__instance._hideLocalBadge || (group.HiddenByDefault && !disp && !__instance._neverHideLocalBadge))
					{
						__instance._badgeCover = false;

						if (!string.IsNullOrEmpty(__instance.MyText))
							return false;

						__instance.NetworkMyText = null;
						__instance.NetworkMyColor = "default";
						__instance.HiddenBadge = group.BadgeText;
						__instance.RefreshHiddenTag();
						__instance.TargetSetHiddenRole(__instance.connectionToClient, group.BadgeText);

						if (!byAdmin)
						{
							__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your role has been granted, but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "yellow");
							return false;
						}

						__instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your role has been granted to you (set by server administrator), but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "cyan");
						return false;
					}
					else
					{
						__instance.HiddenBadge = null;
						__instance.RpcResetFixed();
						__instance.NetworkMyText = group.BadgeText;
						__instance.NetworkMyColor = group.BadgeColor;

						if (!byAdmin)
						{
							__instance._ccm.TargetConsolePrint(__instance.connectionToClient, string.Concat(new string[]
							{
								"Your role \"",
								group.BadgeText,
								"\" with color ",
								group.BadgeColor,
								" has been granted to you (local permissions)."
							}), "cyan");

							return false;
						}

						__instance._ccm.TargetConsolePrint(__instance.connectionToClient, string.Concat(new string[]
						{
							"Your role \"",
							group.BadgeText,
							"\" with color ",
							group.BadgeColor,
							" has been granted to you (set by server administrator)."
						}), "cyan");
					}

					Patcher.Log(typeof(ServerRoles_SetGroup), $"Player {player.Nick} ({player.UserId}) has been assigned to group: {(ServerStatic.PermissionsHandler._members.TryGetValue(player.UserId, out string key) ? key : "none")}");
				}
				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ServerRoles_SetGroup), e);
                return true;
            }
        }
    }
}
