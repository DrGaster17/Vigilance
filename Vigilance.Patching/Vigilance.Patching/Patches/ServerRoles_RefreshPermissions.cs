using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshPermissions))]
    public static class ServerRoles_RefreshPermissions
    {
        public static bool Prefix(ServerRoles __instance, bool disp = false)
        {
            try
            {
                UserGroup userGroup = ServerStatic.PermissionsHandler.GetUserGroup(__instance._ccm.UserId);

                if (userGroup != null)
                {
                    __instance.SetGroup(userGroup, false, false, disp);
                }

                else if (__instance._ccm.UserId2 != null)
                {
                    userGroup = ServerStatic.PermissionsHandler.GetUserGroup(__instance._ccm.UserId2);

                    if (userGroup != null)
                    {
                        __instance.SetGroup(userGroup, false, false, disp);
                    }
                }

                __instance._hub.queryProcessor.GameplayData = PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.GameplayData);

                EventManager.Trigger<IHandlerPlayerVerified>(new PlayerVerifiedEvent(PlayersList.GetPlayer(__instance._hub)));

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(ServerRoles_RefreshPermissions), e);
                return true;
            }
        }
    }
}
