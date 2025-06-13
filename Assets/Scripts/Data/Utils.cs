using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public static class Utils
    {
        public static void DictionaryToArrays<T1, T2>(
            Dictionary<T1, T2> dictionary,
            out T1[] keys,
            out T2[] values)
        {
            int count = dictionary.Count;
            keys = new T1[count];
            values = new T2[count];

            int index = 0;
            foreach (var kvp in dictionary)
            {
                keys[index] = kvp.Key;
                values[index] = kvp.Value;
                index++;
            }
        }

        public static void DictionaryToArrays<T1, T2>(
            Dictionary<T1, List<T2>> dictionary,
            out T1[] keys,
            out T2[][] values)
        {
            int count = dictionary.Count;
            keys = new T1[count];
            values = new T2[count][];

            int index = 0;
            foreach (var kvp in dictionary)
            {
                keys[index] = kvp.Key;
                values[index] = kvp.Value.ToArray();
                index++;
            }
        }
    }
}
