﻿using Harmony;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
    public static class Lift_UseLift
    {
        public static void Prefix(Lift __instance) => __instance.movingSpeed = PluginManager.Config.ElevatorMovingSpeed;
    }
}
