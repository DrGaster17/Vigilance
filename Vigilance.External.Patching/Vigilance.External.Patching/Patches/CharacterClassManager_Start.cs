using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Mirror;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    public static class CharacterClassManager_Start
    {
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool isFirst = false;

			foreach (CodeInstruction instruction in instructions)
			{
				if (!isFirst && instruction.opcode == OpCodes.Call && instruction.operand != null && instruction.operand is MethodBase methodBase
					&& methodBase.Name == "get_" + nameof(NetworkBehaviour.isServer))
				{
					isFirst = true;
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterClassManager), "get_" + nameof(CharacterClassManager.isLocalPlayer)));
				}
				else yield return instruction;
			}
		}
    }
}
