using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mirror;
using Respawning;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Extensions.Rpc
{
    public static class MirrorExtensions
    {
        private static readonly Dictionary<Type, MethodInfo> WriterExtensionsValue = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<string, ulong> SyncVarDirtyBitsValue = new Dictionary<string, ulong>();
        private static readonly ReadOnlyDictionary<Type, MethodInfo> ReadOnlyWriterExtensionsValue = new ReadOnlyDictionary<Type, MethodInfo>(WriterExtensionsValue);
        private static readonly ReadOnlyDictionary<string, ulong> ReadOnlySyncVarDirtyBitsValue = new ReadOnlyDictionary<string, ulong>(SyncVarDirtyBitsValue);

        private static MethodInfo setDirtyBitsMethodInfoValue = null;
        private static MethodInfo sendSpawnMessageMethodInfoValue = null;

        public static ReadOnlyDictionary<Type, MethodInfo> WriterExtensions
        {
            get
            {
                if (WriterExtensionsValue.Count == 0)
                {
                    foreach (var method in typeof(NetworkWriterExtensions).GetMethods().Where(x => !x.IsGenericMethod && x.GetParameters()?.Length == 2))
                        WriterExtensionsValue.Add(method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType, method);

                    foreach (var serializer in typeof(ServerConsole).Assembly.GetTypes().Where(x => x.Name.EndsWith("Serializer")))
                    {
                        foreach (var method in serializer.GetMethods().Where(x => x.ReturnType == typeof(void) && x.Name.StartsWith("Write")))
                            WriterExtensionsValue.Add(method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType, method);
                    }
                }

                return ReadOnlyWriterExtensionsValue;
            }
        }

        public static ReadOnlyDictionary<string, ulong> SyncVarDirtyBits
        {
            get
            {
                if (SyncVarDirtyBitsValue.Count == 0)
                {
                    foreach (var property in typeof(ServerConsole).Assembly.GetTypes().SelectMany(x => x.GetProperties()).Where(m => m.Name.StartsWith("Network")))
                    {
                        var bytecodes = property.GetSetMethod().GetMethodBody().GetILAsByteArray();

                        if (!SyncVarDirtyBitsValue.ContainsKey($"{property.DeclaringType.Name}.{property.Name}"))
                            SyncVarDirtyBitsValue.Add($"{property.DeclaringType.Name}.{property.Name}", bytecodes[bytecodes.LastIndexOf((byte)OpCodes.Ldc_I8.Value) + 1]);
                    }
                }

                return ReadOnlySyncVarDirtyBitsValue;
            }
        }

        public static MethodInfo SetDirtyBitsMethodInfo
        {
            get
            {
                if (setDirtyBitsMethodInfoValue == null)
                {
                    setDirtyBitsMethodInfoValue = typeof(NetworkBehaviour).GetMethod(nameof(NetworkBehaviour.SetDirtyBit));
                }

                return setDirtyBitsMethodInfoValue;
            }
        }

        public static MethodInfo SendSpawnMessageMethodInfo
        {
            get
            {
                if (sendSpawnMessageMethodInfoValue == null)
                {
                    sendSpawnMessageMethodInfoValue = typeof(NetworkServer).GetMethod("SendSpawnMessage", BindingFlags.NonPublic | BindingFlags.Static);
                }

                return sendSpawnMessageMethodInfoValue;
            }
        }

        public static void SendFakeSyncVar(this Player target, NetworkIdentity behaviorOwner, Type targetType, string propertyName, object value)
        {
            Action<NetworkWriter> customSyncVarGenerator = (targetWriter) =>
            {
                targetWriter.WritePackedUInt64(SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"]);
                WriterExtensions[value.GetType()]?.Invoke(null, new object[] { targetWriter, value });
            };

            NetworkWriter writer = NetworkWriterPool.GetWriter();
            NetworkWriter writer2 = NetworkWriterPool.GetWriter();
            MakeCustomSyncWriter(behaviorOwner, targetType, null, customSyncVarGenerator, writer, writer2);
            NetworkServer.SendToClientOfPlayer(target.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
            NetworkWriterPool.Recycle(writer);
            NetworkWriterPool.Recycle(writer2);
        }

        public static void ResyncSyncVar(this NetworkIdentity behaviorOwner, Type targetType, string propertyName) => SetDirtyBitsMethodInfo.Invoke(behaviorOwner.gameObject.GetComponent(targetType), new object[] { SyncVarDirtyBits[$"{targetType.Name}.{propertyName}"] });
        public static void SendFakeTargetRpc(this Player target, NetworkIdentity behaviorOwner, Type targetType, string rpcName, params object[] values)
        {
            NetworkWriter writer = NetworkWriterPool.GetWriter();

            foreach (var value in values)
                WriterExtensions[value.GetType()].Invoke(null, new object[] { writer, value });

            var msg = new RpcMessage
            {
                netId = behaviorOwner.netId,
                componentIndex = GetComponentIndex(behaviorOwner, targetType),
                functionHash = (targetType.FullName.GetStableHashCode() * 503) + rpcName.GetStableHashCode(),
                payload = writer.ToArraySegment(),
            };

            target.Connection.Send(msg, 0);
            NetworkWriterPool.Recycle(writer);
        }

        public static void SendFakeSyncObject(this Player target, NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customAction)
        {
            NetworkWriter writer = NetworkWriterPool.GetWriter();
            NetworkWriter writer2 = NetworkWriterPool.GetWriter();
            MakeCustomSyncWriter(behaviorOwner, targetType, customAction, null, writer, writer2);
            NetworkServer.SendToClientOfPlayer(target.ReferenceHub.networkIdentity, new UpdateVarsMessage() { netId = behaviorOwner.netId, payload = writer.ToArraySegment() });
            NetworkWriterPool.Recycle(writer);
            NetworkWriterPool.Recycle(writer2);
        }

        public static void EditNetworkObject(NetworkIdentity identity, Action<NetworkIdentity> customAction)
        {
            customAction.Invoke(identity);

            ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage
            {
                netId = identity.netId,
            };

            foreach (var ply in PlayersList.List)
            {
                ply.Connection.Send(objectDestroyMessage, 0);
                SendSpawnMessageMethodInfo.Invoke(null, new object[] { identity, ply.Connection });
            }
        }

        private static int GetComponentIndex(NetworkIdentity identity, Type type)
        {
            return Array.FindIndex(identity.NetworkBehaviours, (x) => x.GetType() == type);
        }

        private static void MakeCustomSyncWriter(NetworkIdentity behaviorOwner, Type targetType, Action<NetworkWriter> customSyncObject, Action<NetworkWriter> customSyncVar, NetworkWriter owner, NetworkWriter observer)
        {
            ulong dirty = 0ul;
            ulong dirty_o = 0ul;
            NetworkBehaviour behaviour = null;
            for (int i = 0; i < behaviorOwner.NetworkBehaviours.Length; i++)
            {
                behaviour = behaviorOwner.NetworkBehaviours[i];
                if (behaviour.GetType() == targetType)
                {
                    dirty |= 1UL << i;
                    if (behaviour.syncMode == SyncMode.Observers)
                        dirty_o |= 1UL << i;
                }
            }

            owner.WritePackedUInt64(dirty);
            observer.WritePackedUInt64(dirty & dirty_o);
            int position = owner.Position;
            owner.WriteInt32(0);
            int position2 = owner.Position;
            if (customSyncObject != null)
                customSyncObject.Invoke(owner);
            else
                behaviour.SerializeObjectsDelta(owner);
            customSyncVar?.Invoke(owner);
            int position3 = owner.Position;
            owner.Position = position;
            owner.WriteInt32(position3 - position2);
            owner.Position = position3;

            if (dirty_o != 0ul)
            {
                ArraySegment<byte> arraySegment = owner.ToArraySegment();
                observer.WriteBytes(arraySegment.Array, position, owner.Position - position);
            }
        }
    }

    public static class RpcExtensions
    {
        public static void Shake(this Player player) => player.SendFakeTargetRpc(AlphaWarheadController.Host.netIdentity, typeof(AlphaWarheadController), nameof(AlphaWarheadController.RpcShake), false);
        public static void PlayBeepSound(this Player player) => player.SendFakeTargetRpc(ReferenceHub.HostHub.networkIdentity, typeof(AmbientSoundPlayer), nameof(AmbientSoundPlayer.RpcPlaySound), 7);
        public static void SetPlayerInfoForTargetOnly(this Player player, Player target, string info) => player.SendFakeSyncVar(target.ReferenceHub.networkIdentity, typeof(NicknameSync), nameof(NicknameSync.Network_customPlayerInfoString), info);

        public static void ChangeAppearance(this Player player, RoleType type)
        {
            foreach (var target in PlayersList.List.Where(x => x != player))
                target.SendFakeSyncVar(player.ReferenceHub.networkIdentity, typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), (sbyte)type);
        }

		public static void ChangeWalkingSpeed(this Player player, float multiplier, bool useCap = true)
		{
			if (useCap)
				multiplier = Mathf.Clamp(multiplier, -2f, 2f);

			MirrorExtensions.SendFakeSyncVar(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), nameof(ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier), multiplier);
		}

		public static void ChangeRunningSpeed(this Player player, float multiplier, bool useCap = true)
		{
			if (useCap)
				multiplier = Mathf.Clamp(multiplier, -1.4f, 1.4f);

			MirrorExtensions.SendFakeSyncVar(player, ServerConfigSynchronizer.Singleton.netIdentity, typeof(ServerConfigSynchronizer), nameof(ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier), multiplier);
		}

		public static void RpcPlaySound(this Player player, int soundId)
		{
			player.SendFakeTargetRpc(ReferenceHub.HostHub.networkIdentity, typeof(AmbientSoundPlayer), nameof(AmbientSoundPlayer.RpcPlaySound), soundId);
		}

		public static void RpcBlood(this Player player, Vector3 pos, int type, float f)
		{
			player.SendFakeTargetRpc(ReferenceHub.HostHub.networkIdentity, typeof(CharacterClassManager), nameof(CharacterClassManager.RpcPlaceBlood), pos, type, f);
		}

		public static void RpcEscapeMessage(this Player player, bool isClassD, bool changeTeam)
		{
			player.SendFakeTargetRpc(ReferenceHub.HostHub.networkIdentity, typeof(Escape), nameof(Escape.TargetShowEscapeMessage), isClassD, changeTeam);
		}

		public static void RpcFalldamageSound(this Player player)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(FallDamage), nameof(FallDamage.RpcDoSound));
		}

		public static void RpcFlickerLights(this Player player, float duration)
		{
			player.SendFakeTargetRpc(ReferenceHub.HostHub.networkIdentity, typeof(FlickerableLightController), nameof(FlickerableLightController.RpcFlickerLights), duration);
		}

		public static void RpcIntercomSound(this Player player, bool start, int transmitterID)
		{
			player.SendFakeTargetRpc(Intercom.host.netIdentity, typeof(Intercom), nameof(Intercom.RpcPlaySound), start, transmitterID);
		}

		public static void RpcLiftMusic(this Player player)
		{
			player.SendFakeTargetRpc(player.Connection.identity, typeof(Lift), nameof(Lift.RpcPlayMusic));
		}

		public static void RpcLockerSound(this Player player, int chamber, int locker, bool open)
		{
			player.SendFakeTargetRpc(LockerManager.singleton.netIdentity, typeof(LockerManager), nameof(LockerManager.RpcDoSound), chamber, locker, open);
		}

		public static void RpcHidHitmarker(this Player player, bool achievement)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(MicroHID), nameof(MicroHID.TargetSendHitmarker), achievement);
		}

		public static void RpcSinkholeSlow(this Player player, float slowAmount)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(PlayerMovementSync), nameof(PlayerMovementSync.TargetForceUpdateSinkholeSlow), slowAmount);
		}

		public static void RpcFastRestart(this Player player)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(PlayerStats), nameof(PlayerStats.RpcFastRestart));
		}

		public static void RpcRoundRestart(this Player player, float connectionDelay, bool reconnect)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(PlayerStats), nameof(PlayerStats.RpcRoundrestart), connectionDelay, reconnect);
		}

		public static void Rpc0492DamageAnimation(this Player player)
		{
			player.SendFakeTargetRpc(player.ConnectionIdentity, typeof(Scp049_2PlayerScript), nameof(Scp049_2PlayerScript.RpcShootAnim));
		}

		public static void PlayCassieAnnouncement(this Player player, string words, bool makeHold = false, bool makeNoise = true) => player.SendFakeTargetRpc(RespawnEffectsController.AllControllers.First().netIdentity, typeof(RespawnEffectsController), nameof(RespawnEffectsController.RpcCassieAnnouncement), words, makeHold, makeNoise);
    }
}