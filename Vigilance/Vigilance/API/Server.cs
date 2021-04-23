using System;
using MEC;
using GameCore;
using UnityEngine;
using RemoteAdmin;
using Vigilance.External.Utilities;
using Vigilance.External.Extensions;
using Vigilance.Configs;

namespace Vigilance.API
{
    public static class Server
    {
        public static int Port => ServerStatic.ServerPortSet ? ServerStatic.ServerPort : 7777;
        public static int MaxPlayers => ConfigFile.ServerConfig.GetInt("max_players", 20);

        public static string Name { get => ServerConsole._serverName; set => ServerConsole._serverName = value; }
        public static string Pastebin { get => LocalComponents.CharacterClassManager == null ? "" : LocalComponents.CharacterClassManager.NetworkPastebin; }
        public static string Password { get => ServerConsole.Password; set => ServerConsole.Password = value; }
        public static string ServerRandom { get => LocalComponents.CharacterClassManager == null ? "" : LocalComponents.CharacterClassManager._hub.queryProcessor.NetworkServerRandom; }
        public static string IpAddress { get => ServerConsole.Ip; }
        public static string Version => GameCore.Version.VersionString;

        public static bool Ban(Player player, int duration = 1, string message = "You have been banned.", string issuer = "Server", string reason = "No reason given.") => LocalComponents.BanPlayer.BanUser(player.GameObject, duration, reason, issuer);
        public static void Kick(Player player, string reason = "No reason given.") => ServerConsole.Disconnect(player.GameObject, reason);
        public static bool IssuePermanentBan(Player player, string reason = "No reason given") => Ban(player, int.MaxValue, reason);
        public static void RunCommand(string command) => CommandProcessor.ProcessQuery(command, LocalComponents.ReferenceHub.queryProcessor._sender);
        public static void IssueOfflineBan(Timer.Time type, int duration, string userId, string issuer, string reason) => ServerUtilities.IssueOfflineBan(type, duration, userId, issuer, reason);
        public static bool AddReservedSlot(string userId) => ServerUtilities.AddReservedSlot(userId);

        public static bool IssueBan(long expiery, string id, string issuer, string player, string reason, BanHandler.BanType banType) => BanHandler.IssueBan(new BanDetails
        {
            Expires = expiery,
            Id = id,
            IssuanceTime = DateTime.Now.Ticks,
            Issuer = issuer,
            OriginalName = player,
            Reason = reason
        }, banType);

        public static void Reload(bool plugins = false)
        {
            ConfigFile.ReloadGameConfigs(false);
            ConfigManager.Reload();
            ServerStatic.PermissionsHandler?.RefreshPermissions();
            PlayersList.ForEach(x => x.ReferenceHub.serverRoles.RefreshPermissions());

            if (plugins)
                PluginManager.ReloadPlugins();
        }

        public static void Restart(bool safeRestart = true)
        {
            if (safeRestart)
            {
                LocalComponents.PlayerStats.Roundrestart();
                Timing.CallDelayed(1f, () => Application.Quit());
            }
            else
                Application.Quit();
        }
    }
}