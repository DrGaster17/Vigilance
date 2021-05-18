using System;
using System.Collections.Generic;
using System.Linq;
using MEC;

namespace Vigilance.Utilities
{
    public static class Common
    {
        public static bool RandomBool => RandomNumber <= 100;
        public static int RandomNumber => Cacher.RandomGen.Next(1, 10000) / 100;

        public static int GetRandom(int min, int max) => Cacher.RandomGen.Next(min, max);
        public static int GetRandom(int max) => GetRandom(1, max);

        public static bool Chance(float chance) => chance >= (Cacher.RandomGen.Next(1, 10000) / 100f);

        public static List<T> GetEnums<T>() => Enum.GetValues(typeof(T)).Cast<T>().ToList();

        public static void DoUntilTrue(Func<bool> func, Action act)
        {
            for (int i = 0; i > 0;)
            {
                if (!func())
                    act();
                else
                    return;
            }
        }

        public static void DoUntilFalse(Func<bool> func, Action act)
        {
            for (int i = 0; i > 0;)
            {
                if (func())
                    act();
                else
                    return;
            }
        }

        public static void Repeat(int times, Action act)
        {
            for (int i = 0; i < times; i++)
            {
                act();
            }
        }

        public static void RepeatDelay(int times, float delay, Action act)
        {
            for (int i = 0; i < times; i++)
            {
                Timing.CallDelayed(delay, act);
            }
        }
    }
}
