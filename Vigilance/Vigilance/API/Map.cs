using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MapGeneration;
using Vigilance.Extensions;
using Vigilance.Utilities;

namespace Vigilance.API
{
	public static class Map
	{
		public static IEnumerable<Pickup> Pickups => MapUtilities.FindObjects<Pickup>();
		public static IEnumerable<Ragdoll> Ragdolls => MapUtilities.FindObjects<Ragdoll>();

		public static IEnumerable<Room> Rooms => MapCache.Rooms;
		public static IEnumerable<Door> Doors => DoorExtensions.Doors.Values;
		public static IEnumerable<Workstation> Workstations => WorkstationExtensions.Workstations.Values;
		public static IEnumerable<FlickerableLight> FlickerableLights => MapCache.Lights;
		public static IEnumerable<FlickerableLightController> LightControllers => MapCache.Controllers;
		public static IEnumerable<Camera> Cameras => CameraExtensions.Cameras.Values;
		public static IEnumerable<Elevator> Elevators => LiftExtensions.Elevators.Values;
		public static IEnumerable<Tesla> TeslaGates => TeslaExtensions.Gates.Values;
		public static IEnumerable<GameObject> PdExits => MapCache.PdExits;
		public static IEnumerable<Generator> Generators => GeneratorExtensions.Generators.Values;
		public static IEnumerable<Locker> Lockers => LockerExtensions.Lockers.Values;
		public static IEnumerable<Window> Windows => WindowExtensions.Windows.Values;

		public static Generator MainGenerator => Generators.First();
		public static SeedSynchronizer SeedSynchronizer => CompCache.SeedSync;
		public static LureSubjectContainer LureSubjectContainer => MapUtilities.Find<LureSubjectContainer>();
		public static OneOhSixContainer OneOhSixContainer => MapUtilities.Find<OneOhSixContainer>();

		public static int MapSeed => SeedSynchronizer.Seed;
		public static int ActivatedGenerators => Generator079.mainGenerator.totalVoltage;

		public static void TurnOffLights(float time = 9999f, bool onlyHeavy = false) => MainGenerator.Overcharge(time, onlyHeavy);
		public static void Broadcast(string message, int duration, bool mono = false) => CompCache.Broadcast.RpcAddElement(message, (ushort)duration, mono ? global::Broadcast.BroadcastFlags.Monospaced : global::Broadcast.BroadcastFlags.Normal);
		public static void Broadcast(Broadcast bc) => Broadcast(bc.Message, bc.Duration, bc.Monospaced);
		public static void ClearBroadcasts() => CompCache.Broadcast.RpcClearElements();
		public static void ShowHint(Broadcast bc) => ShowHint(bc.Message, bc.Duration);
		public static void ShowHint(string message, float duration) => PlayersList.List.ForEach((x) => x.ShowHint(message, duration));
		public static Vector3 GetRandomSpawnpoint(RoleType role) => MapUtilities.GetRandomSpawnpoint(role);
		public static Pickup SpawnItem(ItemType itemType, Vector3 position, Quaternion rotation = default) => MapUtilities.SpawnItem(itemType, position, rotation);
		public static Pickup SpawnItem(Inventory.SyncItemInfo item, Vector3 position, Quaternion rotation = default) => MapUtilities.SpawnItem(item, position, rotation);
	}
}