using System;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using LightContainmentZoneDecontamination;

namespace Vigilance.External.Patching.Patches
{
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.UpdateSpeaker))]
    public static class DecontaminationController_UpdateSpeaker
    {
        public static void Prefix(DecontaminationController __instance, bool hard)
        {
            try
            {
                AnnounceDecontaminationEvent ev = new AnnounceDecontaminationEvent(__instance._curFunction == DecontaminationController.DecontaminationPhase.PhaseFunction.GloballyAudible, (__instance._nextPhase - 1) < 1 ? 1 : __instance._nextPhase - 1, true);
                EventManager.Trigger<IHandlerAnnounceDecontamination>(ev);

                if (!ev.Allow)
                    return;

                float b = 0f;
                if (__instance._curFunction == DecontaminationController.DecontaminationPhase.PhaseFunction.Final || __instance._curFunction == DecontaminationController.DecontaminationPhase.PhaseFunction.GloballyAudible)
                    b = 1f;

                else if (Mathf.Abs(SpectatorCamera.Singleton.cam.transform.position.y) < 200f)
                    b = 1f;

                __instance.AnnouncementAudioSource.volume = Mathf.Lerp(__instance.AnnouncementAudioSource.volume, b, hard ? 1f : (Time.deltaTime * 4f));
            }
            catch (Exception e)
            {
                Log.Add("LightContainmentZoneDecontamination.DecontaminationController.UpdateSpeaker", e);
            }
        }
    }
}
