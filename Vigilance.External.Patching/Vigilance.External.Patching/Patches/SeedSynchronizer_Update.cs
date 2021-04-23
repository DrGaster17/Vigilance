using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.External.Utilities;
using Harmony;
using MapGeneration;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Update))]
    public static class SeedSynchronizer_Update
    {
        public static bool EventCalled;

        public static void Postfix()
        {
            try
            {
                if (SeedSynchronizer.MapGenerated && !EventCalled)
                {
                    MapUtilities.OnMapGenerated();
                    EventManager.Trigger<IHandlerMapGenerated>(new MapGeneratedEvent(SeedSynchronizer.Seed));
                    EventCalled = true;
                }
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(SeedSynchronizer_Update), e);
            }
        }
    }
}
