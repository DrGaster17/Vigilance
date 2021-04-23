using LightContainmentZoneDecontamination;

namespace Vigilance.API
{
	public static class Decontamination
	{
		public static DecontaminationController Controller { get; } = DecontaminationController.Singleton;

		public static bool HasBegun => Controller._decontaminationBegun;
		public static bool IsDecontaminated => Controller._stopUpdating;
		public static bool IsDisabled { get => Controller.disableDecontamination; set => Controller.disableDecontamination = value; }
		public static double RoundStartTime => Controller.NetworkRoundStartTime;

		public static void Decontaminate()
        {
			Controller.FinishDecontamination();
			Controller.NetworkRoundStartTime = -1f;
        }
	}
}
