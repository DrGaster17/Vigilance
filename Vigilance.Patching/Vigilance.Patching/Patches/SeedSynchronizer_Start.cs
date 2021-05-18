using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Utilities;
using Harmony;
using MapGeneration;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
    public static class SeedSynchronizer_Start
    {
        public static bool Prefix(SeedSynchronizer __instance)
        {
            try
            {
                int seed = MapUtilities.GenerateSeed();

                GenerateSeedEvent ev = new GenerateSeedEvent(seed);
                EventManager.Trigger<IHandlerGenerateSeed>(ev);

                Log.Add("MapGeneration", $"Server has succesfully generated a random seed: {ev.Seed}", LogType.Info);

                __instance.Network_syncSeed = ev.Seed;

                return false;    
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(SeedSynchronizer_Start), e);
                return true;
            }
        }
    }
}
