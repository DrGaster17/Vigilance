using System;
using Harmony;

using Vigilance.API;
using Vigilance.API.Enums;

using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;

using Vigilance.External.Extensions;
using UnityEngine;
using Mirror;
using PlayableScps;

namespace Vigilance.Patches.Events
{
    [HarmonyPatch(typeof(Scp049), nameof(Scp049.BodyCmd_ByteAndGameObject))]
    public static class Scp049_BodyCmd_ByteAndGameObject
    {
        public static bool Prefix(Scp049 __instance, byte num, GameObject go)
        {
            try
            {
                Player myPlayer = PlayersList.GetPlayer(__instance.Hub);
                Player target = PlayersList.GetPlayer(go);

                if (myPlayer == null)
                    return true;

                Scp049InteractEvent interactEvent = new Scp049InteractEvent(myPlayer, __instance, (Scp049InteractionType)num, go, true);
                EventManager.Trigger<IHandlerScp049Interact>(interactEvent);

                if (num == 0)
                {
                    if (!__instance._interactRateLimit.CanExecute(true)
                        || go == null
                        || Vector3.Distance(target.Position, myPlayer.Position) >= PluginManager.Config.Scp049AttackDistance * 1.25f)
                        return false;

                    myPlayer.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(PluginManager.Config.Scp049Damage, myPlayer.Nick + " (" + myPlayer.UserId + ")", DamageTypes.Scp049, myPlayer.PlayerId), target.GameObject, false);
                    myPlayer.ReferenceHub.scpsController.RpcTransmit_Byte(0);

                    return false;
                }
                else
                {
                    if (num != 1)
                    {
                        if (num == 2)
                        {
                            if (!__instance._interactRateLimit.CanExecute(true)
                                || go == null || !go.TryGetComponent(out Ragdoll ragdoll))
                                return false;

                            Player own = null;

                            foreach (Player player in PlayersList.List)
                            {
                                if (player.PlayerId == ragdoll.owner.PlayerId)
                                {
                                    own = player;
                                    break;
                                }
                            }

                            if (own == null)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; no target found", MessageImportance.LessImportant, false);
                                return false;
                            }

                            if (!__instance._recallInProgressServer || own.ReferenceHub.gameObject != __instance._recallObjectServer || __instance._recallProgressServer < 0.85f)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; Debug code: ", MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#1 " + (__instance._recallInProgressServer ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallInProgressServer.ToString())), MessageImportance.LessImportant, true);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#2 " + ((own.ReferenceHub == __instance._recallObjectServer) ? "<color=green>PASSED</color>" : string.Concat(new object[]
                                {
                                    "<color=red>ERROR</color> - ",
                                        own.PlayerId,
                                    "-",
                                        (__instance._recallObjectServer == null) ? "null" : ReferenceHub.GetHub(__instance._recallObjectServer).queryProcessor.PlayerId.ToString()
                                })), MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#3 " + ((__instance._recallProgressServer >= 0.85f) ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallProgressServer)), MessageImportance.LessImportant, true);

                                return false;
                            }

                            if (own.Role != RoleType.Spectator
                                || !PluginManager.Config.CanScp049ReviveOther && ragdoll.owner.DeathCause.GetDamageInfo() != DamageType.Scp049)
                                return false;

                            SCP049RecallEvent ev = new SCP049RecallEvent(myPlayer, ragdoll, true);
                            EventManager.Trigger<IHandlerScp049Recall>(ev);

                            if (!ev.Allow)
                                return false;

                            GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' accepted", MessageImportance.LessImportant, false);
                            RoundSummary.changed_into_zombies++;

                            own.ReferenceHub.characterClassManager.SetClassID(RoleType.Scp0492);
                            own.ReferenceHub.playerStats.Health = own.ReferenceHub.characterClassManager.Classes.Get(RoleType.Scp0492).maxHP;

                            NetworkServer.Destroy(ragdoll.gameObject);

                            __instance._recallInProgressServer = false;
                            __instance._recallObjectServer = null;
                            __instance._recallProgressServer = 0f;
                        }

                        return false;
                    }

                    if (!__instance._interactRateLimit.CanExecute(true))
                        return false;

                    if (go == null)
                        return false;

                    Ragdoll component2 = go.GetComponent<Ragdoll>();

                    if (component2 == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object is not a dead body", MessageImportance.LessImportant, false);
                        return false;
                    }

                    if (!component2.allowRecall)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object can't be recalled", MessageImportance.LessImportant, false);
                        return false;
                    }

                    if (component2.CurrentTime > Scp049.ReviveEligibilityDuration)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object has decayed too far", MessageImportance.LessImportant, false);
                        return false;
                    }

                    Player owner = null;

                    foreach (Player pl in PlayersList.List)
                    {
                        if (pl != null && pl.PlayerId == component2.owner.PlayerId)
                        {
                            owner = pl;
                            break;
                        }
                    }

                    if (owner == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; target not found", MessageImportance.LessImportant, false);
                        return false;
                    }

                    bool flag = false;
                    Rigidbody[] componentsInChildren = component2.GetComponentsInChildren<Rigidbody>();
                    for (int i = 0; i < componentsInChildren.Length; i++)
                    {
                        if (Vector3.Distance(componentsInChildren[i].transform.position, __instance.Hub.PlayerCameraReference.transform.position) <= Scp049.ReviveDistance * 1.3f)
                        {
                            flag = true;
                            owner.ReferenceHub.characterClassManager.NetworkDeathPosition = __instance.Hub.playerMovementSync.RealModelPosition;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP - 049 | Request 'start recalling' rejected; Distance was too great.", MessageImportance.LessImportant, false);
                        return false;
                    }

                    GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' accepted", MessageImportance.LessImportant, false);

                    __instance._recallObjectServer = owner.ReferenceHub.gameObject;
                    __instance._recallProgressServer = 0f;
                    __instance._recallInProgressServer = true;

                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Add(nameof(Scp049.BodyCmd_ByteAndGameObject), e);
                return true;
            }
        }
    }
}