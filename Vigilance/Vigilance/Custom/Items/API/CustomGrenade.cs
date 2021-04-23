using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vigilance.External.Utilities;
using Vigilance.External.Extensions;
using Vigilance.API;
using Vigilance.API.Enums;
using YamlDotNet.Serialization;
using Grenades;
using Mirror;
using MEC;
using Vigilance.Custom.Items.Handling;
using Vigilance.EventSystem.Events;

namespace Vigilance.Custom.Items.API
{
    public abstract class CustomGrenade : CustomItem
    {
        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsThrowable())
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid grenade type.");

                base.Type = value;
            }
        }

        public abstract bool ExplodeOnCollision { get; set; }
        public abstract float FuseTime { get; set; }

        [YamlIgnore]
        public override float Durability { get; set; }

        [YamlIgnore]
        protected HashSet<GameObject> Tracked { get; } = new HashSet<GameObject>();

        public virtual Grenade Spawn(Vector3 position, Vector3 velocity, float fuseTime = 3f, ItemType grenadeType = ItemType.GrenadeFrag, Player player = null)
        {
            if (player == null)
                player = LocalComponents.Player;

            GrenadeManager grenadeManager = player.GetComponent<GrenadeManager>();
            GrenadeSettings settings = grenadeManager.availableGrenades.FirstOrDefault(g => g.inventoryID == grenadeType);
            Grenade grenade = GameObject.Instantiate(settings.grenadeInstance).GetComponent<Grenade>();

            grenade.FullInitData(grenadeManager, position, Quaternion.Euler(grenade.throwStartAngle), velocity, grenade.throwAngularVelocity, player.IsHost ? Team.RIP : player.ReferenceHub.characterClassManager.CurRole.team);
            grenade.NetworkfuseTime = NetworkTime.time + fuseTime;

            Tracked.Add(grenade.gameObject);

            NetworkServer.Spawn(grenade.gameObject);

            if (ExplodeOnCollision)
                grenade.gameObject.AddComponent<CollisionHandler>().Init(player.GameObject, grenade);

            return grenade;
        }

        protected virtual void OnThrowing(ThrowGrenadeEvent ev)
        {
        }

        protected virtual void OnExploding(GrenadeExplodeEvent ev)
        {
        }

        protected override void OnWaitingForPlayers()
        {
            Tracked.Clear();

            base.OnWaitingForPlayers();
        }

        protected bool Check(GameObject grenade) => Tracked.Contains(grenade);

        public static class Handler
        {
            public static void OnWaiting()
            {
                foreach (CustomItem item in Registered)
                {
                    if (item is CustomGrenade nade)
                        nade.OnWaitingForPlayers();
                }
            }

            public static void OnExplode(GrenadeExplodeEvent ev)
            {
                foreach (CustomItem item in Registered)
                {
                    if (!item.Check(ev.Player.CurrentItem))
                        continue;

                    if (item is CustomGrenade nade)
                    {
                        nade.OnExploding(ev);
                    }
                }
            }

            public static void OnThrow(ThrowGrenadeEvent ev)
            {
                foreach (CustomItem item in Registered)
                {
                    if (!item.Check(ev.Thrower.CurrentItem))
                        continue;

                    if (item is CustomGrenade nade)
                    {
                        nade.OnThrowing(ev);

                        if (!ev.Allow)
                            return;

                        ev.Allow = false;

                        nade.InsideInventories.Remove(ev.Thrower.CurrentItem.uniq);
                        ev.Thrower.RemoveItem(ev.Thrower.CurrentItem);

                        Timing.CallDelayed(1f, () =>
                        {
                            Vector3 position = ev.Thrower.PlayerCamera.TransformPoint(new Vector3(0.0715f, 0.0225f, 0.45f));
                            nade.Spawn(position, ev.Thrower.PlayerCamera.forward * 9f, nade.FuseTime, nade.Type, ev.Thrower);
                        });
                    }
                }
            }
        }
    }
}
