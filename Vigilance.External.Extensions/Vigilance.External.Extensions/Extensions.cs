using Mirror;
using UnityEngine;
using Vigilance.API.Plugins;
using Vigilance.API.Enums;
using System.Collections.Generic;
using System;
using System.Linq;
using Vigilance.API.Configs;
using Vigilance.Commands;
using Vigilance.EventSystem;
using Vigilance.API;
using Interactables.Interobjects.DoorUtils;
using Vigilance.API.Discord.Networking;
using Vigilance.External.Utilities;
using CustomPlayerEffects;
using System.Text.RegularExpressions;
using System.Text;
using NorthwoodLib.Pools;
using System.Reflection;
using Grenades;
using NetworkServer = Mirror.NetworkServer;

namespace Vigilance.External.Extensions
{
	public static class DiscordExtensions
    {
		public static object[] GetParameters(this Player p) => new object[]
		{
			p.BypassMode,
			p.UserIdType.ToString(),
			p.CameraId,
			p.CufferId,
			p.CurrentItem.id.ToString(),
			p.Room == null ? "None" : p.Room.Type.ToString(),
			p.DoNotTrack,
			p.GodMode,
			p.RankColor?.Name,
			p.GroupNode,
			p.RankName,
			p.RemoteAdmin,
			p.Health,
			p.IpAddress,
			p.IsInOverwatch,
			p.IsInPocketDimension,
			p.IsIntercomMuted,
			p.IsMuted,
			p.Items.Select(x => (NetworkItem)(int)x.id),
			p.MaxHealth,
			p.Nick,
			p.NtfUnit,
			p.PlayerId,
			p.Position.x,
			p.Position.y,
			p.Position.z,
			(NetworkRole)(int)p.Role,
			p.Rotations.x,
			p.Rotations.y,
			p.Rotations.z,
			p.Rotations.w,
			p.Stamina,
			p.Team.ToString(),
			p.UserId
		};
    }

	public static class PluginExtensions
    {
		public static void Info(this IPlugin<IConfig> plugin, string msg) => Log.Add(plugin.Name, msg, LogType.Info);
		public static void Error(this IPlugin<IConfig> plugin, string msg) => Log.Add(plugin.Name, msg, LogType.Error);
		public static void Warn(this IPlugin<IConfig> plugin, string msg) => Log.Add(plugin.Name, msg, LogType.Warn);
		public static void Debug(this IPlugin<IConfig> plugin, string msg) => Log.Add(plugin.Name, msg, LogType.Debug);

		public static void AddLog(this IPlugin<IConfig> plugin, string msg) => Log.Add(msg);

		public static void AddCommand(this IPlugin<IConfig> plugin, ICommandHandler commandHandler) => CommandManager.Register(plugin, commandHandler);
		public static void RemoveCommand(this IPlugin<IConfig> plugin, string command) => CommandManager.Remove(plugin, command);
		public static void RemoveCommands(this IPlugin<IConfig> plugin) => CommandManager.RemoveAll(plugin);

		public static void AddEventHandler(this IPlugin<IConfig> plugin, IEventHandler eventHandler) => EventManager.RegisterHandler(plugin, eventHandler);
		public static void RemoveHandler(this IPlugin<IConfig> plugin, IEventHandler eventHandler) => EventManager.UnregisterHandler(plugin, eventHandler);
		public static void RemoveHandlers(this IPlugin<IConfig> plugin) => EventManager.UnregisterHandlers(plugin);

		public static IEnumerable<ICommandHandler> GetCommands(this IPlugin<IConfig> plugin) => CommandManager.GetCommands(plugin);
	}

	public static class ItemExtensions
	{
		public static Pickup Spawn(this ItemType itemType, float durability, Vector3 position, Quaternion rotation = default, int sight = 0, int barrel = 0, int other = 0)
		{
			return LocalComponents.Inventory.SetPickup(itemType, durability, position, rotation, sight, barrel, other);
		}

		public static Pickup Spawn(this Inventory.SyncItemInfo item, Vector3 position, Quaternion rotation = default)
		{
			return item.id.Spawn(item.durability, position, rotation, item.modSight, item.modBarrel, item.modOther);
		}

		public static void SetWeaponAmmo(this Inventory.SyncListItemInfo list, Inventory.SyncItemInfo weapon, int amount) => list.ModifyDuration(list.IndexOf(weapon), amount);
		public static void SetWeaponAmmo(this Player player, Inventory.SyncItemInfo weapon, int amount) => player.ReferenceHub.inventory.items.ModifyDuration(player.ReferenceHub.inventory.items.IndexOf(weapon), amount);

		public static float GetWeaponAmmo(this Inventory.SyncItemInfo weapon) => weapon.durability;

		public static bool IsAmmo(this ItemType item) => item == ItemType.Ammo556 || item == ItemType.Ammo9mm || item == ItemType.Ammo762;
		public static bool IsWeapon(this ItemType type, bool checkMicro = true) => type == ItemType.GunCOM15 || type == ItemType.GunE11SR || type == ItemType.GunLogicer || type == ItemType.GunMP7 || type == ItemType.GunProject90 || type == ItemType.GunUSP || (checkMicro && type == ItemType.MicroHID);
		public static bool IsScp(this ItemType type) => type == ItemType.SCP018 || type == ItemType.SCP500 || type == ItemType.SCP268 || type == ItemType.SCP207;
		public static bool IsThrowable(this ItemType type) => type == ItemType.SCP018 || type == ItemType.GrenadeFrag || type == ItemType.GrenadeFlash;
		public static bool IsMedical(this ItemType type) => type == ItemType.Painkillers || type == ItemType.Medkit || type == ItemType.SCP500 || type == ItemType.Adrenaline;
		public static bool IsUtility(this ItemType type) => type == ItemType.Disarmer || type == ItemType.Flashlight || type == ItemType.Radio || type == ItemType.WeaponManagerTablet;
		public static bool IsKeycard(this ItemType type) => type == ItemType.KeycardChaosInsurgency || type == ItemType.KeycardContainmentEngineer || type == ItemType.KeycardFacilityManager || type == ItemType.KeycardGuard || type == ItemType.KeycardJanitor || type == ItemType.KeycardNTFCommander || type == ItemType.KeycardNTFLieutenant || type == ItemType.KeycardO5 || type == ItemType.KeycardScientist || type == ItemType.KeycardScientistMajor || type == ItemType.KeycardSeniorGuard || type == ItemType.KeycardZoneManager;

		public static SightType GetSight(this Player player, Inventory.SyncItemInfo weapon)
		{
			WeaponManager wmanager = player.ReferenceHub.weaponManager;

			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = wmanager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					try
					{
						string name = wep.mod_sights[weapon.modSight].name.RemoveSpaces();
						return (SightType)Enum.Parse(typeof(SightType), name);
					}
					catch (Exception)
					{
					}
				}
			}

