using System;
using Vigilance.External.Utilities;
using Vigilance.External.Extensions;

namespace Vigilance.API
{
	public static class AlphaWarhead
	{
		public static AlphaWarheadController Controller => MapCache.AlphaWarhead;
		public static AlphaWarheadNukesitePanel NukesitePanel => MapCache.Nukesite;
		public static AlphaWarheadOutsitePanel OutsitePanel => MapCache.Outsite;
		public static bool IsDetonated => Controller.detonated;
		public static bool IsLocked { get => Controller._isLocked; set => Controller._isLocked = value; }
		public static bool IsInProgress => Controller.NetworkinProgress;
		public static bool IsKeycardActivated { get => OutsitePanel.NetworkkeycardEntered; set => OutsitePanel.NetworkkeycardEntered = value; }
		public static bool LeverStatus { get => NukesitePanel.Networkenabled; set => NukesitePanel.Networkenabled = value; }
		public static float TimeToDetonation { get => Controller.NetworktimeToDetonation; set => Controller.NetworktimeToDetonation = value; }
		public static float RealDetonationTimer => Controller.RealDetonationTime();
		public static bool CanBeStarted => !Recontainer079.isLocked && ((AlphaWarheadController._resumeScenario == -1 && Math.Abs(Controller.scenarios_start[AlphaWarheadController._startScenario].SumTime() - Controller.timeToDetonation) < 0.0001f) || (AlphaWarheadController._resumeScenario != -1 && Math.Abs(Controller.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime() - Controller.timeToDetonation) < 0.0001f));

		public static void Prepare() => Controller.InstantPrepare();
		public static void Stop() => Controller.CancelDetonation();
		public static void Shake() => Controller.RpcShake(true);
		public static void Detonate()
		{
			Prepare();
			Controller.Detonate();
		}

		public static void Start()
        {
			Prepare();
			Controller.StartDetonation();
        }

		public static bool CanOperatePanel(Player player)
        {
			if (player.PlayerLock)
				return false;

			if (player.BypassMode)
				return true;

			if (PluginManager.Config.PanelPermissions.Contains("SCP_ACCESS") && player.IsSCP)
				return true;

			if (PluginManager.Config.RemoteCard)
            {
				foreach (Inventory.SyncItemInfo itemInfo in player.ReferenceHub.inventory.items)
                {
					if (!itemInfo.id.IsKeycard())
						continue;

					foreach (string perm in player.ReferenceHub.inventory.GetItemByID(itemInfo.id).permissions)
                    {
						if (PluginManager.Config.PanelPermissions.Contains(perm))
							return true;
                    }
                }
            }
			else
            {
				Item inhand = player.ReferenceHub.inventory.GetItemByID(player.ReferenceHub.inventory.curItem);

				if (inhand != null && inhand.id.IsKeycard())
                {
					foreach (string perm in inhand.permissions)
                    {
						if (PluginManager.Config.PanelPermissions.Contains(perm))
							return true;
                    }
                }
				else
                {
					return false;
                }
            }

			return false;
        }
	}
}
