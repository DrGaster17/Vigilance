using System.Collections.Generic;
using Scp914;
using UnityEngine;
using Utils.ConfigHandler;
using Mirror;

namespace Vigilance.API
{
	public static class Scp914
	{
        public static Scp914Machine Controller => Scp914Machine.singleton;
        public static Scp914Knob KnobStatus { get => Controller.NetworkknobState; set => Controller.NetworkknobState = value; }
        public static Dictionary<ItemType, Dictionary<Scp914Knob, ItemType[]>> Recipes { get => Controller.recipesDict; set => Controller.recipesDict = value; }
        public static ConfigEntry<Scp914Mode> ConfigMode { get => Controller.configMode; set => Controller.configMode = value; }
        public static Transform Intake => Scp914Machine.singleton.intake;
        public static Transform Output => Scp914Machine.singleton.output;
        public static bool IsWorking => Scp914Machine.singleton.working;

        public static void Start() => Scp914Machine.singleton.RpcActivate(NetworkTime.time);
    }
}
