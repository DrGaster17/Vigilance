using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RemoteAdmin;
using UnityEngine;
using Mirror;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Plugins.Commands
{
    public static class CommandUtils
    {
        public static List<GameObject> Dummies = new List<GameObject>();
        public static List<WorkStation> Stations = new List<WorkStation>();

        public static void ListAllComponentsToFile()
        {
            try
            {
                GameObject[] comps = UnityEngine.Object.FindObjectsOfType<GameObject>();

                string s = "Listing all components ...:\n";

                foreach (GameObject obj in comps)
                {
                    try
                    {
                        s += $"----------------- {obj.GetInstanceID()} - START - -----------------\n";
                        s += $"ID: {obj.GetInstanceID()}\n";

                        if (!string.IsNullOrEmpty(obj.name))
                            s += $"Name: {obj.name}\n";

                        s += $"Tag: {obj.tag}\n\n";

                        if (obj.transform != null)
                        {
                            s += $"Position: {obj.transform.position.AsString()}\n";
                            s += $"Rotation: {obj.transform.rotation.eulerAngles.AsString()}\n";
                            s += $"Scale: {obj.transform.localScale.AsString()}\n\n";
                        }

                        s += $"Layer: {obj.layer}\n";
                        s += $"LayerName: {(LayerType)obj.layer}\n\n";

                        if (obj.transform != null && obj.transform.parent != null)
                        {
                            s += $"Parent ID: {obj.transform.parent.GetInstanceID()}\n";
                            s += $"Parent Name: {obj.transform.name}\n";
                            s += $"Parent Tag: {obj.transform.parent.tag}\n\n";

                            s += $"Parent Position: {obj.transform.parent.position.AsString()}\n";
                            s += $"Parent Rotation: {obj.transform.parent.rotation.eulerAngles.AsString()}\n";
                            s += $"Parent Scale: {obj.transform.parent.localScale.AsString()}\n\n";

                            if (obj.transform.parent.gameObject != null)
                            {
                                s += $"Parent Layer: {obj.transform.parent.gameObject.layer}\n";
                                s += $"Parent LayerName: {(LayerType)obj.transform.parent.gameObject.layer}\n\n\n";
                            }
                        }

                        s += $"----------------- {obj.GetInstanceID()} - END - -----------------\n";
                    }
                    catch (Exception e)
                    {
                        Log.Add("ComponentsToFile-A", e);
                        continue;
                    }
                }

                if (File.Exists(CommandHandler.ComponentsFilePath))
                    File.Delete(CommandHandler.ComponentsFilePath);

                File.Create(CommandHandler.ComponentsFilePath).Close();

                File.WriteAllText(CommandHandler.ComponentsFilePath, s);
            }
            catch (Exception e)
            {
                Log.Add("ComponentsToFile-B", e);
            }
        }

        public static Pickup SpawnItemOfSize(ItemType item, Vector3 scale, Vector3 pos, Quaternion rot)
        {
            GameObject pickup = PrefabType.Pickup.Spawn(pos, rot, scale);

            Pickup i = pickup.GetComponent<Pickup>() ?? pickup.AddComponent<Pickup>();

            i.SetIDFull(item);

            return i;
        }

        public static GameObject SpawnDummy(RoleType role, ItemType item, Vector3 scale, Vector3 position, Quaternion rotation)
        {
            GameObject instance = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));

            if (instance == null)
                return null;

            CharacterClassManager ccm = instance.GetComponent<CharacterClassManager>() ?? instance.AddComponent<CharacterClassManager>();
            QueryProcessor qp = instance.GetComponent<QueryProcessor>() ?? instance.AddComponent<QueryProcessor>();
            NicknameSync nick = instance.GetComponent<NicknameSync>() ?? instance.AddComponent<NicknameSync>();
            Inventory inv = instance.GetComponent<Inventory>() ?? instance.AddComponent<Inventory>();

            ccm.UserId = "Dummy";
            ccm.CurClass = role;
            ccm.RefreshModel(role);

            qp.PlayerId = 9999;
            qp.NetworkPlayerId = 9999;

            nick.Network_myNickSync = "Dummy";

            if (!inv.items.Select(x => x.id).Contains(item))
                inv.AddNewItem(item);

            inv.SetCurItem(item);

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;

            NetworkServer.Spawn(instance);

            Dummies.Add(instance);

            return instance;
        }

        public static WorkStation SpawnWS(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            GameObject instance = PrefabType.WorkStation.Spawn(pos, rot, scale);

            WorkStation station = instance.GetComponent<WorkStation>() ?? instance.AddComponent<WorkStation>();

            Stations.Add(station);

            return station;
        }

        public static List<BanDetails> GetBansFor(string text)
        {
            List<BanDetails> bansMatched = new List<BanDetails>();
            List<BanDetails> allBans = new List<BanDetails>(BanHandler.GetBans(BanHandler.BanType.IP));

            allBans.AddRange(BanHandler.GetBans(BanHandler.BanType.NULL));
            allBans.AddRange(BanHandler.GetBans(BanHandler.BanType.UserId));

            foreach (BanDetails ban in allBans)
            {
                if (ban.Id == text || ban.OriginalName.ToLower() == text.ToLower() || ban.OriginalName.ToLower().Contains(text.ToLower()))
                    bansMatched.Add(ban);
            }

            return bansMatched;
        }

        public static void RemoveBans(IEnumerable<BanDetails> bans)
        {
            foreach (BanDetails ban in bans)
            {
                BanHandler.RemoveBan(ban.Id, BanHandler.BanType.IP);
                BanHandler.RemoveBan(ban.Id, BanHandler.BanType.NULL);
                BanHandler.RemoveBan(ban.Id, BanHandler.BanType.UserId);
            }
        }

        public static bool IssueOfflineBan(string[] commandLine, string issuer, out string expiery, out string reason, out string error)
        {
            if (commandLine.Length < 2)
            {
                expiery = "";
                error = "Missing arguments!\nUsage: oban <UserID/IP> <Duration> (Reason)";
                reason = "";
                return false;
            }

            reason = commandLine.Length > 2 ? commandLine.Skip(2).ToArray().Combine() : "No reason provided.";

            string id = commandLine[0];
            string timeAndDur = commandLine[1];

            string durOnly = timeAndDur.Where(x => int.TryParse(x.ToString(), out _)).Select(x => x.ToString()).ToArray().Combine().Replace(" ", "");
            string timeOnly = timeAndDur.Where(x => !durOnly.Contains(x)).Select(x => x.ToString()).ToArray().Combine();

            if (!Timer.TryParse(timeOnly, out Timer.Time timeUnit))
            {
                expiery = "";
                reason = "";
                error = "Cannot parse the Time Unit.";
                return false;
            }

            if (!int.TryParse(durOnly, out int duration))
            {
                expiery = "";
                reason = "";
                error = "Cannot parse the duration.";
                return false;
            }

            Server.IssueOfflineBan(timeUnit, duration, id, issuer, reason);

            expiery = $"{duration} {timeUnit.ToString().ToLower()}(s)";
            error = "";
            return true;
        }

        public static void ClearStations()
        {
            Stations.ForEach(x => NetworkServer.Destroy(x.gameObject));
            Stations.Clear();
        }

        public static void ClearDummies()
        {
            Dummies.ForEach(x => NetworkServer.Destroy(x));
            Dummies.Clear();
        }
    }
}
