using System.Collections.Generic;
using Vigilance.API.Configs;
using System.ComponentModel;
using Vigilance.API.Enums;

namespace Vigilance.Configs
{
    public class Config : IConfig
    {
		[Description("Enables Vigilance.")]
		public bool IsEnabled { get; set; } = true;

		[Description("Your server will appear as \"Modded\" in the server browser if you enable this.")]
		public bool MarkAsModded { get; set; } = true;

		[Description("Your server will have a invisible Vigilance tag after your server name. This server sorely for tracking how many servers use Vigilance.")]
		public bool ShouldTrack { get; set; } = true;

		[Description("Enables debugging messages, useful if you're having trouble with something.")]
		public bool ShouldDebug { get; set; } = true;

		[Description("All configs will be reloaded when the round restarts if this is set to true.")]
		public bool ShouldReloadConfigsOnRoundRestart { get; set; } = false;

		[Description("Round Lock and Lobby Lock will be disabled when the round restarts if this is set to true.")]
		public bool DisableLocksOnRestart { get; set; } = false;

		[Description("Enables debugging messages for patcher - useful for development & diagnostic purposes only")]
		public bool PatcherEventDebug { get; set; } = true;

		[Description("Spawn all items when the round starts instead of spawning them when a door opens?")]
		public bool SpawnItemsOnStart { get; set; } = false;

		[Description("Should Tesla Gates ignore players with god mode?")]
		public bool TeslaIgnoreGodMode { get; set; } = false;

		[Description("Should Tesla Gates ignore players with noclip?")]
		public bool TeslaIgnoreNoClip { get; set; } = false;

		[Description("Should Tesla Gates ignore players with bypass mode?")]
		public bool TeslaIgnoreBypassMode { get; set; } = false;

		[Description("The text to be displayed while Intercom is in Ready state.")]
		public string IntercomReady { get; set; } = "READY";

		[Description("The text to be displayed while Intercom is in Bypass Transmit state.")]
		public string IntercomBypass { get; set; } = "TRANSMITING ... BYPASS MODE";

		[Description("The text to be displayed while Intercom is in Transmit state.")]
		public string IntercomTransmit { get; set; } = "TRANSMITTING ... %time% SECONDS LEFT";

		[Description("The text to be displayed while Intercom is in Muted state.")]
		public string IntercomMuted { get; set; } = "YOU ARE MUTED";

		[Description("The text to be displayed while Intercom is in Admin Transmit state.")]
		public string IntercomAdmin { get; set; } = "ADMIN IS USING THE INTERCOM";

		[Description("The text to be displayed while Intercom is in Restarting state.")]
		public string IntercomRestart { get; set; } = "RESTARTING ..";

		[Description("The amount of damage to deal to players that get caught by SCP-106.")]
		public float Scp106PocketEnterDamage { get; set; } = 40f;

		[Description("Multiplier for SCP falldamage.")]
		public float ScpFalldamageMultiplier { get; set; } = 1f;

		[Description("Maximal allowed timeout while connecting. Increasing this number may help users with slower PCs / internet connection.")]
		public float MaxAllowedTimeout { get; set; } = 45f;

		[Description("The damage SCP-049 deals to players.")]
		public float Scp049Damage { get; set; } = 4949f;

		[Description("How long does an elevator take to arrive.")]
		public int ElevatorMovingSpeed { get; set; } = 5;

		[Description("Health of all windows in the facility. Set to -1 to make them invicible.")]
		public int WindowHealth { get; set; } = 100;

		[Description("Should players drop their items before being set to spectator?")]
		public bool ShouldDropInventory { get; set; } = true;

		[Description("Should radio battery be drained?")]
		public bool UnlimitedRadioBattery { get; set; } = false;

		[Description("Can SCP-049 revive players that were not killed by SCP-049?")]
		public bool CanScp049ReviveOther { get; set; } = true;

		[Description("Can Tutorial players stop SCP-173 from moving?")]
		public bool CanTutorialBlockScp173 { get; set; } = true;

		[Description("Can Tutorial players trigger SCP-096?")]
		public bool CanTutorialTriggerScp096 { get; set; } = true;

		[Description("Should players get the Amnesia effect when bit by SCP-939?")]
		public bool Scp939Amnesia { get; set; } = true;

		[Description("The damage SCP-939 deals to players.")]
		public float Scp939Damage { get; set; } = 65f;

		[Description("The damage SCP-173 deals to players.")]
		public float Scp173Damage { get; set; } = 999990f;

		[Description("The damage SCP-096 deals to targets.")]
		public float Scp096TargetDamage { get; set; } = 9696f;

		[Description("The damage SCP-096 deals to non-targets.")]
		public float Scp096NonTargetDamage { get; set; } = 35f;

		[Description("Should energy of MicroHID be drained while being used?")]
		public bool UnlimitedMicroEnergy { get; set; } = false;

		[Description("Enables cuffing players that are holding an item in hand.")]
		public bool AllowCuffWhileHolding { get; set; } = false;

		[Description("Enables opening doors that require keycard without the need to take that keycard out of your inventory.")]
		public bool RemoteCard { get; set; } = false;

		[Description("Enables falldamage for SCPs.")]
		public bool ScpFalldamage { get; set; } = false;

		[Description("Enables spawning ragdolls. Disabling this is a big nerf for SCP-049, as it will be unable to revive.")]
		public bool SpawnRagdolls { get; set; } = true;

