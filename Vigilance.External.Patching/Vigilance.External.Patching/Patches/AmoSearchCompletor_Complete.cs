using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Searching;
using Hints;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(AmmoSearchCompletor), nameof(AmmoSearchCompletor.Complete))]
    public static class AmoSearchCompletor_Complete
    {
        public static bool Prefix(AmmoSearchCompletor __instance)
        {
            try
            {
                Player player = PlayersList.GetPlayer(__instance.Hub);

                if (player == null)
                    return true;

                if (player.PlayerLock)
                    return false;

                PickupItemEvent ev = new PickupItemEvent(__instance.TargetPickup, player, true);
                EventManager.Trigger<IHandlerPickupItem>(ev);
                Custom.Items.API.CustomItem.EventHandling.OnPickupItem(ev);

                if (!ev.Allow)
                {
                    __instance.TargetPickup.InUse = false;
                    return false;
                }

                uint currentAmmo = __instance.CurrentAmmo;
                uint num = Math.Min(currentAmmo + (uint)__instance.TargetPickup.durability, __instance.MaxAmmo) - currentAmmo;

                if (num >= __instance.TargetPickup.durability)
                {
                    __instance.TargetPickup.Delete();
                }
                else
                {
                    Pickup targetPickup = __instance.TargetPickup;
                    targetPickup.Networkdurability = targetPickup.durability - num;

                    __instance.TargetPickup.InUse = false;

                    __instance.Hub.hints.Show(new TranslationHint(HintTranslations.MaxAmmoReached, new HintParameter[]
                    {
                        new AmmoHintParameter(__instance._ammoType),
                        new PackedULongHintParameter(__instance.MaxAmmo)
                    }, HintEffectPresets.FadeInAndOut(0.25f, 1f, 0f), 1.5f));
                }

                __instance.CurrentAmmo += num;

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(AmmoSearchCompletor), e);
                return true;
            }
        }
    }
}
