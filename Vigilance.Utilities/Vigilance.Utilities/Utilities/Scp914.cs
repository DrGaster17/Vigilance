using Scp914;
using System.Collections.Generic;

namespace Vigilance.Utilities
{
    public static class Scp914Utilities
    {
        public static ItemType UpgradeItemId(ItemType input)
        {
            ItemType[] array = UpgradeItemIds(input);

            if (array == null)
                return input;

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static ItemType[] UpgradeItemIds(ItemType input)
        {
            Dictionary<Scp914Knob, ItemType[]> dictionary;
            ItemType[] result;

            if (!API.Scp914.Recipes.TryGetValue(input, out dictionary) || !dictionary.TryGetValue(API.Scp914.KnobStatus, out result))
                return null;

            return result;
        }
    }
}