		[Description("Should players keep the SCP-268 effect after interacting?")]
		public bool ShouldKeepScp268 { get; set; } = false;

		[Description("The maximum amount of SCP-096's shield.")]
		public int Scp096MaxShield { get; set; } = 350;

		[Description("The amount of shield SCP-096 gains for every player that looked at it.")]
		public int Scp096ShieldPerPlayer { get; set; } = 70;

		[Description("The amount of seconds to wait before recharging.")]
		public int Scp096RechargeRate { get; set; } = 5;

		[Description("Should SCP-096 be able to pry gates?")]
		public bool Scp096PryGates { get; set; } = true;

		[Description("Should SCP-096 be able to kill only the players that looked at it?")]
		public bool Scp096CanKillOnlyTargets { get; set; } = false;

		[Description("Should SCP-096 be able to recharge it's shield?")]
		public bool Scp096CanRegen { get; set; } = true;

		[Description("Should SCP-096 be able to see particles on players that looked at it?")]
		public bool Scp096VisionParticles { get; set; } = true;

		[Description("Should SCP-096 be able to break doors?")]
		public bool Scp096DestroyDoors { get; set; } = true;

		[Description("The maximal distance SCP-049 can attack a player from.")]
		public float Scp049AttackDistance { get; set; } = 2.4f;

		[Description("The cooldown SCP-049 gets after a succesfull attack.")]
		public float Scp049KillCooldown { get; set; } = 1.5f;

		[Description("The maximal distance SCP-049 can revive from.")]
		public float Scp049ReviveDistance { get; set; } = 3.5f;

		[Description("The amount of time that has to pass for a specific ragdoll to not be able to be recalled.")]
		public float Scp049ReviveEligibility { get; set; } = 10f;

		[Description("How long does it take SCP-049 to revive a ragdoll.")]
		public float Scp049ReviveDuration { get; set; } = 7f;

		[Description("The cooldown SCP-939 gets after succesfully dealing damage to target.")]
		public float Scp939KillCooldown { get; set; } = 1f;

		[Description("Should all items that the disconnected player dropped be removed?")]
		public bool RemovePickupsOnDisconnect { get; set; } = false;

		[Description("Should all ragdolls that the disconnected player owns be removed?")]
		public bool RemoveRagdollsOnDisconnect { get; set; } = false;

		[Description("Determines whether the cuffed player will be uncuffed when the cuffer disconnects.")]
		public bool UncuffOnDisconnect { get; set; } = true;

		[Description("Determines whether the cuffed player will be uncuffed when the cuffer dies.")]
		public bool UncuffOnDeath { get; set; } = true;

		[Description("Should frag grenades give players that got hit the Burned effect?")]
		public bool FragGrenadeBurnedEffect { get; set; } = true;

		[Description("Should frag grenades give players that got hit the Concussed effect?")]
		public bool FragGrenadeConcussedEffect { get; set; } = true;

		[Description("Should flasg grenades give players that got hit the Flashed effect?")]
		public bool FlashGrenadeFlashedEffect { get; set; } = true;

		[Description("Should flash grenades give players that got hit the Deafened effect?")]
		public bool FlashGrenadeDeafenedEffect { get; set; } = true;

		[Description("The maximal distance between the cuffed player and the cuffer.")]
		public float MaxCuffDistance { get; set; } = 130f;

		[Description("List of roles that will trigger tesla gates.")]
		public List<RoleType> TeslaTriggerableRoles { get; set; } = new List<RoleType>() { RoleType.ChaosInsurgency, RoleType.ClassD, RoleType.FacilityGuard, RoleType.NtfCadet, RoleType.NtfCommander, RoleType.NtfLieutenant, RoleType.NtfScientist, RoleType.Scientist, RoleType.Scp049, RoleType.Scp0492, RoleType.Scp079, RoleType.Scp096, RoleType.Scp106, RoleType.Scp173, RoleType.Scp93953, RoleType.Scp93989, RoleType.Tutorial };

		[Description("List of roles that will be able to use SCP-939's alt voice chat.")]
		public List<RoleType> AltChatAllowedRoles { get; set; } = new List<RoleType>() { RoleType.Scp93953, RoleType.Scp93989 };

		[Description("List of permissions that will be able to open generators.")]
		public List<string> GeneratorPermissions { get; set; } = new List<string>
		{
			{ "ARMORY_LVL_2" }
		};

		[Description("List of permissions that will be able to open the alpha warhead outsite panel")]
		public List<string> PanelPermissions { get; set; } = new List<string>
		{
			{ "CONT_LVL_3" }
		};

		[Description("List of permissions that will be able to open lockers")]
		public List<string> LockerPermissions { get; set; } = new List<string>
		{
			{ "ARMORY_LVL_2" }
		};

		[Description("List of blacklisted seeds.")]
		public List<string> BlacklistedSeeds { get; set; } = new List<string>();

		[Description("Used to configure custom player permissions.")]
		public Dictionary<string, List<CommandPermission>> Permissions { get; set; } = new Dictionary<string, List<CommandPermission>>
		{
			{ "SERVER CONSOLE", new List<CommandPermission> { CommandPermission.All } }
		};

		[Description("A dictionary with users for a commands blacklist.")]
		public Dictionary<string, List<string>> CommandsBlacklist { get; set; } = new Dictionary<string, List<string>>();
	}
}
