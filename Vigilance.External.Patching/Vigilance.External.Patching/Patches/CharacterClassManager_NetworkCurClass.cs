using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), MethodType.Setter)]
    public static class CharacterClassManager_NetworkCurClass
    {
        // i hate IL and it hates me back
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
