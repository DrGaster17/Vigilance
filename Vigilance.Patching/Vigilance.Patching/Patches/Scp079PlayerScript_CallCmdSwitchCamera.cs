using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdSwitchCamera))]
    public static class Scp079PlayerScript_CallCmdSwitchCamera
    {
        public static bool Prefix(Scp079PlayerScript __instance, ushort cameraId, bool lookatRotation)
        {
            try
			{
				if (!__instance._interactRateLimit.CanExecute(true) || !__instance.iAm079)
					return false;

				Camera079 camera = MapUtilities.GetCamera(cameraId);

				if (camera == null)
					return false;

				Player player = PlayersList.GetPlayer(__instance.roles._hub);

				if (player == null)
					return true;

				float num = __instance.CalculateCameraSwitchCost(camera.transform.position);

				if (num > __instance.curMana)
				{
					__instance.RpcNotEnoughMana(num, __instance.curMana);
					return false;
				}

				Scp079SwitchCameraEvent ev = new Scp079SwitchCameraEvent(player, (API.Enums.CameraType)__instance.currentCamera.cameraId, (API.Enums.CameraType)cameraId, true);
				EventManager.Trigger<IHandlerScp079SwitchCamera>(ev);

				if (!ev.Allow)
					return false;

				if (cameraId != (ushort)ev.New)
				{
					cameraId = (ushort)ev.New;
					camera = MapUtilities.GetCamera(cameraId);
				}

				__instance.RpcSwitchCamera(cameraId, lookatRotation);

				__instance.Mana -= num;
				__instance.currentCamera = camera; 

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Scp079PlayerScript_CallCmdSwitchCamera), e);
                return true;
            }
        }
    }
}
