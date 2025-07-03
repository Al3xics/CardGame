using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Provides utility methods for common operations on dictionaries, such as converting
    /// dictionary data into separate arrays for keys and values.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Splits the keys and values of the provided dictionary into separate arrays.
        /// </summary>
        /// <typeparam name="T1">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="T2">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to split into keys and values.</param>
        /// <param name="keys">The array to contain the keys from the dictionary.</param>
        /// <param name="values">The array to contain the values from the dictionary.</param>
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

        /// Converts a dictionary into separate arrays for keys and values.
        /// <typeparam name="T1">Type of the dictionary keys.</typeparam>
        /// <typeparam name="T2">Type of the dictionary values.</typeparam>
        /// <param name="dictionary">The dictionary to convert.</param>
        /// <param name="keys">An array of dictionary keys, output by the method.</param>
        /// <param name="values">An array of values arrays corresponding to the dictionary, output by the method.</param>
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
