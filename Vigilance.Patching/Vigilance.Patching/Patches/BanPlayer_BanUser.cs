using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using UnityEngine;
using Harmony;
using GameCore;
using NorthwoodLib;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    public static class BanPlayer_BanUser
    {
        public static bool Prefix(BanPlayer __instance, GameObject user, int duration, string reason, string issuer, bool isGlobalBan, ref bool __result)
        {
            try
            {
				if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
					duration = int.MaxValue;

				Player player = PlayersList.GetPlayer(user);
				Player myPlayer = PlayersList.GetPlayer(issuer);

				if (player == null)
					return true;

				if (myPlayer == null)
					myPlayer = CompCache.Player;

				string text = null;
				string address = player.Connection.address;

				if (ConfigFile.ServerConfig.GetBool("online_mode", true))
					text = player.UserId;

				bool ipBanning = ConfigFile.ServerConfig.GetBool("ip_banning", true);

				if (duration > 0 && (!ServerStatic.PermissionsHandler.IsVerified || !user.GetComponent<ServerRoles>().BypassStaff))
				{
					int @int = ConfigFile.ServerConfig.GetInt("ban_nickname_maxlength", 30);
					bool @bool = ConfigFile.ServerConfig.GetBool("ban_nickname_trimunicode", true);
					NicknameSync component = user.GetComponent<NicknameSync>();
					string text2 = string.IsNullOrEmpty(component.MyNick) ? "(no nick)" : component.MyNick;

					if (@bool)
						text2 = StringUtils.StripUnicodeCharacters(text2, "");

					if (text2.Length > @int)
						text2 = text2.Substring(0, @int);

					long issuanceTime = TimeBehaviour.CurrentTimestamp();
					long banExpirationTime = TimeBehaviour.GetBanExpirationTime((uint)duration);
					BanEvent banEvent = new BanEvent(player, myPlayer, reason, issuanceTime, banExpirationTime, true);
					EventManager.Trigger<IHandlerBan>(banEvent);

					if (!banEvent.Allow)
					{
						__result = false;
						return false;
					}

					if (text != null && !isGlobalBan)
					{
						BanHandler.IssueBan(new BanDetails
						{
							OriginalName = text2,
							Id = text,
							IssuanceTime = issuanceTime,
							Expires = banExpirationTime,
							Reason = reason,
							Issuer = issuer
						}, BanHandler.BanType.UserId);

						if (!string.IsNullOrEmpty(player.ReferenceHub.characterClassManager.UserId2))
						{
							BanHandler.IssueBan(new BanDetails
							{
								OriginalName = text2,
								Id = player.ReferenceHub.characterClassManager.UserId2,
								IssuanceTime = issuanceTime,
								Expires = banExpirationTime,
								Reason = reason,
								Issuer = issuer
							}, BanHandler.BanType.UserId);
						}
					}

					if (ipBanning || isGlobalBan)
					{
						BanHandler.IssueBan(new BanDetails
						{
							OriginalName = text2,
							Id = address,
							IssuanceTime = issuanceTime,
							Expires = banExpirationTime,
							Reason = reason,
							Issuer = issuer
						}, BanHandler.BanType.IP);
					}
				}

				string str = (duration > 0) ? "banned" : "kicked";
				string text3 = "You have been " + str + ". ";

				if (!string.IsNullOrEmpty(reason))
					text3 = text3 + "Reason: " + reason;

				foreach (Player ply in PlayersList.List)
				{
					if ((!string.IsNullOrEmpty(text) && ply.UserId == text) || (!string.IsNullOrEmpty(address) && ply.Connection.address == address && ipBanning))
                    {
						if (duration == 0)
						{
							KickEvent ev = new KickEvent(myPlayer, ply, reason, true);
							EventManager.Trigger<IHandlerKick>(ev);

							if (!ev.Allow)
							{
								__result = false;
								return false;
							}

							ServerConsole.Disconnect(ply.GameObject, text3);
							__result = true;
							return false;
						}
						else
						{
							ServerConsole.Disconnect(ply.GameObject, text3);
							__result = true;
							return false;
						}
                    }
				}

				__result = true;
				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(BanPlayer_BanUser), e);
				__result = false;
                return true;
            }
        }
    }
}
