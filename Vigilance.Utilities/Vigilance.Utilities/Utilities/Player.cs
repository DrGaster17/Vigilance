using System;
using System.Collections.Generic;

using Vigilance.Patching;
using Vigilance.Extensions;

using Vigilance.API;
using Vigilance.API.Enums;

using Mirror;
using UnityEngine;
using CustomPlayerEffects;
using MEC;

namespace Vigilance.Utilities
{
    public static class PlayerUtilities
    {
        public static List<Ragdoll> GetRagdolls(Player p)
        {
            if (!PatchData.Ragdolls.ContainsKey(p))
                PatchData.Ragdolls.Add(p, new List<Ragdoll>());

            return PatchData.Ragdolls[p];
        }

        public static List<Pickup> GetPickups(Player p)
        {
            if (!PatchData.Pickups.ContainsKey(p))
                PatchData.Pickups.Add(p, new List<Pickup>());

            return PatchData.Pickups[p];
        }

        public static string GetGroupNode(UserGroup group)
        {
            if (group == null)
                return "";

            PermissionsHandler ph = ServerStatic.PermissionsHandler;

            if (ph == null)
                throw new Exception("Update your Remote Admin config!");

            foreach (KeyValuePair<string, UserGroup> pair in ph.GetAllGroups())
            {
                if (pair.Value == group || (pair.Value.BadgeColor == group.BadgeColor && pair.Value.BadgeText == group.BadgeText && pair.Value.Cover == group.Cover && pair.Value.HiddenByDefault == group.HiddenByDefault && pair.Value.KickPower == group.KickPower && pair.Value.Permissions == group.Permissions && pair.Value.RequiredKickPower == group.RequiredKickPower && pair.Value.Shared == group.Shared))
                {
                    return pair.Key;
                }
            }

            return "";
        }

        public static bool GetBadgeHidden(ServerRoles roles) => string.IsNullOrEmpty(roles.HiddenBadge);
        public static void SetBadgeHidden(bool value, Player player)
        {
            if (value) 
                player.ReferenceHub.characterClassManager.CmdRequestHideTag(); 
            else 
                player.ReferenceHub.characterClassManager.CallCmdRequestShowTag(false);
        }

        public static ulong GetParsedId(Player player)
        {
            string id = player.IsHost ? "Dedicated Server" : player.UserId.Split('@')[0];

            if (id == "Dedicated Server")
                return 0;

            if (!ulong.TryParse(id, out ulong user))
                return 0;

            return user;
        }

        public static UserIdType GetIdType(Player player)
        {
            if (string.IsNullOrEmpty(player.UserId))
                return UserIdType.Server;

            if (player.UserId.EndsWith("@steam"))
                return UserIdType.Steam;

            if (player.UserId.EndsWith("@discord"))
                return UserIdType.Discord;

            if (player.UserId.EndsWith("@northwood"))
                return UserIdType.Northwood;

            if (player.UserId.EndsWith("@patreon"))
                return UserIdType.Patreon;

            return UserIdType.Unspecified;
        }

        public static Room GetRoom(Player player) => MapUtilities.FindParentRoom(player.GameObject);

        public static void SetScale(Player player, Vector3 scale)
        {
            try
            {
                NetworkIdentity identity = player.ConnectionIdentity;
                player.GameObject.transform.localScale = scale;
                ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
                destroyMessage.netId = identity.netId;

                foreach (GameObject obj in PlayerManager.players)
                {
                    NetworkConnection playerCon = player.Connection;

                    if (obj != player.GameObject)
                        playerCon.Send(destroyMessage, 0);

                    NetworkServer.SendSpawnMessage(identity, playerCon);
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static void AddItem(Player player, ItemType item)
        {
            if (player.ReferenceHub.inventory.items.Count >= 8)
                MapUtilities.SpawnItem(item, player.Position, player.Rotations);
            else 
                player.ReferenceHub.inventory.AddNewItem(item);
        }

        public static void SetExperience(Player player, float exp)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.Exp = exp;
            player.ReferenceHub.scp079PlayerScript.OnExpChange();
        }

        public static void SetLevel(Player player, byte level)
        {
            if (player.ReferenceHub.scp079PlayerScript == null || player.ReferenceHub.scp079PlayerScript.Lvl == level)
                return;

            player.ReferenceHub.scp079PlayerScript.Lvl = level;
            player.ReferenceHub.scp079PlayerScript.TargetLevelChanged(player.Connection, level);
        }

        public static void SetMaxEnergy(Player player, float value)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.NetworkmaxMana = value;
            player.ReferenceHub.scp079PlayerScript.levels[player.ReferenceHub.scp079PlayerScript.Lvl].maxMana = value;
        }

        public static void SetEnergy(Player player, float value)
        {
            if (player.ReferenceHub.scp079PlayerScript == null)
                return;

            player.ReferenceHub.scp079PlayerScript.Mana = value;
        }

        public static bool GetActiveEffect<T>(Player player) where T : PlayerEffect
        {
            if (player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(T), out PlayerEffect playerEffect))
                return playerEffect.Enabled;

            return false;
        }

        public static void DisableAllEffects(Player player)
        {
            foreach (KeyValuePair<Type, PlayerEffect> effect in player.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (effect.Value.Enabled)
                    effect.Value.ServerDisable();
            }
        }

        public static PlayerEffect GetEffect(Player player, EffectType effect)
        {
            bool found = player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(effect.Type(), out var playerEffect);
            return found ? null : playerEffect;
        }

        public static bool TryGetEffect(Player player, EffectType effect, out PlayerEffect playerEffect)
        {
            playerEffect = GetEffect(player, effect);
            return playerEffect != null;
        }

        public static byte GetEffectIntensity<T>(Player player) where T : PlayerEffect
        {
            if (player.ReferenceHub.playerEffectsController.AllEffects.TryGetValue(typeof(T), out PlayerEffect playerEffect))
                return playerEffect.Intensity;

            throw new ArgumentException("The given type is invalid.");
        }

        public static void EnableEffect(Player player, EffectType effect, float duration = 0f, bool addDurationIfActive = false)
        {
            if (TryGetEffect(player, effect, out var pEffect))
                player.ReferenceHub.playerEffectsController.EnableEffect(pEffect, duration, addDurationIfActive);
        }

        public static void DisableEffect(Player player, EffectType effect)
        {
            if (TryGetEffect(player, effect, out var playerEffect))
                playerEffect.ServerDisable();
        }

        public static void DisableEffect<T>(Player player) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.DisableEffect<T>();
        public static void EnableEffect<T>(Player player, float duration = 0f, bool addDurationIfActive = false) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.EnableEffect<T>(duration, addDurationIfActive);
        public static void EnableEffect(Player player, PlayerEffect effect, float duration = 0f, bool addDurationIfActive = false) => player.ReferenceHub.playerEffectsController.EnableEffect(effect, duration, addDurationIfActive);
        public static bool EnableEffect(Player player, string effect, float duration = 0f, bool addDurationIfActive = false) => player.ReferenceHub.playerEffectsController.EnableByString(effect, duration, addDurationIfActive);
        public static void ChangeEffectIntensity<T>(Player player, byte intensity) where T : PlayerEffect => player.ReferenceHub.playerEffectsController.ChangeEffectIntensity<T>(intensity);
        public static void ChangeEffectIntensity(Player player, string effect, byte intensity, float duration = 0) => player.ReferenceHub.playerEffectsController.ChangeByString(effect, intensity, duration);

        public static void Rocket(Player player, float speed) => Timing.RunCoroutine(Coroutines.Rocket(player, speed));
    }
}