			return SightType.None;
		}

		public static void SetSight(this Player player, Inventory.SyncItemInfo weapon, SightType type)
		{
			WeaponManager weaponManager = player.ReferenceHub.weaponManager;
			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = weaponManager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					string name = type.ToString("g").SplitCamelCase();
					int weaponMod = wep.mod_sights.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
					int weaponId = player.ReferenceHub.inventory.items.FindIndex(s => s == weapon);
					weapon.modSight = weaponMod;
					if (weaponId > -1)
					{
						player.ReferenceHub.inventory.items[weaponId] = weapon;
					}
				}
			}
		}

		public static BarrelType GetBarrel(this Player player, Inventory.SyncItemInfo weapon)
		{
			WeaponManager wmanager = player.ReferenceHub.weaponManager;
			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = wmanager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					try
					{
						string name = wep.mod_barrels[weapon.modBarrel].name.RemoveSpaces();
						return (BarrelType)Enum.Parse(typeof(BarrelType), name);
					}
					catch (Exception)
					{
					}
				}
			}

			return BarrelType.None;
		}

		public static void SetBarrel(this Player player, Inventory.SyncItemInfo weapon, BarrelType type)
		{
			WeaponManager wmanager = player.ReferenceHub.weaponManager;
			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = wmanager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					string name = type.ToString("g").SplitCamelCase();
					int weaponMod = wep.mod_barrels.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
					int weaponId = player.ReferenceHub.inventory.items.FindIndex(s => s == weapon);
					weapon.modBarrel = weaponMod;
					if (weaponId > -1)
					{
						player.ReferenceHub.inventory.items[weaponId] = weapon;
					}
				}
			}
		}

		public static OtherType GetOther(this Player player, Inventory.SyncItemInfo weapon)
		{
			WeaponManager wmanager = player.ReferenceHub.weaponManager;
			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = wmanager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					try
					{
						string name = wep.mod_others[weapon.modOther].name.RemoveSpaces();
						return (OtherType)Enum.Parse(typeof(OtherType), name);
					}
					catch (Exception)
					{
					}
				}
			}

			return OtherType.None;
		}

		public static void SetOther(this Player player, Inventory.SyncItemInfo weapon, OtherType type)
		{
			WeaponManager wmanager = player.ReferenceHub.weaponManager;
			if (weapon.id.IsWeapon())
			{
				WeaponManager.Weapon wep = wmanager.weapons.FirstOrDefault(wp => wp.inventoryID == weapon.id);
				if (wep != null)
				{
					string name = type.ToString("g").SplitCamelCase();
					int weaponMod = wep.mod_others.Select((s, i) => new { s, i }).Where(e => e.s.name == name).Select(e => e.i).FirstOrDefault();
					int weaponId = player.ReferenceHub.inventory.items.FindIndex(s => s == weapon);
					weapon.modOther = weaponMod;
					if (weaponId > -1)
					{
						player.ReferenceHub.inventory.items[weaponId] = weapon;
					}
				}
			}
		}
	}

	public static class RagdollExtensions
	{
		public static Room GetRoom(this Ragdoll ragdoll) => MapUtilities.FindParentRoom(ragdoll.gameObject);
		public static RoleType GetRole(this Ragdoll ragdoll) => CharacterClassManager._staticClasses.FirstOrDefault(role => role.fullName == ragdoll.owner.FullName).roleId;
		public static Player GetOwner(this Ragdoll ragdoll) => PlayersList.GetPlayer(ragdoll.owner.PlayerId);
		public static Player GetKiller(this Ragdoll ragdoll) => ragdoll.owner.DeathCause.IsPlayer ? PlayersList.GetPlayer(ragdoll.owner.DeathCause.RHub) : null;
	}

	public static class ReflectionExtentions
	{
		public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
			type.GetMethod(methodName, flags)?.Invoke(null, param);
		}

		public static void CopyProperties(this object target, object source)
		{
			Type type = target.GetType();
			if (type != source.GetType())
				throw new Exception("Target and source type mismatch!");
			foreach (var sourceProperty in type.GetProperties())
				type.GetProperty(sourceProperty.Name)?.SetValue(target, sourceProperty.GetValue(source, null), null);
		}
	}

	public static class StringExtensions
	{
		public static string Combine(this string[] array)
		{
			string str = string.Join(" ", array);
			int index = str.LastIndexOf(" ");
			if (index > 0 && index > array.Length)
				str.Remove(index, 1);
			return string.Join(" ", array);
		}

		public static string SkipWords(this string[] array, int amount)
		{
			return array.Skip(amount).ToArray().Combine();
		}

		public static string[] SkipCommand(this string[] array)
		{
			return array.Skip(1).ToArray();
		}

		public static bool IsNumber(this char ch) => int.TryParse(ch.ToString(), out int idk);
		public static bool IsNumber(this string str) => int.TryParse(str, out int idk);

		public static int GetDistance(this string firstString, string secondString)
		{
			int n = firstString.Length;
			int m = secondString.Length;
			int[,] d = new int[n + 1, m + 1];

			if (n == 0)
				return m;

			if (m == 0)
				return n;

			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (secondString[j - 1] == firstString[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}

			return d[n, m];
		}

		public static string ToSnakeCase(this string str, bool shouldReplaceSpecialChars = true)
		{
			string snakeCaseString = string.Concat(str.Select((ch, i) => i > 0 && char.IsUpper(ch) ? "_" + ch.ToString() : ch.ToString())).ToLower();
			return shouldReplaceSpecialChars ? Regex.Replace(snakeCaseString, @"[^0-9a-zA-Z_]+", string.Empty) : snakeCaseString;
		}

		public static string ToString<T>(this IEnumerable<T> enumerable, bool showIndex = true)
		{
			StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
			int index = 0;
			stringBuilder.AppendLine(string.Empty);

			foreach (var enumerator in enumerable)
			{
				if (showIndex)
				{
					stringBuilder.Append(index++);
					stringBuilder.Append(' ');
				}

				stringBuilder.AppendLine(enumerator.ToString());
			}

			string result = stringBuilder.ToString();
			StringBuilderPool.Shared.Return(stringBuilder);
			return result;
		}

		public static string RemoveBracketsOnEndOfName(this string name)
		{
			var bracketStart = name.IndexOf('(') - 1;
			if (bracketStart > 0)
				name = name.Remove(bracketStart, name.Length - bracketStart);
			return name;
		}

		public static string GetBefore(this string input, char symbol)
		{
			var start = input.IndexOf(symbol);
			if (start != 0)
				input = input.Substring(0, input.Length - start);
			return input;
		}

		public static string SplitCamelCase(this string input) => Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
		public static string RemoveSpaces(this string input) => Regex.Replace(input, @"\s+", string.Empty);
		public static string GetRawUserId(this string userId) => userId.Substring(0, userId.LastIndexOf('@'));
	}

	public static class RoleExtensions
	{
		public static Color GetColor(this RoleType role) => role == RoleType.None ? Color.white : CharacterClassManager._staticClasses.Get(role).classColor;
		public static Side GetSide(this RoleType role) => role.GetTeam().GetSide();

		public static Side GetSide(this Team team)
		{
			switch (team)
			{
				case Team.SCP:
					return Side.Scp;
				case Team.MTF:
				case Team.RSC:
					return Side.NineTailedFox;
				case Team.CHI:
				case Team.CDP:
					return Side.ChaosInsurgency;
				case Team.TUT:
					return Side.Tutorial;
				default:
					return Side.None;
			}
		}

		public static Team GetTeam(this RoleType roleType)
		{
			switch (roleType)
			{
				case RoleType.ChaosInsurgency:
					return Team.CHI;
				case RoleType.Scientist:
					return Team.RSC;
				case RoleType.ClassD:
					return Team.CDP;
				case RoleType.Scp049:
				case RoleType.Scp93953:
				case RoleType.Scp93989:
				case RoleType.Scp0492:
				case RoleType.Scp079:
				case RoleType.Scp096:
				case RoleType.Scp106:
				case RoleType.Scp173:
					return Team.SCP;
				case RoleType.Spectator:
					return Team.RIP;
				case RoleType.FacilityGuard:
				case RoleType.NtfCadet:
				case RoleType.NtfLieutenant:
				case RoleType.NtfCommander:
				case RoleType.NtfScientist:
					return Team.MTF;
				case RoleType.Tutorial:
					return Team.TUT;
				default:
					return Team.RIP;
			}
		}
	}

	public static class WindowExtensions
    {
		public static readonly Dictionary<int, Window> Windows = new Dictionary<int, Window>();

		public static Window GetWindow(this BreakableWindow window) => Windows.TryGetValue(window.GetInstanceID(), out Window w) ? w : null;

		public static void SetInfo()
        {
			var windows = MapUtilities.FindObjects<BreakableWindow>().ToList();
			var count = windows.Count;

			for (int i = 0; i < count; i++)
            {
				var windowObject = windows[i];
				var window = new Window(windowObject);
				var windowId = windowObject.GetInstanceID();
				Windows.Add(windowId, window);
            }
        }
    }

	public static class GeneratorExtensions
    {
		public static readonly Dictionary<int, Generator> Generators = new Dictionary<int, Generator>();

		public static Generator GetGenerator(this Generator079 gen) => Generators.TryGetValue(gen.GetInstanceID(), out Generator generator) ? generator : null;

		public static void SetInfo()
        {
			var gens = MapUtilities.FindObjects<Generator079>().ToList();
			var count = gens.Count;
			for (int i = 0; i < count; i++)
            {
				var generator = new Generator(gens[i]);
				Generators.Add(gens[i].GetInstanceID(), generator);
            }
        }
    }

	public static class LockerExtensions
    {
		public static readonly Dictionary<int, API.Locker> Lockers = new Dictionary<int, API.Locker>();

		public static API.Locker GetLocker(this Locker locker) => Lockers.TryGetValue(locker.gameObject.GetInstanceID(), out API.Locker l) ? l : null;

		public static void SetInfo()
        {
			var lockers = LockerManager.singleton.lockers.ToList();
			var count = lockers.Count;
			
			for (int i = 0; i < count; i++)
            {
				var locker = lockers[i];
				var lockerObject = new API.Locker(locker, (byte)i);
				var lockerId = locker.gameObject.GetInstanceID();
				Lockers.Add(lockerId, lockerObject);
            }
        }
    }

	public static class LiftExtensions
	{
		public static readonly Dictionary<int, ElevatorType> OrderedElevatorTypes = new Dictionary<int, ElevatorType>();
		public static readonly Dictionary<int, Elevator> Elevators = new Dictionary<int, Elevator>();
		public static ElevatorType Type(this Lift lift) => OrderedElevatorTypes.TryGetValue(lift.GetInstanceID(), out var elevatorType) ? elevatorType : ElevatorType.Unknown;
		public static Elevator GetElevator(this Lift lift) => Elevators.TryGetValue(lift.GetInstanceID(), out Elevator elevator) ? elevator : null;

		public static void SetInfo()
		{
			var lifts = MapUtilities.FindObjects<Lift>().ToList();
			var liftCount = lifts.Count;
			for (int i = 0; i < liftCount; i++)
			{
				var lift = lifts[i];
				var elevator = new Elevator(lift);
				var liftID = lift.GetInstanceID();
				var liftName = string.IsNullOrWhiteSpace(lift.elevatorName) ? lift.elevatorName.RemoveBracketsOnEndOfName() : lift.elevatorName;
				OrderedElevatorTypes.Add(liftID, GetElevatorType(liftName));
				Elevators.Add(lift.GetInstanceID(), elevator);
			}
		}

		private static ElevatorType GetElevatorType(string elevatorName)
		{
			switch (elevatorName)
			{
				case "SCP-049":
					return ElevatorType.Scp049;
				case "GateA":
					return ElevatorType.GateA;
				case "GateB":
					return ElevatorType.GateB;
				case "ElA":
				case "ElA2":
					return ElevatorType.LczA;
				case "ElB":
				case "ElB2":
					return ElevatorType.LczB;
				case "":
					return ElevatorType.Nuke;
				default:
					return ElevatorType.Unknown;
			}
		}
	}

	public static class EffectTypeExtensions
	{
		public static Type Type(this EffectType effect)
		{
			switch (effect)
			{
				case EffectType.Amnesia: return typeof(Amnesia);
				case EffectType.Asphyxiated: return typeof(Asphyxiated);
				case EffectType.Bleeding: return typeof(Bleeding);
				case EffectType.Blinded: return typeof(Blinded);
				case EffectType.Burned: return typeof(Burned);
				case EffectType.Concussed: return typeof(Concussed);
				case EffectType.Corroding: return typeof(Corroding);
				case EffectType.Deafened: return typeof(Deafened);
				case EffectType.Decontaminating: return typeof(Decontaminating);
				case EffectType.Disabled: return typeof(Disabled);
				case EffectType.Ensnared: return typeof(Ensnared);
				case EffectType.Exhausted: return typeof(Exhausted);
				case EffectType.Flashed: return typeof(Flashed);
				case EffectType.Hemorrhage: return typeof(Hemorrhage);
				case EffectType.Invigorated: return typeof(Invigorated);
				case EffectType.Panic: return typeof(Panic);
				case EffectType.Poisoned: return typeof(Poisoned);
				case EffectType.Scp207: return typeof(Scp207);
				case EffectType.Scp268: return typeof(Scp268);
				case EffectType.SinkHole: return typeof(SinkHole);
				case EffectType.Visuals939: return typeof(Visuals939);
				default: return null;
			}
		}
	}

	public static class TeslaExtensions
    {
		public static readonly Dictionary<int, Tesla> Gates = new Dictionary<int, Tesla>();

		public static Tesla GetTesla(this TeslaGate gate) => Gates.TryGetValue(gate.GetInstanceID(), out Tesla tesla) ? tesla : null;

		public static void SetInfo()
        {
			var gates = MapUtilities.FindObjects<TeslaGate>().ToList();
			var count = gates.Count;
			for (int i = 0; i < count; i++)
            {
				var gate = new Tesla(gates[i]);
				Gates.Add(gates[i].GetInstanceID(), gate);
            }
        }
    }

	public static class WorkstationExtensions
    {
		public static readonly Dictionary<int, Workstation> Workstations = new Dictionary<int, Workstation>();

		public static Workstation GetWorkstation(this WorkStation station) => Workstations.TryGetValue(station.GetInstanceID(), out Workstation workstation) ? workstation : null;

		public static void SetInfo()
        {
			var workstations = MapUtilities.FindObjects<WorkStation>().ToList();
			var count = workstations.Count;
			for (int i = 0; i < count; i++)
            {
				var workstation = new Workstation(workstations[i]);
				Workstations.Add(workstations[i].GetInstanceID(), workstation);
            }
        }
    }

	public static class CameraExtensions
	{
		public static readonly Dictionary<int, API.Enums.CameraType> Types = new Dictionary<int, API.Enums.CameraType>();
		public static readonly Dictionary<int, API.Camera> Cameras = new Dictionary<int, API.Camera>();

		public static API.Camera GetCamera(this Camera079 camera) => Cameras.TryGetValue(camera.GetInstanceID(), out API.Camera cam) ? cam : null;
		public static API.Enums.CameraType GetCameraType(this Camera079 camera) => Types.TryGetValue(camera.GetInstanceID(), out API.Enums.CameraType cameraType) ? cameraType : API.Enums.CameraType.Unknown;

		public static void SetInfo()
		{
			var cameras = MapUtilities.FindObjects<Camera079>().ToList();
			var cameraCount = cameras.Count;
			for (int i = 0; i < cameraCount; i++)
			{
				var camera = cameras[i];
				var camObject = new API.Camera(camera);
				var cameraID = camera.GetInstanceID();
				var cameraType = (API.Enums.CameraType)cameraID;
				Cameras.Add(cameraID, camObject);
				Types.Add(cameraID, cameraType);
			}
		}
	}

	public static class DoorExtensions
	{
		public static readonly Dictionary<int, DoorType> Types = new Dictionary<int, DoorType>();
		public static readonly Dictionary<int, API.Door> Doors = new Dictionary<int, API.Door>();
		public static List<DoorType> LightContainmentDoors = new List<DoorType>() { DoorType.Airlocks, DoorType.CheckpointLczA, DoorType.CheckpointLczB, DoorType.LczArmory, DoorType.LczCafe, DoorType.LczWc, DoorType.LightContainmentDoor, DoorType.PrisonDoor, DoorType.Scp012, DoorType.Scp012Bottom, DoorType.Scp012Locker, DoorType.Scp173, DoorType.Scp173Armory, DoorType.Scp173Bottom, DoorType.Scp372, DoorType.Scp914 };
		public static List<DoorType> HeavyContainmentDoors = new List<DoorType>() { DoorType.HczArmory, DoorType.HeavyContainmentDoor, DoorType.HID, DoorType.HIDLeft, DoorType.HIDRight, DoorType.NukeArmory, DoorType.Scp049Armory, DoorType.Scp079First, DoorType.Scp079Second, DoorType.Scp096, DoorType.Scp106Bottom, DoorType.Scp106Primary, DoorType.Scp106Secondary };
		public static List<DoorType> EntranceZoneDoors = new List<DoorType>() { DoorType.CheckpointEntrance, DoorType.EntranceDoor, DoorType.GateA, DoorType.GateB, DoorType.Intercom };
		public static List<DoorType> SurfaceZoneDoors = new List<DoorType>() { DoorType.ContDoor, DoorType.Escape, DoorType.EscapeInner, DoorType.NukeSurface, DoorType.SurfaceGate };

		public static DoorType GetDoorType(this DoorVariant door) => Types.TryGetValue(door.GetInstanceID(), out var doorType) ? doorType : DoorType.Unknown;
		public static API.Door GetDoor(this DoorVariant d) => Doors.TryGetValue(d.GetInstanceID(), out var door) ? door : null;

		public static void SetInfo()
		{
			var doors = MapUtilities.FindObjects<DoorVariant>().ToList();
			var doorCount = doors.Count;
			for (int i = 0; i < doorCount; i++)
			{
				var door = doors[i];
				var doorID = door.GetInstanceID();
				var doorNameTag = door.GetComponent<DoorNametagExtension>();
				var doorName = doorNameTag == null ? door.name.RemoveBracketsOnEndOfName() : doorNameTag.GetName;
				var doorType = GetDoorType(doorName);
				var doorRoom = MapUtilities.FindParentRoom(door.gameObject);
				var doorObject = new API.Door(door, doorRoom, doorType, doorID, doorName);
				Types.Add(doorID, doorType);
				Doors.Add(doorID, doorObject);
				if (doorType == DoorType.CheckpointEntrance) API.Door.CheckpointEZ = doorObject;
				if (doorType == DoorType.CheckpointLczA) API.Door.CheckpointA = doorObject;
				if (doorType == DoorType.CheckpointLczB) API.Door.CheckpointB = doorObject;
				if (doorType == DoorType.Scp173) API.Door.Scp173Gate = doorObject;
			}
		}

		public static DoorType GetDoorType(this string doorName)
		{
			switch (doorName)
			{
				case "012": return DoorType.Scp012;
				case "012_BOTTOM": return DoorType.Scp012Bottom;
				case "012_LOCKER": return DoorType.Scp012Locker;
				case "049_ARMORY": return DoorType.Scp049Armory;
				case "079_FIRST": return DoorType.Scp079First;
				case "079_SECOND": return DoorType.Scp079Second;
				case "096": return DoorType.Scp096;
				case "106_BOTTOM": return DoorType.Scp106Bottom;
				case "106_PRIMARY": return DoorType.Scp106Primary;
				case "106_SECONDARY": return DoorType.Scp106Secondary;
				case "173": return DoorType.Scp173;
				case "173_ARMORY": return DoorType.Scp173Armory;
				case "173_BOTTOM": return DoorType.Scp173Bottom;
				case "372": return DoorType.Scp372;
				case "914": return DoorType.Scp914;
				case "Airlocks": return DoorType.Airlocks;
				case "CHECKPOINT_ENT": return DoorType.CheckpointEntrance;
				case "CHECKPOINT_LCZ_A": return DoorType.CheckpointLczA;
				case "CHECKPOINT_LCZ_B": return DoorType.CheckpointLczB;
				case "ContDoor": return DoorType.ContDoor;
				case "EntrDoor": return DoorType.EntranceDoor;
				case "ESCAPE": return DoorType.Escape;
				case "ESCAPE_INNER": return DoorType.EscapeInner;
				case "GATE_A": return DoorType.GateA;
				case "GATE_B": return DoorType.GateB;
				case "HCZ_ARMORY": return DoorType.HczArmory;
				case "HeavyContainmentDoor": return DoorType.HeavyContainmentDoor;
				case "HID": return DoorType.HID;
				case "HID_LEFT": return DoorType.HIDLeft;
				case "HID_RIGHT": return DoorType.HIDRight;
				case "INTERCOM": return DoorType.Intercom;
				case "LCZ_ARMORY": return DoorType.LczArmory;
				case "LCZ_CAFE": return DoorType.LczCafe;
				case "LCZ_WC": return DoorType.LczWc;
				case "LightContainmentDoor": return DoorType.LightContainmentDoor;
				case "NUKE_ARMORY": return DoorType.NukeArmory;
				case "NUKE_SURFACE": return DoorType.NukeSurface;
				case "PrisonDoor": return DoorType.PrisonDoor;
				case "SURFACE_GATE": return DoorType.SurfaceGate;
				default: return DoorType.Unknown;
			}
		}
	}

	public static class HitInfoExtensions
	{
		public static Player GetAttacker(this PlayerStats.HitInfo hitInfo) => hitInfo.GetDamageType() == DamageTypes.Grenade ? hitInfo.PlayerId.GetPlayer() : hitInfo.RHub.GetPlayer();

		public static Player GetPlayer(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.GetPlayerObject().GetPlayer();
		}

		public static DamageType GetDamageInfo(this PlayerStats.HitInfo info)
		{
			string attacker = info.Attacker.ToUpper();
			if (attacker == "CMDSUICIDE") return DamageType.Suicide;
			if (attacker == "DISCONNECT") return DamageType.Disconnect;
			return info.GetDamageType().AsDamageType();
		}
	}

	public static class DamageExtensions
	{
		public static DamageType AsDamageType(this DamageTypes.DamageType dmgType)
		{
			if (string.IsNullOrEmpty(dmgType.name)) return DamageType.None;
			switch (dmgType.name)
			{
				case "NONE": return DamageType.None;
				case "LURE": return DamageType.Lure;
				case "NUKE": return DamageType.Nuke;
				case "WALL": return DamageType.Wall;
				case "DECONT": return DamageType.Decontamination;
				case "TESLA": return DamageType.Tesla;
				case "FALLDOWN": return DamageType.Falldown;
				case "Flying detection": return DamageType.AntiCheat;
				case "Friendly fire detector": return DamageType.FriendlyFireDetector;
				case "RECONTAINMENT": return DamageType.Recontainment;
				case "BLEEDING": return DamageType.Bleeding;
				case "POISONED": return DamageType.Poison;
				case "ASPHYXIATION": return DamageType.Asphyxiation;
				case "CONTAIN": return DamageType.Containment;
				case "POCKET": return DamageType.PocketDimension;
				case "RAGDOLL-LESS": return DamageType.Ragdolless;
				case "Com15": return DamageType.COM15;
				case "P90": return DamageType.P90;
				case "E11 Standard Rifle": return DamageType.Epsilon11;
				case "MP7": return DamageType.MP7;
				case "Logicier": return DamageType.Logicer;
				case "USP": return DamageType.USP;
				case "MicroHID": return DamageType.MicroHID;
				case "GRENADE": return DamageType.FragGrenade;
				case "SCP-049": return DamageType.Scp049;
				case "SCP-049-2": return DamageType.Scp0492;
				case "SCP-096": return DamageType.Scp096;
				case "SCP-106": return DamageType.Scp106;
				case "SCP-173": return DamageType.Scp173;
				case "SCP-939": return DamageType.Scp939;
				case "SCP-207": return DamageType.Scp207;
				default: return DamageType.None;
			}
		}
	}

	public static class CommandSenderExtensions
	{
		public static void SendRemoteAdminMessage(this CommandSender sender, string command, string message)
		{
			sender.RaReply(command.ToUpper() + "#" + message, true, true, string.Empty);
		}

		public static void SendRemoteAdminMessage(this CommandSender sender, string message)
		{
			sender.SendRemoteAdminMessage(message, "server");
		}

		public static void SendRemoteAdminMessage(this Player player, string message)
		{
			player.ReferenceHub.queryProcessor._sender.RaReply($"SERVER#{message}", true, true, string.Empty);
		}
	}

	public static class PlayerExtensions
	{
		public static Grenade SpawnGrenade(this Player player, Vector3 position, float fuseTime = 3f, GrenadeType grenadeType = GrenadeType.Frag)
		{
			if (player == null)
				player = LocalComponents.Player;
			GrenadeManager component = player.GetComponent<GrenadeManager>();
			Grenade component2 = GameObject.Instantiate(component.availableGrenades[(int)grenadeType].grenadeInstance).GetComponent<Grenade>();
			component2.FullInitData(component, position, Quaternion.Euler(component2.throwStartAngle), player.ReferenceHub.PlayerCameraReference.forward, component2.throwAngularVelocity, Team.RIP);
			component2.NetworkfuseTime = NetworkTime.time + fuseTime;
			NetworkServer.Spawn(component2.gameObject);
			return component2;
		}

		public static Grenade SpawnGrenade(this Player player, Vector3 position, Vector3 velocity, float fuseTime = 3f, GrenadeType grenadeType = GrenadeType.Frag)
		{
			if (player == null)
				player = LocalComponents.Player;
			GrenadeManager component = player.GetComponent<GrenadeManager>();
			Grenade component2 = GameObject.Instantiate(component.availableGrenades[(int)grenadeType].grenadeInstance).GetComponent<Grenade>();
			component2.FullInitData(component, position, Quaternion.Euler(component2.throwStartAngle), velocity, component2.throwAngularVelocity, Team.RIP);
			component2.NetworkfuseTime = NetworkTime.time + fuseTime;
			NetworkServer.Spawn(component2.gameObject);
			return component2;
		}

		public static Player GetPlayer(this CommandSender sender)
		{
			string id = sender.SenderId;
			if (id == "SERVER CONSOLE" || sender.Nickname == "SERVER CONSOLE") return LocalComponents.Player;

			foreach (Player player in PlayersList.List)
			{
				if (player.UserId == sender.SenderId)
					return player;
			}

			return null;
		}

		public static Player GetPlayer(this GameObject gameObject)
		{
			return PlayersList.GetPlayer(gameObject);
		}

		public static Player GetPlayer(this ReferenceHub hub)
		{
			return PlayersList.GetPlayer(hub);
		}

		public static Player GetPlayer(this CharacterClassManager ccm)
		{
			return ccm._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayerStats stats)
		{
			return stats.ccm._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayerInteract interact)
		{
			return interact._hub.GetPlayer();
		}

		public static Player GetPlayer(this Inventory inventory)
		{
			return inventory._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayableScpsController controller)
		{
			return controller._hub.GetPlayer();
		}

		public static Player GetPlayer(this WeaponManager manager)
		{
			return manager._hub.GetPlayer();
		}

		public static List<Player> GetPlayers(this List<ReferenceHub> hubs)
		{
			List<Player> list = new List<Player>();
			foreach (ReferenceHub referenceHub in hubs)
			{
				list.Add(new Player(referenceHub));
			}
			return list;
		}

		public static Player GetPlayer(this int playerId)
		{
			return PlayersList.GetPlayer(playerId);
		}

		public static Player GetPlayer(this string args)
		{
			return PlayersList.GetPlayer(args);
		}

		public static bool Compare(this Player player, Player playerTwo)
		{
			if (player.Nick == playerTwo.Nick && player.PlayerId == playerTwo.PlayerId && player.IpAddress == playerTwo.IpAddress && player.UserId == playerTwo.UserId)
				return true;
			else
				return false;
		}

		public static IEnumerable<Player> GetPlayers(this RoleType role)
		{
			return PlayersList.GetPlayers(role);
		}

		public static IEnumerable<Player> GetPlayers(this Team team)
		{
			return PlayersList.GetPlayers(team);
		}

		public static void RefreshModel(this CharacterClassManager ccm, RoleType classId = RoleType.None)
		{
			ReferenceHub hub = ReferenceHub.LocalHub;
			hub.GetComponent<AnimationController>().OnChangeClass();
			if (ccm.MyModel != null) UnityEngine.Object.Destroy(ccm.MyModel);
			Role role = ccm.Classes.SafeGet((classId < RoleType.Scp173) ? ccm.CurClass : classId);
			if (role.team != Team.RIP)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(role.model_player, ccm.gameObject.transform, true);
				gameObject.transform.localPosition = role.model_offset.position;
				gameObject.transform.localRotation = Quaternion.Euler(role.model_offset.rotation);
				gameObject.transform.localScale = role.model_offset.scale;
				ccm.MyModel = gameObject;
				AnimationController component = hub.GetComponent<AnimationController>();
				if (ccm.MyModel.GetComponent<Animator>() != null) component.animator = ccm.MyModel.GetComponent<Animator>();
				FootstepSync component2 = ccm.GetComponent<FootstepSync>();
				FootstepHandler component3 = ccm.MyModel.GetComponent<FootstepHandler>();
				if (component2 != null) component2.FootstepHandler = component3;
				if (component3 != null)
				{
					component3.FootstepSync = component2;
					component3.AnimationController = component;
				}

				if (ccm.isLocalPlayer)
				{
					if (ccm.MyModel.GetComponent<Renderer>() != null) ccm.MyModel.GetComponent<Renderer>().enabled = false;
					Renderer[] componentsInChildren = ccm.MyModel.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++) componentsInChildren[i].enabled = false;
					foreach (Collider collider in ccm.MyModel.GetComponentsInChildren<Collider>())
					{
						if (collider.name != "LookingTarget") collider.enabled = false;
					}
				}
			}

			ccm.GetComponent<CapsuleCollider>().enabled = (role.team != Team.RIP);
			if (ccm.MyModel != null) ccm.GetComponent<WeaponManager>().hitboxes = ccm.MyModel.GetComponentsInChildren<HitboxIdentity>(true);
		}
	}

	public static class BanDetailsExtensions
	{
		public static Player GetIssuer(this BanDetails banDetails)
		{
			return banDetails.Issuer.GetPlayer();
		}

		public static Player GetPlayer(this BanDetails banDetails)
		{
			return banDetails.Id.GetPlayer();
		}

		public static TimeSpan GetExpirationTime(this BanDetails banDetails)
		{
			return TimeSpan.FromTicks(banDetails.Expires);
		}

		public static string ToString(this TimeSpan expiery)
		{
			DateTime dateTime = new DateTime(expiery.Ticks);
			return dateTime.ToString("dd.MM.yy [HH:mm:ss]");
		}

		public static string ToString(this TimeSpan expiery, TimeSpan issuance)
		{
			TimeSpan time = expiery - issuance;
			int seconds = time.Seconds;
			int minutes = time.Minutes;
			int hours = time.Hours;
			int days = time.Days;
			int months = days / 30;
			if (months > 0) return $" {months} month(s)";
			if (days > 0) return $" {days} day(s)";
			if (hours > 0) return $" {hours} hour(s)";
			if (minutes > 0) return $" {minutes} minute(s)";
			if (seconds > 0) return $" {seconds} second(s)";
			return $"{months} month(s) | {days} day(s) | {hours} hour(s) | {minutes} minute(s) | {seconds} second(s)";
		}

		public static int GetDuration(this BanDetails banDetails)
		{
			return banDetails.GetExpirationTime().Seconds;
		}

		public static TimeSpan GetIssuanceTime(this BanDetails banDetails)
		{
			return TimeSpan.FromTicks(banDetails.IssuanceTime);
		}
	}

	public static class Timer
	{
		public static string TimeToString(DateTime time)
		{
			return time.ToString("F");
		}

		public static string TimeToString(TimeSpan span)
		{
			return TimeToString(ParseDate(span));
		}

		public enum Time
		{
			Second,
			Minute,
			Hour,
			Day,
			Month,
			Year
		}

		public static string ParseString(ulong dur, Time time)
		{
			string str = "";
			if (time == Time.Day)
			{
				if (dur > 1)
					str = "days";
				else
					str = "day";
			}

			if (time == Time.Hour)
			{
				if (dur > 1)
					str = "hours";
				else
					str = "hour";
			}

			if (time == Time.Minute)
			{
				if (dur > 1)
					str = "minutes";
				else
					str = "minute";
			}

			if (time == Time.Month)
			{
				if (dur > 1)
					str = "months";
				else
					str = "month";
			}

			if (time == Time.Second)
			{
				if (dur > 1)
					str = "seconds";
				else
					str = "second";
			}

			if (time == Time.Year)
			{
				if (dur > 1)
					str = "years";
				else
					str = "year";
			}
			return str;
		}

		public static DateTime ParseDate(TimeSpan span) => new DateTime(span.Ticks);

		public static bool TryParse(this string arg, out Time time)
		{
			if (int.TryParse(arg, out int t))
			{
				time = Time.Hour;
				return true;
			}

			foreach (Time e in Enum.GetValues(typeof(Time)).Cast<Time>())
			{
				if (e.ToString().ToLower() == arg.ToLower() || arg.ToLower().Contains(e.ToString().ToLower()))
				{
					time = e;
					return true;
				}
			}

			if (arg.Contains("s"))
			{
				time = Time.Second;
				return true;
			}

			if (arg.Contains("m"))
			{
				time = Time.Minute;
				return true;
			}

			if (arg.Contains("h"))
			{
				time = Time.Hour;
				return true;
			}

			if (arg.Contains("d"))
			{
				time = Time.Day;
				return true;
			}

			if (arg.Contains("M"))
			{
				time = Time.Month;
				return true;
			}

			if (arg.Contains("y"))
			{
				time = Time.Year;
				return true;
			}

			time = Time.Hour;
			return false;
		}
	}

	public static class EnumExtensions
	{
		public static bool IsSteam(this UserIdType idType) => idType != UserIdType.Discord && idType != UserIdType.Unspecified;

		public static KeycardPermissions[] GetPermissions(this string[] array)
		{
			List<KeycardPermissions> perms = new List<KeycardPermissions>();
			foreach (string perm in array)
			{
				if (DoorPermissionUtils.BackwardsCompatibilityPermissions.TryGetValue(perm, out KeycardPermissions value)) perms.Add(value);
			}
			return perms.ToArray();
		}

		public static UserIdType GetIdType(this ulong id)
		{
			if (id.ToString().Length == 17) return UserIdType.Steam;
			if (id.ToString().Length == 18) return UserIdType.Discord;
			return UserIdType.Unspecified;
		}

		public static string GetName(this PrefabType prefab)
		{
			if (prefab == PrefabType.GrenadeFlash) return "Grenade Flash";
			if (prefab == PrefabType.GrenadeFrag) return "Grenade Frag";
			if (prefab == PrefabType.GrenadeScp018) return "Grenade SCP-018";
			if (prefab == PrefabType.Scp096_Ragdoll) return "SCP-096_Ragdoll";
			if (prefab == PrefabType.WorkStation) return "Work Station";
			return prefab.ToString();
		}

		public static GameObject GetObject(this PrefabType prefab)
		{
			foreach (GameObject obj in Mirror.NetworkManager.singleton.spawnPrefabs) if (obj.name == prefab.GetName()) return obj;
			return null;
		}

		public static GameObject Spawn(this PrefabType prefab, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			GameObject toInstantiate = prefab.GetObject();
			GameObject clone = UnityEngine.Object.Instantiate(toInstantiate);
			clone.transform.localScale = scale;
			clone.transform.position = position;
			clone.transform.rotation = rotation;
			NetworkServer.Spawn(clone);
			return clone;
		}

		public static PrefabType GetPrefab(this string str)
		{
			if (int.TryParse(str, out int index))
				return (PrefabType)index;
			if (str == "Player")
				return PrefabType.Player;
			if (str == "PlaybackLobby")
				return PrefabType.PlaybackLobby;
			if (str == "Pickup")
				return PrefabType.Pickup;
			if (str == "Work Station")
				return PrefabType.WorkStation;
			if (str == "Ragdoll_0")
				return PrefabType.Ragdoll_0;
			if (str == "Ragdoll_1")
				return PrefabType.Ragdoll_1;
			if (str == "Ragdoll_3")
				return PrefabType.Ragdoll_3;
			if (str == "Ragdoll_4")
				return PrefabType.Ragdoll_4;
			if (str == "Ragdoll_6")
				return PrefabType.Ragdoll_6;
			if (str == "Ragdoll_7")
				return PrefabType.Ragdoll_7;
			if (str == "Ragdoll_8")
				return PrefabType.Ragdoll_8;
			if (str == "SCP-096_Ragdoll")
				return PrefabType.Scp096_Ragdoll;
			if (str == "Ragdoll_10")
				return PrefabType.Ragdoll_10;
			if (str == "Ragdoll_14")
				return PrefabType.Ragdoll_14;
			if (str == "Ragdoll_16")
				return PrefabType.Ragdoll_16;
			if (str == "Ragdoll_17")
				return PrefabType.Ragdoll_17;
			if (str == "Grenade Flash")
				return PrefabType.GrenadeFlash;
			if (str == "Grenade Frag")
				return PrefabType.GrenadeFrag;
			if (str == "Grenade SCP-018")
				return PrefabType.GrenadeScp018;
			return PrefabType.None;
		}

		public static RoleType GetRole(this string str)
		{
			if (int.TryParse(str, out int id))
				return (RoleType)id;
			str = str.ToLower();
			if (str == "chaosinsurgency")
				return RoleType.ChaosInsurgency;
			if (str == "classd")
				return RoleType.ClassD;
			if (str == "facilityguard")
				return RoleType.FacilityGuard;
			if (str == "none")
				return RoleType.None;
			if (str == "ntfcadet")
				return RoleType.NtfCadet;
			if (str == "ntfcommander")
				return RoleType.NtfCommander;
			if (str == "ntflieutenant")
				return RoleType.NtfLieutenant;
			if (str == "ntfscientist")
				return RoleType.NtfScientist;
			if (str == "scientist")
				return RoleType.Scientist;
			if (str == "scp049")
				return RoleType.Scp049;
			if (str == "scp0492" || str == "zombie")
				return RoleType.Scp0492;
			if (str == "scp079")
				return RoleType.Scp079;
			if (str == "scp096")
				return RoleType.Scp096;
			if (str == "scp106")
				return RoleType.Scp106;
			if (str == "scp173")
				return RoleType.Scp173;
			if (str == "scp93989")
				return RoleType.Scp93989;
			if (str == "scp93989")
				return RoleType.Scp93989;
			if (str == "spectator")
				return RoleType.Spectator;
			if (str == "tutorial")
				return RoleType.Tutorial;
			return RoleType.None;
		}

		public static ItemType GetItem(this string str)
		{
			if (int.TryParse(str, out int id))
				return (ItemType)id;
			str = str.ToLower();
			if (str == "adrenaline")
				return ItemType.Adrenaline;
			if (str == "ammo556")
				return ItemType.Ammo556;
			if (str == "ammo762")
				return ItemType.Ammo762;
			if (str == "ammo9mm")
				return ItemType.Ammo9mm;
			if (str == "coin")
				return ItemType.Coin;
			if (str == "disarmer")
				return ItemType.Disarmer;
			if (str == "flashlight")
				return ItemType.Flashlight;
			if (str == "flash" || str == "grenadeflash")
				return ItemType.GrenadeFlash;
			if (str == "frag" || str == "grenadefrag")
				return ItemType.GrenadeFrag;
			if (str == "guncom15" || str == "com15")
				return ItemType.GunCOM15;
			if (str == "gune11sr" || str == "e11sr" || str == "epsilon11")
				return ItemType.GunE11SR;
			if (str == "gunlogicer" || str == "logicer")
				return ItemType.GunLogicer;
			if (str == "gunmp7" || str == "mp7")
				return ItemType.GunMP7;
			if (str == "gunproject90" || str == "project90" || str == "p90")
				return ItemType.GunProject90;
			if (str == "gunusp" || str == "usp")
				return ItemType.GunUSP;
			if (str == "keycardchaosinsurgency")
				return ItemType.KeycardChaosInsurgency;
			if (str == "keycardcontainmentengineer")
				return ItemType.KeycardContainmentEngineer;
			if (str == "keycardfacilitymanager")
				return ItemType.KeycardFacilityManager;
			if (str == "keycardguard")
				return ItemType.KeycardGuard;
			if (str == "keycardjanitor")
				return ItemType.KeycardJanitor;
			if (str == "keycardntfcommander")
				return ItemType.KeycardNTFCommander;
			if (str == "keycardntflieutenant")
				return ItemType.KeycardNTFLieutenant;
			if (str == "keycardo5")
				return ItemType.KeycardO5;
			if (str == "keycardscientist")
				return ItemType.KeycardScientist;
			if (str == "keycardscientistmajor")
				return ItemType.KeycardScientistMajor;
			if (str == "keycardseniorguard")
				return ItemType.KeycardSeniorGuard;
			if (str == "keycardzonemanager")
				return ItemType.KeycardZoneManager;
			if (str == "medkit")
				return ItemType.Medkit;
			if (str == "microhid")
				return ItemType.MicroHID;
			if (str == "none")
				return ItemType.None;
			if (str == "painkillers")
				return ItemType.Painkillers;
			if (str == "radio")
				return ItemType.Radio;
			if (str == "scp018")
				return ItemType.SCP018;
			if (str == "scp500")
				return ItemType.SCP500;
			if (str == "scp207")
				return ItemType.SCP207;
			if (str == "scp268")
				return ItemType.SCP268;
			if (str == "tablet" || str == "weaponmanagertablet")
				return ItemType.WeaponManagerTablet;
			return ItemType.None;
		}

		public static Team GetTeam(this string str)
		{
			if (int.TryParse(str, out int id))
				return (Team)id;

			str = str.ToLower();
			if (str == "chaosinsurgency")
				return Team.CHI;
			if (str == "classd")
				return Team.CDP;
			if (str == "ntf" || str == "ninetailedfox")
				return Team.MTF;
			if (str == "scientist")
				return Team.RSC;
			if (str == "scp")
				return Team.SCP;
			if (str == "spectator")
				return Team.RIP;
			if (str == "tutorial")
				return Team.TUT;
			return Team.RIP;
		}

		public static AmmoType GetAmmoType(this string str)
		{
			if (int.TryParse(str, out int id))
				return (AmmoType)id;
			if (str == "5mm")
				return AmmoType.Ammo5mm;
			if (str == "7mm")
				return AmmoType.Ammo7mm;
			if (str == "9mm")
				return AmmoType.Ammo9mm;
			return AmmoType.Ammo5mm;
		}

		public static T GetEnum<T>(this string str)
		{
			foreach (T t in EnumUtilities.GetValues<T>())
			{
				if (t.ToString().ToLower() == str.ToLower())
					return t;
			}
			return default;
		}

		public static string AsString(this RoleType role)
		{
			switch (role)
			{
				case RoleType.ChaosInsurgency:
					return "Chaos Insurgent";
				case RoleType.ClassD:
					return "Class-D";
				case RoleType.FacilityGuard:
					return "Facility Guard";
				case RoleType.None:
					return "None";
				case RoleType.NtfCadet:
					return "NTF Cadet";
				case RoleType.NtfCommander:
					return "NTF Commander";
				case RoleType.NtfLieutenant:
					return "NTF Lieutenant";
				case RoleType.NtfScientist:
					return "NTF Scientist";
				case RoleType.Scientist:
					return "Scientist";
				case RoleType.Scp049:
					return "SCP-049";
				case RoleType.Scp0492:
					return "SCP-049-2";
				case RoleType.Scp079:
					return "SCP-079";
				case RoleType.Scp096:
					return "SCP-096";
				case RoleType.Scp106:
					return "SCP-106";
				case RoleType.Scp173:
					return "SCP-173";
				case RoleType.Scp93953:
					return "SCP-939-53";
				case RoleType.Scp93989:
					return "SCP-939-89";
				case RoleType.Spectator:
					return "Spectator";
				case RoleType.Tutorial:
					return "Tutorial";
				default:
					return "Unspecified";
			}
		}

		public static string ToCassie(this RoleType role)
		{
			if (role == RoleType.ChaosInsurgency)
				return "ChaosInsurgency";
			if (role == RoleType.ClassD)
				return "ClassD";
			if (role == RoleType.FacilityGuard)
				return "Facility Guard";
			if (role == RoleType.NtfCadet)
				return "Cadet";
			if (role == RoleType.NtfCommander)
				return "Commander";
			if (role == RoleType.NtfLieutenant)
				return "Lieutenant";
			if (role == RoleType.NtfScientist)
				return "NineTailedFox Scientist";
			if (role == RoleType.Scientist)
				return "Scientist";
			if (role == RoleType.Scp049)
				return "SCP 0 4 9";
			if (role == RoleType.Scp0492)
				return "SCP 0 4 9 . 2";
			if (role == RoleType.Scp079)
				return "SCP 0 7 9";
			if (role == RoleType.Scp096)
				return "SCP 0 9 6";
			if (role == RoleType.Scp106)
				return "SCP 1 0 6";
			if (role == RoleType.Scp173)
				return "SCP 1 7 3";
			if (role == RoleType.Scp93953 || role == RoleType.Scp93989)
				return "SCP 9 3 9";
			return "";
		}

		public static GrenadeType GetGrenadeType(this ItemType item)
		{
			if (item == ItemType.GrenadeFlash)
				return GrenadeType.Flash;
			if (item == ItemType.GrenadeFrag)
				return GrenadeType.Frag;
			if (item == ItemType.SCP018)
				return GrenadeType.Scp018;
			return GrenadeType.None;
		}
	}

	public static class VectorExtensions
	{
		public static string AsString(this Vector3 vec) => $"[X: {vec.x} | Y: {vec.y} | Z: {vec.z}]";
	}

	public static class ConfigExtensions
	{
		public static ItemType GetItem(this YamlConfig cfg, string key) => cfg.GetString(key).GetItem();
		public static RoleType GetRole(this YamlConfig cfg, string key) => cfg.GetString(key).GetRole();
		public static Team GetTeam(this YamlConfig cfg, string key) => cfg.GetString(key).GetTeam();
		public static API.Broadcast GetBroadcast(this YamlConfig cfg, string key) => ParseBroadcast(cfg.GetString(key));

		public static List<ItemType> GetItems(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new List<ItemType>();
			try
			{
				List<ItemType> items = new List<ItemType>();
				foreach (string val in cfg.GetStringList(key))
				{
					items.Add(val.GetItem());
				}
				return items;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<ItemType>();
			}
		}

		public static List<RoleType> GetRoles(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new List<RoleType>();
			try
			{
				List<RoleType> roles = new List<RoleType>();
				foreach (string val in cfg.GetStringList(key))
				{
					roles.Add(val.GetRole());
				}
				return roles;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<RoleType>();
			}
		}

		public static List<Team> GetTeams(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new List<Team>();
			try
			{
				List<Team> teams = new List<Team>();

				foreach (string val in cfg.GetStringList(key))
				{
					teams.Add(val.GetTeam());
				}

				return teams;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<Team>();
			}
		}

		public static Vector3 GetVector(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return Vector3.zero;
			Vector3 vector = Vector3.zero;
			string[] args = cfg.GetString(key).Split('=');
			if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
			{
				vector = new Vector3(x, y, z);
			}
			return vector;
		}

		public static List<Vector3> GetVectorList(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new List<Vector3>();
			List<Vector3> vectors = new List<Vector3>();
			foreach (string val in cfg.GetStringList(key))
			{
				string[] args = val.Split('=');
				if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
				{
					Vector3 vector = new Vector3(x, y, z);
					vectors.Add(vector);
				}
			}
			return vectors;
		}

		public static Dictionary<int, int> GetIntDictionary(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new Dictionary<int, int>();
			Dictionary<string, string> stringDictionary = cfg.GetStringDictionary(key);
			if (stringDictionary.Count == 0)
				return new Dictionary<int, int>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (KeyValuePair<string, string> keyValuePair in stringDictionary)
			{
				if (int.TryParse(keyValuePair.Key, out int k) && int.TryParse(keyValuePair.Value, out int value))
				{
					dictionary.Add(k, value);
				}
			}
			return dictionary;
		}

		public static List<API.Broadcast> GetBroadcasts(this YamlConfig cfg, string key)
		{
			List<API.Broadcast> bcs = new List<API.Broadcast>();
			foreach (string str in cfg.GetStringList(key))
			{
				bcs.Add(ParseBroadcast(str));
			}
			return bcs;
		}

		public static Dictionary<RoleType, ItemType> GetRoleItemDictionary(this YamlConfig cfg, string key)
		{
			Dictionary<RoleType, ItemType> pairs = new Dictionary<RoleType, ItemType>();
			Dictionary<string, string> strs = cfg.GetStringDictionary(key);
			foreach (KeyValuePair<string, string> pair in strs)
			{
				pairs.Add(pair.Key.GetRole(), pair.Value.GetItem());
			}
			return pairs;
		}

		public static Dictionary<RoleType, float> GetRoleIntegerDictionary(this YamlConfig cfg, string key)
		{
			Dictionary<RoleType, float> pairs = new Dictionary<RoleType, float>();
			Dictionary<string, string> strs = cfg.GetStringDictionary(key);
			foreach (KeyValuePair<string, string> pair in strs)
			{
				if (!float.TryParse(pair.Value, out float f))
					continue;
				pairs.Add(pair.Key.GetRole(), f);
			}
			return pairs;
		}

		public static API.Broadcast ParseBroadcast(string arg)
		{
			if (string.IsNullOrEmpty(arg)) return new API.Broadcast(0, "", false);
			if (!arg.Contains("|")) return new API.Broadcast(0, "", false);
			string[] args = arg.Split('|');
			string txt = args[0];
			bool mono = false;
			if (!int.TryParse(args[1], out int duration)) return new API.Broadcast(0, "", false);
			if (args.Length < 3 || !bool.TryParse(args[2], out mono)) mono = false;
			return new API.Broadcast(duration, txt, mono);
		}
	}
}