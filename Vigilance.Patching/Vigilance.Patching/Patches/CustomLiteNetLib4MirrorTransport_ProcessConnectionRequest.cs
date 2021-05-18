﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cryptography;
using GameCore;
using Harmony;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using Vigilance.EventSystem;
using Vigilance.EventSystem.EventHandlers;
using Vigilance.EventSystem.Events;

namespace Vigilance.Patching.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest), typeof(ConnectionRequest))]
    public static class CustomLiteNetLib4MirrorTransport_ProcessConnectionRequest
    {
        public static bool Prefix(ref ConnectionRequest request)
        {
            try
            {
                HandleConnection(request);
                return false;
            }
            catch (Exception e)
            {
                Patcher.Log(typeof(CustomLiteNetLib4MirrorTransport_ProcessConnectionRequest), e);
                return true;
            }
        }

        private static void HandleConnection(ConnectionRequest request)
        {
            try
            {
                if (!request.Data.TryGetByte(out byte b) || b >= 2)
                {
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)2);

                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                }
                else if (b == 1)
                {
                    if (CustomLiteNetLib4MirrorTransport.VerificationChallenge != null && request.Data.TryGetString(out string a) && a == CustomLiteNetLib4MirrorTransport.VerificationChallenge)
                    {
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)18);
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.VerificationResponse);

                        request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);

                        CustomLiteNetLib4MirrorTransport.VerificationChallenge = null;
                        CustomLiteNetLib4MirrorTransport.VerificationResponse = null;
                    }
                    else
                    {
                        CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                        if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                        {
                            CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                        }

                        if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                        {
                            ServerConsole.AddLog(string.Format("Invalid verification challenge has been received from endpoint {0}.", request.RemoteEndPoint), ConsoleColor.Gray);
                        }

                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)19);

                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                    }
                }
                else
                {
                    byte cBackwardRevision = 0;

                    if (!request.Data.TryGetByte(out byte cMajor) || !request.Data.TryGetByte(out byte cMinor) || !request.Data.TryGetByte(out byte cRevision) || !request.Data.TryGetBool(out bool flag) || (flag && !request.Data.TryGetByte(out cBackwardRevision)))
                    {
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)3);

                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                    }
                    else if (!GameCore.Version.CompatibilityCheck(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision, cMajor, cMinor, cRevision, flag, cBackwardRevision))
                    {
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)3);

                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                    }
                    else
                    {
                        bool flag2 = request.Data.TryGetInt(out int num);

                        if (!request.Data.TryGetBytesWithLength(out byte[] array))
                        {
                            flag2 = false;
                        }

                        if (!flag2)
                        {
                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)15);

                            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                        }
                        else if (CustomLiteNetLib4MirrorTransport.DelayConnections)
                        {
                            CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();

                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)17);
                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.DelayTime);

                            if (CustomLiteNetLib4MirrorTransport.DelayVolume < 255)
                            {
                                CustomLiteNetLib4MirrorTransport.DelayVolume += 1;
                            }

                            if (CustomLiteNetLib4MirrorTransport.DelayVolume < CustomLiteNetLib4MirrorTransport.DelayVolumeThreshold)
                            {
                                ServerConsole.AddLog(string.Format("Delayed connection incoming from endpoint {0} by {1} seconds.", request.RemoteEndPoint, CustomLiteNetLib4MirrorTransport.DelayTime), ConsoleColor.Gray);
                                request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }
                            else
                            {
                                ServerConsole.AddLog(string.Format("Force delayed connection incoming from endpoint {0} by {1} seconds.", request.RemoteEndPoint, CustomLiteNetLib4MirrorTransport.DelayTime), ConsoleColor.Gray);
                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }
                        }
                        else
                        {
                            if (CustomLiteNetLib4MirrorTransport.UseChallenge)
                            {
                                if (num == 0 || array == null || array.Length == 0)
                                {
                                    if (!CustomLiteNetLib4MirrorTransport.CheckIpRateLimit(request))
                                    {
                                        return;
                                    }

                                    int num2 = 0;
                                    string key = string.Empty;

                                    for (byte b2 = 0; b2 < 3; b2 += 1)
                                    {
                                        num2 = RandomGenerator.GetInt32(false);

                                        if (num2 == 0)
                                        {
                                            num2 = 1;
                                        }

                                        key = request.RemoteEndPoint.Address + "-" + num2;

                                        if (!CustomLiteNetLib4MirrorTransport.Challenges.ContainsKey(key))
                                        {
                                            break;
                                        }

                                        if (b2 == 2)
                                        {
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)4);

                                            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                            ServerConsole.AddLog(string.Format("Failed to generate ID for challenge for incoming connection from endpoint {0}.", request.RemoteEndPoint), ConsoleColor.Gray);
                                            return;
                                        }
                                    }

                                    byte[] bytes = RandomGenerator.GetBytes(CustomLiteNetLib4MirrorTransport.ChallengeInitLen + CustomLiteNetLib4MirrorTransport.ChallengeSecretLen, true);

                                    ServerConsole.AddLog(string.Format("Requested challenge for incoming connection from endpoint {0}.", request.RemoteEndPoint), ConsoleColor.Gray);

                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)13);
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)CustomLiteNetLib4MirrorTransport.ChallengeMode);
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put(num2);

                                    switch (CustomLiteNetLib4MirrorTransport.ChallengeMode)
                                    {
                                        case ChallengeType.MD5:
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(bytes, 0, CustomLiteNetLib4MirrorTransport.ChallengeInitLen);
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.ChallengeSecretLen);
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(Md.Md5(bytes));

                                            CustomLiteNetLib4MirrorTransport.Challenges.Add(key, new PreauthChallengeItem(new ArraySegment<byte>(bytes, CustomLiteNetLib4MirrorTransport.ChallengeInitLen, CustomLiteNetLib4MirrorTransport.ChallengeSecretLen)));
                                            break;
                                        case ChallengeType.SHA1:
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(bytes, 0, CustomLiteNetLib4MirrorTransport.ChallengeInitLen);
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.ChallengeSecretLen);
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(Sha.Sha1(bytes));

                                            CustomLiteNetLib4MirrorTransport.Challenges.Add(key, new PreauthChallengeItem(new ArraySegment<byte>(bytes, CustomLiteNetLib4MirrorTransport.ChallengeInitLen, CustomLiteNetLib4MirrorTransport.ChallengeSecretLen)));
                                            break;
                                        case ChallengeType.Reply:
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(bytes);

                                            CustomLiteNetLib4MirrorTransport.Challenges.Add(key, new PreauthChallengeItem(new ArraySegment<byte>(bytes)));
                                            break;
                                    }

                                    request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                    CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();

                                    return;
                                }
                                else
                                {
                                    string key2 = request.RemoteEndPoint.Address + "-" + num;

                                    if (!CustomLiteNetLib4MirrorTransport.Challenges.ContainsKey(key2))
                                    {
                                        CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                        if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                        {
                                            CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                        }

                                        if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                        {
                                            ServerConsole.AddLog(string.Format("Security challenge response of incoming connection from endpoint {0} has been REJECTED (invalid Challenge ID).", request.RemoteEndPoint), ConsoleColor.Gray);
                                        }

                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)14);

                                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                        return;
                                    }

                                    ArraySegment<byte> validResponse = CustomLiteNetLib4MirrorTransport.Challenges[key2].ValidResponse;

                                    if (!array.SequenceEqual(validResponse))
                                    {
                                        CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                        if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                        {
                                            CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                        }

                                        if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                        {
                                            ServerConsole.AddLog(string.Format("Security challenge response of incoming connection from endpoint {0} has been REJECTED (invalid response).", request.RemoteEndPoint), ConsoleColor.Gray);
                                        }

                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)15);

                                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                        return;
                                    }

                                    CustomLiteNetLib4MirrorTransport.Challenges.Remove(key2);
                                    CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();

                                    ServerConsole.AddLog(string.Format("Security challenge response of incoming connection from endpoint {0} has been accepted.", request.RemoteEndPoint), ConsoleColor.Gray);
                                }
                            }
                            else if (!CustomLiteNetLib4MirrorTransport.CheckIpRateLimit(request))
                            {
                                return;
                            }
                            if (!CharacterClassManager.OnlineMode)
                            {
                                KeyValuePair<BanDetails, BanDetails> keyValuePair = BanHandler.QueryBan(null, request.RemoteEndPoint.Address.ToString());

                                if (keyValuePair.Value != null)
                                {
                                    ServerConsole.AddLog(string.Format("Player tried to connect from banned endpoint {0}.", request.RemoteEndPoint), ConsoleColor.Gray);

                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)6);
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put(keyValuePair.Value.Expires);

                                    NetDataWriter requestWriter = CustomLiteNetLib4MirrorTransport.RequestWriter;
                                    BanDetails value = keyValuePair.Value;

                                    requestWriter.Put(((value != null) ? value.Reason : null) ?? string.Empty);

                                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                }
                                else
                                {
                                    request.Accept();
                                    CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                                }
                            }
                            else if (!request.Data.TryGetString(out string text) || text == string.Empty)
                            {
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)5);

                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }
                            else if (!request.Data.TryGetLong(out long num3) || !request.Data.TryGetByte(out byte b3) || !request.Data.TryGetString(out string text2) || !request.Data.TryGetBytesWithLength(out byte[] signature))
                            {
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)4);

                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                            }
                            else
                            {
                                CentralAuthPreauthFlags flags = (CentralAuthPreauthFlags)b3;
                                try
                                {
                                    if (!ECDSA.VerifyBytes($"{text};{b3};{text2};{num3}", signature, ServerConsole.PublicKey))
                                    {
                                        CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                        if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                        {
                                            CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                        }

                                        if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                        {
                                            ServerConsole.AddLog(string.Format("Player from endpoint {0} sent preauthentication token with invalid digital signature.", request.RemoteEndPoint), ConsoleColor.Gray);
                                        }

                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)2);

                                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                    }
                                    else if (TimeBehaviour.CurrentUnixTimestamp > num3)
                                    {

                                        ServerConsole.AddLog(string.Format("Player from endpoint {0} sent expired preauthentication token.", request.RemoteEndPoint), ConsoleColor.Gray);
                                        ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.", ConsoleColor.Gray);

                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)11);

                                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                    }
                                    else
                                    {
                                        if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
                                        {
                                            if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(text))
                                            {
                                                CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                                if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                                {
                                                    CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                                }

                                                if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                                {
                                                    ServerConsole.AddLog(string.Format("Incoming connection from {0} ({1}) rejected due to exceeding the rate limit.", text, request.RemoteEndPoint), ConsoleColor.Gray);
                                                }

                                                ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Incoming connection from endpoint {0} ({1}) rejected due to exceeding the rate limit.", text, request.RemoteEndPoint), ServerLogs.ServerLogType.RateLimit, false);

                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)12);

                                                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                                return;
                                            }

                                            CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(text);
                                        }
                                        if (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreBans) || !ServerStatic.GetPermissionsHandler().IsVerified)
                                        {
                                            KeyValuePair<BanDetails, BanDetails> keyValuePair2 = BanHandler.QueryBan(text, request.RemoteEndPoint.Address.ToString());

                                            if (keyValuePair2.Key != null || keyValuePair2.Value != null)
                                            {
                                                CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                                if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                                {
                                                    CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                                }

                                                if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                                {
                                                    var yes = $"{((keyValuePair2.Key == null) ? "Player" : "Banned player")} {text} tried to connect from {((keyValuePair2.Value == null) ? string.Empty : "banned ")} endpoint {request.RemoteEndPoint}.";

                                                    ServerConsole.AddLog(yes, ConsoleColor.Gray);
                                                    ServerLogs.AddLog(ServerLogs.Modules.Networking, yes, ServerLogs.ServerLogType.ConnectionUpdate, false);
                                                }

                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)6);

                                                NetDataWriter requestWriter2 = CustomLiteNetLib4MirrorTransport.RequestWriter;
                                                BanDetails key3 = keyValuePair2.Key;

                                                requestWriter2.Put((key3 != null) ? key3.Expires : keyValuePair2.Value.Expires);

                                                NetDataWriter requestWriter3 = CustomLiteNetLib4MirrorTransport.RequestWriter;
                                                BanDetails key4 = keyValuePair2.Key;

                                                string value2;

                                                if ((value2 = (key4 != null) ? key4.Reason : null) == null)
                                                {
                                                    BanDetails value3 = keyValuePair2.Value;
                                                    value2 = ((value3 != null) ? value3.Reason : null) ?? string.Empty;
                                                }

                                                requestWriter3.Put(value2);
                                                request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);

                                                return;
                                            }
                                        }

                                        if (flags.HasFlagFast(CentralAuthPreauthFlags.AuthRejected))
                                        {
                                            ServerConsole.AddLog(string.Format("Player {0} ({1}) kicked due to auth rejection by central server.", text, request.RemoteEndPoint));

                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)20);

                                            request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                        }

                                        if (flags.HasFlagFast(CentralAuthPreauthFlags.GloballyBanned) && (ServerStatic.PermissionsHandler.IsVerified || CustomLiteNetLib4MirrorTransport.UseGlobalBans))
                                        {
                                            ServerConsole.AddLog(string.Format("Player {0} ({1}) kicked due to an active global ban.", text, request.RemoteEndPoint), ConsoleColor.Gray);

                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)8);

                                            request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                        }
                                        else if ((!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist) || !ServerStatic.GetPermissionsHandler().IsVerified) && !WhiteList.IsWhitelisted(text))
                                        {
                                            ServerConsole.AddLog(string.Format("Player {0} tried joined from endpoint {1}, but is not whitelisted.", text, request.RemoteEndPoint), ConsoleColor.Gray);

                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)7);

                                            request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                        }
                                        else if (CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None && (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreGeoblock) || !ServerStatic.PermissionsHandler.BanTeamBypassGeo) && (!CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted || !WhiteList.IsOnWhitelist(text)) && ((CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Whitelist && !CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(text2.ToUpper())) || (CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Blacklist && CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(text2.ToUpper()))))
                                        {
                                            CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                            if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                            {
                                                CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                            }

                                            if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                            {
                                                ServerConsole.AddLog(string.Format("Player {0} ({1}) tried joined from blocked country {2}.", text, request.RemoteEndPoint, text2.ToUpper()), ConsoleColor.Gray);
                                            }

                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)9);

                                            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                        }
                                        else
                                        {
                                            int num4 = CustomNetworkManager.slots;

                                            if (flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) && ServerStatic.PermissionsHandler.BanTeamSlots)
                                            {
                                                num4 = LiteNetLib4MirrorNetworkManager.singleton.maxConnections;
                                            }

                                            else if (ConfigFile.ServerConfig.GetBool("use_reserved_slots", true) && ReservedSlot.HasReservedSlot(text))
                                            {
                                                num4 += CustomNetworkManager.reservedSlots;
                                            }

                                            if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount < num4)
                                            {
                                                if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                                                {
                                                    CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(text);
                                                }
                                                else
                                                {
                                                    CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint, new PreauthItem(text));
                                                }

                                                PreAuthEvent ev = new PreAuthEvent(text, request.Data.Position, b3, text2, request, true);
                                                EventManager.Trigger<IHandlerPreAuth>(ev);

                                                if (ev.Allow)
                                                {
                                                    request.Accept();
                                                    CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                                                    ServerConsole.AddLog($"Player {text} preauthenticated from endpoint {request.RemoteEndPoint}.");
                                                    ServerLogs.AddLog(ServerLogs.Modules.Networking, $"{text} preauthenticated from endpoint {request.RemoteEndPoint}.", ServerLogs.ServerLogType.ConnectionUpdate);
                                                }
                                                else
                                                {
                                                    ServerConsole.AddLog($"Player {text} tried to preauthenticated from endpoint {request.RemoteEndPoint}, but the request has been rejected by a plugin.");
                                                    ServerLogs.AddLog(ServerLogs.Modules.Networking, $"{text} tried to preauthenticated from endpoint {request.RemoteEndPoint}, but the request has been rejected by a plugin.", ServerLogs.ServerLogType.ConnectionUpdate);
                                                }
                                            }
                                            else
                                            {
                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)1);
                                                request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                                    if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                                    {
                                        CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                                    }

                                    if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                                    {
                                        ServerConsole.AddLog(string.Format("Player from endpoint {0} sent an invalid preauthentication token. {1}", request.RemoteEndPoint, ex.Message), ConsoleColor.Gray);
                                    }

                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)2);

                                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                CustomLiteNetLib4MirrorTransport.Rejected += 1U;

                if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                {
                    CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
                }

                if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
                {
                    ServerConsole.AddLog(string.Format("Player from endpoint {0} failed to preauthenticate: {1}", request.RemoteEndPoint, ex2.Message), ConsoleColor.Gray);
                }

                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)4);

                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
            }
        }
    }
}
