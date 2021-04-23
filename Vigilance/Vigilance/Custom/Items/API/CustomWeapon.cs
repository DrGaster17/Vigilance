using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vigilance.Custom.Items.Extensions;
using Vigilance.EventSystem.Events;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.External.Utilities;
using Vigilance.External.Extensions;
using Vigilance.API;
using Vigilance.API.Enums;
using YamlDotNet.Serialization;
using Grenades;
using Mirror;
using MEC;
using Vigilance.Serializable;

namespace Vigilance.Custom.Items.API
{
    public abstract class CustomWeapon : CustomItem
    {
        public abstract PickupModifiers Modifiers { get; set; }

        public override ItemType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsWeapon())
                    throw new ArgumentOutOfRangeException("Type", value, "Invalid weapon type.");

                base.Type = value;
            }
        }

        public abstract float Damage { get; set; }

        public virtual uint ClipSize
        {
            get => (uint)Durability;
            set => Durability = value;
        }

        [YamlIgnore]
        public override float Durability { get; set; }

        public override void Spawn(Vector3 position) => Spawned.Add(ItemExtensions.Spawn(Type, ClipSize, position, default, (int)Modifiers.SightType, (int)Modifiers.BarrelType, (int)Modifiers.OtherType));

        public override void Give(Player player)
        {
            Inventory.SyncItemInfo syncItemInfo = new Inventory.SyncItemInfo()
            {
                durability = ClipSize,
                id = Type,
                uniq = ++Inventory._uniqId,
                modBarrel = (int)Modifiers.BarrelType,
                modSight = (int)Modifiers.SightType,
                modOther = (int)Modifiers.OtherType,
            };

            player.ReferenceHub.inventory.items.Add(syncItemInfo);

            InsideInventories.Add(syncItemInfo.uniq);
        }

        protected virtual void OnReload(WeaponReloadEvent ev)
        {
        }

        protected virtual void OnShoot(WeaponShootEvent ev)
        {
        }

        protected virtual void OnHurt(PlayerHurtEvent ev)
        {
        }

        public static class EventHandler
        {
            public static void OnHurt(PlayerHurtEvent ev)
            {
                foreach (CustomItem item in Registered)
                {
                    if (!item.Check(ev.Attacker.CurrentItem))
                        continue;

                    if (item is CustomWeapon weapon && weapon != null)
                    {
                        if (ev.Allow && ev.Attacker != ev.Target)
                            ev.SetDamage(weapon.Damage);

                        weapon.OnHurt(ev);
                    }
                }
            }

            public static void OnShoot(WeaponShootEvent ev)
            {
                foreach (CustomItem item in Registered)
                {
                    if (!item.Check(ev.Player.CurrentItem))
                        continue;

                    if (item is CustomWeapon weapon && weapon != null)
                    {
                        weapon.OnShoot(ev);
                    }
                }
            }

            public static void OnReload(WeaponReloadEvent ev)
            {
                foreach (CustomItem item in Registered)
                {
                    if (!item.Check(ev.Player.CurrentItem))
                        continue;

                    if (item is CustomWeapon weapon && weapon != null)
                    {
                        weapon.OnReload(ev);

                        if (!ev.Allow)
                            return;

                        ev.Allow = false;

                        uint remainingClip = (uint)ev.Player.CurrentItem.durability;

                        if (remainingClip >= weapon.ClipSize)
                            return;

                        Log.Debug($"{ev.Player.Nick} ({ev.Player.UserId}) [{ev.Player.Role}] is reloading a {weapon.Name} ({weapon.Id}) [{weapon.Type} ({remainingClip}/{weapon.ClipSize})]!");

                        if (ev.AnimationOnly)
                        {
                            ev.Player.ReloadWeapon();
                        }
                        else
                        {
                            int ammoType = ev.Player.ReferenceHub.weaponManager.weapons[ev.Player.ReferenceHub.weaponManager.curWeapon].ammoType;
                            uint amountToReload = Math.Min(weapon.ClipSize - remainingClip, ev.Player.ReferenceHub.ammoBox[ammoType]);

                            if (amountToReload <= 0)
                                return;

                            ev.Player.ReferenceHub.weaponManager.scp268.ServerDisable();

                            ev.Player.ReferenceHub.ammoBox[ammoType] -= amountToReload;
                            ev.Player.ReferenceHub.inventory.items.ModifyDuration(ev.Player.ReferenceHub.inventory.GetItemIndex(), ev.Player.CurrentItem.durability + amountToReload);

                            Log.Debug($"{ev.Player.Nick} ({ev.Player.UserId}) [{ev.Player.Role}] reloaded a {weapon.Name} ({weapon.Id}) [{weapon.Type} ({ev.Player.CurrentItem.durability}/{weapon.ClipSize})]!");
                        }
                    }
                }
            }
        }
    }
}
