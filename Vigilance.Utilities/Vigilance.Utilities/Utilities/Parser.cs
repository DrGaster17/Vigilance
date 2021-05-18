using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

using Vigilance.API;
using Vigilance.Extensions;

using UnityEngine;

namespace Vigilance.Utilities
{
    public static class Parser
    {
        public const float NegativeInfinity = -4.6566467E+11f;

        public static bool TryParse(object input, out float result) => float.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out int result) => int.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out long result) => long.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out byte result) => byte.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out sbyte result) => sbyte.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out double result) => double.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out decimal result) => decimal.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out short result) => short.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out ushort result) => ushort.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out ulong result) => ulong.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out uint result) => uint.TryParse(input.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out result);
        public static bool TryParse(object input, out bool result) => bool.TryParse(input.ToString(), out result);
        public static bool TryParse<TEnum>(object input, out TEnum result) where TEnum : struct => Enum.TryParse(input.ToString(), out result);
        public static bool TryParse<TEnum>(object input, bool ignoreCase, out TEnum result) where TEnum : struct => Enum.TryParse(input.ToString(), ignoreCase, out result);

        /// <summary>
        /// Converts the string representation of a player array to its equivalent.
        /// </summary>
        /// <param name="input">A <see cref="string"/> representating the array to convert.</param>
        /// <param name="result">The converted <see cref="Player"/> <see cref="Array"/></param>
        /// <returns>true if input was converted succesfully, otherwise false.</returns>
        public static bool TryParse(object input, out Player[] result) => TryParse(input, '.', out result);

        /// <summary>
        /// Converts a <see cref="string"/> representation of a <see cref="Vector3"/> to it's equivalent
        /// </summary>
        /// <param name="input">An <see cref="object"/> that can be converted to a <see cref="string"/> representating the object to convert.</param>
        /// <returns>A boolean indicating whether the conversion was succesfull or not.</returns>
        public static bool TryParse(object input, out Vector3 result) => TryParse(input, ',', out result);

        /// <summary>
        /// Converts the string representation of a player array to its equivalent.
        /// </summary>
        /// <param name="input">A <see cref="string"/> representating the array to convert.</param>
        /// <param name="separator">The <see cref="char"/> to split the array by.</param>
        /// <param name="result">The converted <see cref="Player"/> <see cref="Array"/></param>
        /// <returns>true if input was converted succesfully, otherwise false.</returns>
        public static bool TryParse(object input, char separator, out Player[] result)
        {
            string inp = input.ToString();

            if (string.IsNullOrEmpty(inp))
            {
                result = new Player[] { };
                return false;
            }

            if (!inp.Contains(separator))
            {
                result = new Player[] { };
                return false;
            }

            List<Player> players = new List<Player>();

            string[] args = inp.Split(separator);

            foreach (string arg in args)
            {
                Player player = arg.GetPlayer();

                if (player != null)
                    players.Add(player);
            }

            result = players.ToArray();
            return true;
        }

        /// <summary>
        /// Converts a <see cref="string"/> representation of a <see cref="Vector3"/> to it's equivalent
        /// </summary>
        /// <param name="input">An <see cref="object"/> that can be converted to a <see cref="string"/> representating the object to convert.</param>
        /// <param name="separator">A <see cref="char"/> parameter separator (format: X,Y,Z)</param>
        /// <param name="vector">The converted <see cref="Vector3"/>.</param>
        /// <returns>A boolean indicating whether the conversion was succesfull or not.</returns>
        public static bool TryParse(object input, char separator, out Vector3 vector)
        {
            string ip = input.ToString();

            if (!ip.Contains(separator))
            {
                vector = Vector3.zero;
                return false;
            }

            string[] array = ip.Split(separator);

            if (array.Length < 3)
            {
                vector = Vector3.zero;
                return false;
            }

            if (!TryParse(array[0], out float x) || !TryParse(array[1], out float y)
                || !TryParse(array[2], out float z))
            {
                vector = Vector3.zero;
                return false;
            }

            vector = new Vector3(x, y, z);
            return true;
        }
    }
}
