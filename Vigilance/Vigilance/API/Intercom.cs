using UnityEngine;
using System.Linq;
using Vigilance.Extensions;

namespace Vigilance.API
{
	public static class Intercom
	{
		public static Door Door => Map.Doors.FirstOrDefault(x => x.Type == Enums.DoorType.Intercom);
		public static global::Intercom.State State => global::Intercom.host.Network_state;
		public static bool IsInUse => State == global::Intercom.State.Transmitting || State == global::Intercom.State.TransmittingBypass || State == global::Intercom.State.AdminSpeaking;
		public static bool IsAdminSpeaking => State == global::Intercom.State.AdminSpeaking;
		public static Player Speaker => global::Intercom.host.speaker.GetPlayer();
		public static float RemainingCooldown { get => global::Intercom.host.remainingCooldown; set => global::Intercom.host.remainingCooldown = value; }
		public static float RemainingTime { get => global::Intercom.host.speechRemainingTime; set => global::Intercom.host.speechRemainingTime = value; }
		public static float SpeechTime { get => global::Intercom.host._speechTime; set => global::Intercom.host._speechTime = value; }
		public static string CustomText { get => global::Intercom.host.CustomContent; set => global::Intercom.host.CustomContent = value; }

		public static void Timeout() => global::Intercom.host.speechRemainingTime = -1f;
		public static void ResetCooldown() => global::Intercom.host.remainingCooldown = -1f;

		public static void SetSpeaker(Player player)
		{
			global::Intercom.host.Networkspeaker = player.GameObject;
		}

		public static void SetText(string txt)
		{
			if (string.IsNullOrEmpty(txt)) 
				return;

			global::Intercom.host.CustomContent = txt;
			global::Intercom.host.UpdateText();
		}

		public static void SetContent(global::Intercom singleton, global::Intercom.State state, string content)
		{
			if (state == global::Intercom.State.Restarting) 
				content = content.Replace("%remaining%", Mathf.CeilToInt(singleton.remainingCooldown).ToString()); else content = content.Replace("%time%", Mathf.CeilToInt(singleton.speechRemainingTime).ToString());

			if (!string.IsNullOrEmpty(content))
			{
				singleton.Network_intercomText = content;
				singleton.Network_state = global::Intercom.State.Custom;
			}
			else
			{
				singleton.Network_state = state;
			}
		}
	}
}
