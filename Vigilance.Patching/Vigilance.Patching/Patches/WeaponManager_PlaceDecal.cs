using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.PlaceDecal))]
    public static class WeaponManager_PlaceDecal
    {
        public static bool Prefix(WeaponManager __instance, bool isBlood, Ray ray, int classId, float distanceAddition)
        {
            try
            {
                if (__instance._hub.characterClassManager.Classes.CheckBounds(classId) && Physics.Raycast(ray, out RaycastHit raycastHit, isBlood ? (10f + distanceAddition) : 100f, __instance.bloodMask))
                {
                    if (isBlood)
                    {
                        PlaceBloodEvent bEv = new PlaceBloodEvent(true, __instance._hub.characterClassManager.Classes.SafeGet(classId).bloodType, raycastHit.point + raycastHit.normal * 0.01f);
                        EventManager.Trigger<IHandlerPlaceBlood>(bEv);

                        if (!bEv.Allow)
                            return false;

                        __instance.RpcPlaceDecal(isBlood, (sbyte)bEv.Type, bEv.Position, Quaternion.FromToRotation(Vector3.up, raycastHit.normal));
                    }
                    else
                    {
                        PlaceDecalEvent ev = new PlaceDecalEvent(true, raycastHit.point + raycastHit.normal * 0.01f);
                        EventManager.Trigger<IHandlerPlaceDecal>(ev);

                        if (!ev.Allow)
                            return false;

                        __instance.RpcPlaceDecal(isBlood, (sbyte)__instance._hub.characterClassManager.Classes.SafeGet(classId).bloodType, ev.Position, Quaternion.FromToRotation(Vector3.up, raycastHit.normal));
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(WeaponManager_PlaceDecal), e);
                return true;
            }
        }
    }
}
