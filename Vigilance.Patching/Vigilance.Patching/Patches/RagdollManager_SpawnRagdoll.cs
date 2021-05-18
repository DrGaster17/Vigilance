using System;
using Vigilance.API;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;
using UnityEngine;
using Harmony;
using Mirror;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
    public static class RagdollManager_SpawnRagdoll
    {
        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, 
            PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId)
        {
            try
            {
                if (!PluginManager.Config.SpawnRagdolls)
                    return false;

                Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
                SpawnRagdollEvent ev = new SpawnRagdollEvent(role, role.model_ragdoll, role.ragdoll_offset, pos, velocity, rot, (RoleType)classId,
                    ragdollInfo, allowRecall, ownerID, ownerNick, playerId, new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId), true);
                EventManager.Trigger<IHandlerSpawnRagdoll>(ev);

                if (!ev.Allow || ev.RagdollModel == null)
                    return false;

                role = __instance.hub.characterClassManager.Classes.SafeGet(ev.RoleId);
                pos = ev.Position;
                rot = ev.Rotation;
                velocity = ev.Velocity;
                classId = (int)ev.RoleId;
                ragdollInfo = ev.HitInfo;
                allowRecall = ev.AllowRecall;
                ownerID = ev.OwnerId;
                ownerNick = ev.OwnerNick;
                playerId = ev.PlayerId;

                Ragdoll.Info info = ev.RagdollInfo;
                info.PlayerId = playerId;
                info.ownerHLAPI_id = ownerID;
                info.Nick = ownerNick;
                info.FullName = role.fullName;
                info.DeathCause = ragdollInfo;
                info.ClassColor = role.classColor;

                GameObject gameObject = UnityEngine.Object.Instantiate(ev.RagdollModel, ev.Position + ev.Offset.position, Quaternion.Euler(ev.Rotation.eulerAngles + ev.Offset.rotation));
                Ragdoll ragdoll = gameObject.GetComponent<Ragdoll>();
                NetworkServer.Spawn(gameObject);

                ragdoll.Networkowner = info;
                ragdoll.NetworkallowRecall = allowRecall;
                ragdoll.NetworkPlayerVelo = velocity;

                Player owner = PlayersList.GetPlayer(playerId);

                if (owner != null)
                    PatchData.AddRagdoll(owner, ragdoll);

                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(RagdollManager_SpawnRagdoll), e);
                return true;
            }
        }
    }
}
