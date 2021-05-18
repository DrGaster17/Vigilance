using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.CheckFinish))]
    public static class Generator079_CheckFinish
    {
        public static bool Precix(Generator079 __instance)
        {
            try
            {
                if (__instance.prevFinish || __instance._localTime > 0f)
                    return false;

                if (!GeneratorExtensions.Generators.TryGetValue(__instance.GetInstanceID(), out Generator generator))
                    return true;

                GeneratorFinishEvent ev = new GeneratorFinishEvent(generator, true);
                EventManager.Trigger<IHandlerGeneratorFinish>(ev);

                if (!ev.Allow)
                    return false;

                __instance.prevFinish = true;
                __instance.epsenRenderer.sharedMaterial = __instance.matLetGreen;
                __instance.epsdisRenderer.sharedMaterial = __instance.matLedBlack;
                __instance._asource.PlayOneShot(__instance.unlockSound);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Generator079_CheckFinish), e);
                return true;
            }
        }
    }
}
