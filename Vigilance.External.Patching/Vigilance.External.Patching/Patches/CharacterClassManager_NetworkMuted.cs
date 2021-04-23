using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Harmony;
using Mirror;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkMuted), MethodType.Setter)]
    public static class CharacterClassManager_NetworkMuted
    {
        public static bool Prefix(CharacterClassManager __instance, bool value)
        {
            try
            {
                ChangeMuteStatusEvent ev = new ChangeMuteStatusEvent(PlayersList.GetPlayer(__instance._hub), __instance.Muted, value, true);
                EventManager.Trigger<IHandlerChangeMuteStatus>(ev);

                if (!ev.Allow)
                    return false;

                value = ev.New;

                if (NetworkServer.localClientActive && !__instance.getSyncVarHookGuard(2UL))
                {
                    __instance.setSyncVarHookGuard(2UL, true);
                    __instance.SetMuted(value);
                    __instance.setSyncVarHookGuard(2UL, false);
                }

                __instance.SetSyncVar(value, ref __instance.Muted, 2UL);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CharacterClassManager_NetworkMuted), e);
                return true;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool isNOPDetected = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Nop)
                    isNOPDetected = true;

                if (!isNOPDetected)
                    yield return new CodeInstruction(OpCodes.Nop);
                else
                    yield return instruction;
            }
        }
    }
}
