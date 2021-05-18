using System;
using System.IO;

using Vigilance.API;
using Vigilance.API.Enums;
using Vigilance.Extensions;

namespace Vigilance.Utilities
{
    public static class ServerUtilities
    {
        public static void IssueOfflineBan(Timer.Time type, int duration, string userId, string issuer, string reason)
        {
            BanHandler.BanType banType = BanHandler.BanType.IP;

            if (ulong.TryParse(userId, out ulong id))
                banType = BanHandler.BanType.UserId;

            if (banType != BanHandler.BanType.IP)
            {
                UserIdType idType = id.ToString().Length == 17 ? UserIdType.Steam : UserIdType.Discord;
                userId = $"{id}@{idType.ToString().ToLower()}";
            }

            Log.Debug("SERVER", $"Issuing offline ban\nDuration: {duration} {type}\nUserID: {userId}\nIssuer: {issuer}\nReason: \"{reason}\"");

            switch (type)
            {
                case Timer.Time.Second:
                    {
                        long ticks = DateTime.Now.AddSeconds(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Minute:
                    {
                        long ticks = DateTime.UtcNow.AddMinutes(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Hour:
                    {
                        long ticks = DateTime.UtcNow.AddHours(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Day:
                    {
                        long ticks = DateTime.UtcNow.AddDays(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Month:
                    {
                        long ticks = DateTime.UtcNow.AddMonths(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                case Timer.Time.Year:
                    {
                        long ticks = DateTime.UtcNow.AddYears(duration).Ticks;
                        Server.IssueBan(ticks, userId, issuer, "Offline Player", reason, banType);
                        break;
                    }
                default:
                    return;
            }
        }

        public static bool AddReservedSlot(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || userId.StartsWith("#"))
                    return false;

                if (!userId.Contains("@"))
                {
                    UserIdType userIdType = userId.Length == 17 ? UserIdType.Steam : UserIdType.Discord;

                    if (userIdType == UserIdType.Unspecified)
                        return false;

                    userId += $"@{userIdType.ToString().ToLower()}";
                }

                if (!System.IO.File.Exists(Directories.ReservedSlotsFile))
                    System.IO.File.Create(Directories.ReservedSlotsFile).Close();

                using (StreamWriter writer = new StreamWriter(Directories.ReservedSlotsFile, true))
                {
                    writer.WriteLine("");
                    writer.WriteLine(userId);
                    writer.Flush();
                    writer.Close();
                    FileManager.RemoveEmptyLines(Directories.ReservedSlotsFile);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Add(nameof(AddReservedSlot), e);
                return false;
            }
        }
    }
}
