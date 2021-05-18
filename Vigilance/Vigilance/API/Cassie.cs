using Vigilance.Extensions.Rpc;
using Respawning;
using MEC;

namespace Vigilance.API
{
    public static class Cassie
    {
        public static bool IsSpeaking => !NineTailedFoxAnnouncer.singleton.Free;

        public static void MessagePlayer(Player player, string message, bool isHeld = false, bool isNoisy = true) => player.PlayCassieAnnouncement(message, isHeld, isNoisy);
        public static void Message(string message, bool isHeld = false, bool isNoisy = true) => RespawnEffectsController.PlayCassieAnnouncement(message, isHeld, isNoisy);
        public static void GlitchyMessage(string message, float glitchChance, float jamChance) => NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(message, glitchChance, jamChance);
        public static void DelayedMessage(string message, float delay, bool isHeld = false, bool isNoisy = true) => Timing.CallDelayed(delay, () => RespawnEffectsController.PlayCassieAnnouncement(message, isHeld, isNoisy));
        public static void DelayedGlitchyMessage(string message, float delay, float glitchChance, float jamChance) => Timing.CallDelayed(delay, () => NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase(message, glitchChance, jamChance));
        public static float CalculateDuration(string message, bool rawNumber = false) => NineTailedFoxAnnouncer.singleton.CalculateDuration(message, rawNumber);
    }
}