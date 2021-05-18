using System;
using System.Collections.Generic;
using UnityEngine;

using Vigilance.Utilities;

namespace Vigilance.Plugins.Commands
{
    public static class StringUtils
    {
        public static bool TryParseVector(string input, out Vector3? vector)
        {
            if (!input.Contains(","))
            {
                vector = null;
                return false;
            }

            string[] array = input.Split(',');

            if (array.Length < 3)
            {
                vector = null;
                return false;
            }

            if (!Parser.TryParse(array[0], out float x) || !Parser.TryParse(array[1], out float y) 
                || !Parser.TryParse(array[2], out float z))
            {
                vector = null;
                return false;
            }

            vector = new Vector3(x, y, z);
            return true;
        }

        public static string BansToString(List<BanDetails> bans)
        {
            string str = $"Removed bans ({bans.Count}):\n";

            for (int i = 0; i < bans.Count; i++)
            {
                var detail = bans[i];

                str += $"-------- {i + 1} --------\n";
                str += $"Nick: {detail.OriginalName}\n";
                str += $"{(detail.Id.Contains("@") ? "UserID" : "IP")}: {detail.Id}\n";
                str += $"Reason: {detail.Reason}\n";
                str += $"Issued by: {detail.Issuer}\n";
                str += $"Issued at: {new DateTime(detail.IssuanceTime).ToString("F")}\n";
                str += $"Expires at: {new DateTime(detail.Expires).ToString("F")}\n";
            }

            return str;
        }
    }
}
