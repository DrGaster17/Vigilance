using System;
using System.Collections.Generic;
using Vigilance.API.Enums;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using UnityEngine;
using Harmony;
using Mirror;
using MapGeneration;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Patching.Patches
{
	[HarmonyPatch(typeof(DoorSpawnpoint), nameof(DoorSpawnpoint.SetupAllDoors))]
	public static class DoorSpawnpoint_SetupAllDoors
	{
		public static DoorVariant LczPrefab;
		public static DoorVariant HczPrefab;
		public static DoorVariant EzPrefab;

		public static readonly List<DoorType> Gates = new List<DoorType>() { DoorType.GateA, DoorType.GateB, DoorType.SurfaceGate, DoorType.Scp079First, DoorType.Scp079Second };

		public static bool Prefix()
		{
			try
			{
				HashSet<DoorSpawnpoint> hashSet = new HashSet<DoorSpawnpoint>();

				while (DoorSpawnpoint.AllInstances.Count > 0)
				{
					DoorSpawnpoint spawn = DoorSpawnpoint.AllInstances.Dequeue();

					if (spawn != null && spawn.gameObject.activeInHierarchy)
					{
						Vector3 position = spawn.transform.position;

						bool flag = false;

						foreach (DoorSpawnpoint doorSpawnpoint2 in hashSet)
						{
							Vector3 position2 = doorSpawnpoint2.transform.position;

							if (Mathf.Abs(position2.y - position.y) <= 2.5f && Mathf.Abs(position2.x - position.x) <= 2.5f && Mathf.Abs(position2.z - position.z) <= 2.5f)
							{
								flag = true;

								doorSpawnpoint2.transform.position = Vector3.Lerp(position2, position, 0.5f);

								if (!string.IsNullOrEmpty(spawn.DesiredNametag))
								{
									doorSpawnpoint2.DesiredNametag = spawn.DesiredNametag;
									break;
								}

								break;
							}
						}

						if (!flag)
						{
							hashSet.Add(spawn);
						}
					}
				}

				PatchData.DoorSpawnpoints = hashSet;

				foreach (DoorSpawnpoint doorSpawnpoint3 in hashSet)
				{
					DoorVariant doorVariant = UnityEngine.Object.Instantiate(doorSpawnpoint3.TargetPrefab, doorSpawnpoint3.transform.position, doorSpawnpoint3.transform.rotation);

					string name = string.IsNullOrEmpty(doorSpawnpoint3.DesiredNametag) ? doorVariant.name.RemoveBracketsOnEndOfName() : doorSpawnpoint3.DesiredNametag;

					if (name.StartsWith("HCZ") && HczPrefab == null)
						HczPrefab = doorSpawnpoint3.TargetPrefab;

					if (name.StartsWith("LCZ") && LczPrefab == null)
						LczPrefab = doorSpawnpoint3.TargetPrefab;

					if (name.StartsWith("EZ") && EzPrefab == null)
						EzPrefab = doorSpawnpoint3.TargetPrefab;

					SpawnDoorEvent ev = new SpawnDoorEvent(doorVariant, doorSpawnpoint3.TargetPrefab, true);
					EventManager.Trigger<IHandlerSpawnDoor>(ev);

					if (!ev.Allow)
						return false;

					if (!string.IsNullOrEmpty(doorSpawnpoint3.DesiredNametag))
					{
						ev.Door.gameObject.AddComponent<DoorNametagExtension>().UpdateName(doorSpawnpoint3.DesiredNametag);
					}

					NetworkServer.Spawn(ev.Door.gameObject);
				}

				return false;
			}
			catch (Exception e)
			{
				Patcher.Log(typeof(DoorSpawnpoint_SetupAllDoors), e);
				return true;
			}
		}
	}
}