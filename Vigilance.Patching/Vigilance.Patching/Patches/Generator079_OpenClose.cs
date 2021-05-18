using System;
using System.Linq;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using Vigilance.Extensions;
using UnityEngine;
using Harmony;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(Generator079), nameof(Generator079.OpenClose))]
    public static class Generator079_OpenClose
    {
        public static bool Prefix(Generator079 __instance, GameObject person)
        {
            try
            {
				if (__instance._doorAnimationCooldown > 0f || __instance._deniedCooldown > 0f)
					return false;

				Player player = PlayersList.GetPlayer(person);

				if (player == null || !GeneratorExtensions.Generators.TryGetValue(__instance.GetInstanceID(), out Generator generator))
					return true;

				if (player.PlayerLock || generator.DisallowedPlayers.Contains(player))
					return false;

				if (__instance.isDoorUnlocked)
				{
					if (__instance.isDoorOpen)
					{
						GeneratorCloseEvent ev = new GeneratorCloseEvent(generator, player, true);
						EventManager.Trigger<IHandlerGeneratorClose>(ev);

						if (!ev.Allow)
							return false;
					}
					else
					{
						GeneratorOpenEvent ev = new GeneratorOpenEvent(generator, player, true);
						EventManager.Trigger<IHandlerGeneratorOpen>(ev);

						if (!ev.Allow)
							return false;
					}

					__instance._doorAnimationCooldown = 1.5f;
					__instance.NetworkisDoorOpen = !__instance.isDoorOpen;

					__instance.RpcDoSound(__instance.isDoorOpen);

					return false;
				}

				if (generator.CanUnlock(player))
				{
					Patcher.Log(typeof(Generator079_OpenClose), $"Generator Permission check succeeded: {player.Nick} - {player.BypassMode} - {player.Items.Where(x => x.id.IsKeycard()).ToString(true)}");

					GeneratorUnlockEvent gEv = new GeneratorUnlockEvent(generator, player, true);
					EventManager.Trigger<IHandlerGeneratorUnlock>(gEv);

					if (!gEv.Allow)
						return false;

					__instance.NetworkisDoorUnlocked = true;
					__instance._doorAnimationCooldown = 0.5f;
					return false;
				}

				__instance.RpcDenied();

				return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(Generator079_OpenClose), e);
                return true;
            }
        }
    }
}
