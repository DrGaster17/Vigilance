using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RemoteAdmin;
using Vigilance.External.Extensions;

namespace Vigilance.API
{
    public static class PlayersList
    {
        public static List<Player> List { get; } = new List<Player>();

        public static Dictionary<ReferenceHub, Player> Hubs { get; } = new Dictionary<ReferenceHub, Player>();
        public static Dictionary<GameObject, Player> GameObjects { get; } = new Dictionary<GameObject, Player>();
        public static Dictionary<string, Player> UserIdCache { get; } = new Dictionary<string, Player>();
        public static Dictionary<int, Player> PlayerIdCache { get; } = new Dictionary<int, Player>();

        public static string String => GetString();
        public static int Count => List.Count;

        public static void Clear()
        {
            Hubs.Clear();
            UserIdCache.Clear();
            PlayerIdCache.Clear();
            List.Clear();
        }

        public static void ForEach(Action<Player> action) => List.ForEach(action);
        public static IEnumerable<Player> Where(Func<Player, bool> predicate) => List.Where(predicate);
        public static void CopyTo(Player[] copyTo) => List.CopyTo(copyTo);

        public static Player Add(ReferenceHub player)
        {
            try
            {
                bool isHost = player == null || player.characterClassManager == null || player.queryProcessor == null 
                    || string.IsNullOrEmpty(player.characterClassManager.UserId) || player.queryProcessor.PlayerId == 0 
                    || player.characterClassManager.IsHost || player.isLocalPlayer || player.isDedicatedServer;

                if (isHost)
                    return null;

                if (Hubs.TryGetValue(player, out Player ply))
                    return ply;

                Log.Add("PlayersList", $"[{player.queryProcessor.PlayerId}] {player.nicknameSync.MyNick} ({player.characterClassManager.UserId}] [{player.queryProcessor._ipAddress}) joined.", LogType.Debug);

                ply = new Player(player);

                List.Add(ply);

                Hubs.Add(player, ply);

                GameObjects.Add(player.gameObject, ply);

                UserIdCache.Add(player.characterClassManager.UserId, ply);

                if (!string.IsNullOrEmpty(player.characterClassManager.UserId2) && !UserIdCache.ContainsKey(player.characterClassManager.UserId2))
                    UserIdCache.Add(player.characterClassManager.UserId2, ply);

                PlayerIdCache.Add(player.queryProcessor.PlayerId, ply);

                return ply;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Add), e);
                return null;
            }
        }

        public static bool Remove(ReferenceHub player)
        {
            try
            {
                bool isHost = player == null || player.characterClassManager == null || player.queryProcessor == null
                    || string.IsNullOrEmpty(player.characterClassManager.UserId) || player.queryProcessor.PlayerId == 0
                    || player.characterClassManager.IsHost || player.isLocalPlayer || player.isDedicatedServer;

                if (isHost)
                    return false;

                if (!Hubs.TryGetValue(player, out Player ply))
                    return false;

                Log.Add("PlayersList", $"[{player.queryProcessor.PlayerId}] {player.nicknameSync.MyNick} ({player.characterClassManager.UserId}] [{player.queryProcessor._ipAddress}] disconnected.", LogType.Debug);

                List.Remove(ply);
                Hubs.Remove(player);
                GameObjects.Remove(player.gameObject);
                UserIdCache.Remove(player.characterClassManager.UserId);
                PlayerIdCache.Remove(player.queryProcessor.PlayerId);

                return true;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Remove), e);
                return false;
            }
        }
        

        public static Player GetPlayer(GameObject gameObject)
        {
            try
            {
                if (gameObject == null)
                    return null;

                if (GameObjects.TryGetValue(gameObject, out Player player))
                {
                    return player;
                }
                else
                {
                    if (ReferenceHub.TryGetHub(gameObject, out ReferenceHub hub))
                    {
                        if (Hubs.TryGetValue(hub, out player))
                        {
                            return player;
                        }
                        else
                        {
                            if (hub.queryProcessor != null && hub.queryProcessor.PlayerId > 0 && PlayerIdCache.TryGetValue(hub.queryProcessor.PlayerId, out player))
                            {
                                return player;
                            }
                            else
                            {
                                if (hub.characterClassManager != null && !string.IsNullOrEmpty(hub.characterClassManager.UserId) && UserIdCache.TryGetValue(hub.characterClassManager.UserId, out player))
                                {
                                    return player;
                                }
                            }
                        }
                    }
                    else
                    {
                        QueryProcessor qp = gameObject.GetComponent<QueryProcessor>();

                        if (qp != null && qp.PlayerId > 0)
                        {
                            if (PlayerIdCache.TryGetValue(qp.PlayerId, out player))
                                return player;
                        }
                        else
                        {
                            CharacterClassManager ccm = gameObject.GetComponent<CharacterClassManager>();

                            if (ccm != null && !string.IsNullOrEmpty(ccm.UserId))
                            {
                                if (UserIdCache.TryGetValue(ccm.UserId, out player))
                                    return player;
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Add(nameof(GetPlayer), e);
                return null;
            }
        }

        public static Player GetPlayer(ReferenceHub hub)
        {
            try
            {
                if (hub == null)
                    return null;    

                if (Hubs.TryGetValue(hub, out Player player))
                {
                    return player;
                }
                else
                {
                    if (hub.queryProcessor != null && hub.queryProcessor.PlayerId > 0 && PlayerIdCache.TryGetValue(hub.queryProcessor.PlayerId, out player))
                    {
                        return player;
                    }
                    else
                    {
                        if (hub.characterClassManager != null && !string.IsNullOrEmpty(hub.characterClassManager.UserId) && UserIdCache.TryGetValue(hub.characterClassManager.UserId, out player))
                        {
                            return player;
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Add(nameof(GetPlayer), e);
                return null;
            }
        }

        public static Player GetPlayer(int playerId)
        {
            if (!PlayerIdCache.TryGetValue(playerId, out Player player))
            {
                foreach (Player ply in List)
                {
                    if (ply.PlayerId == playerId)
                    {
                        player = ply;

                        break;
                    }
                }

            }

            return player;
        }

        public static Player GetPlayerByUserId(string id)
        {
            if (!UserIdCache.TryGetValue(id, out Player ply))
            {
                foreach (Player player in List)
                {
                    if (player.UserId == id || player.ParsedUserId.ToString() == id)
                    {
                        ply = player;

                        break;
                    }
                }
            }

            return ply;
        }

        public static Player GetPlayer(string args)
        {
            try
            {
                Player playerFound = null;

                foreach (string userId in UserIdCache.Keys)
                {
                    if (userId == args)
                        return UserIdCache[userId];
                }

                if (int.TryParse(args, out int id))
                    return GetPlayer(id);

                if (args.EndsWith("@steam") || args.EndsWith("@discord") || args.EndsWith("@northwood") || args.EndsWith("@patreon"))
                {
                    playerFound = GetPlayerByUserId(args);
                }
                else
                {
                    int maxNameLength = 31, lastnameDifference = 31;
                    string firstString = args.ToLower();

                    foreach (Player player in List)
                    {
                        if (!player.Nick.ToLower().Contains(args.ToLower()))
                            continue;

                        if (firstString.Length < maxNameLength)
                        {
                            int x = maxNameLength - firstString.Length;
                            int y = maxNameLength - player.Nick.Length;
                            string secondString = player.Nick;
                            for (int i = 0; i < x; i++)
                                firstString += "z";
                            for (int i = 0; i < y; i++)
                                secondString += "z";
                            int nameDifference = firstString.GetDistance(secondString);
                            if (nameDifference < lastnameDifference)
                            {
                                lastnameDifference = nameDifference;
                                playerFound = player;
                            }
                        }
                    }
                }

                return playerFound;
            }
            catch (Exception exception)
            {
                Log.Add(exception);
                return null;
            }
        }

        public static string GetString()
        {
            if (List.Count < 1)
                return $"There are not any players on the server.";

            string s = $"Players ({List.Count}):";

            foreach (Player player in List)
            {
                s += $"\n{player}";
            }

            return s;
        }

        public static IEnumerable<Player> GetPlayers(RoleType role) => List.Where(x => x.Role == role);
        public static IEnumerable<Player> GetPlayers(Team team) => List.Where(x => x.Team == team);

        public static bool Contains(Player player) => List.Contains(player);
        public static bool Contains(ReferenceHub player) => Hubs.ContainsKey(player);
        public static bool Contains(GameObject obj) => GameObjects.ContainsKey(obj);
    }
}