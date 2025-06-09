using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wendogo;
using Wendogo.Menu;

public class GameManager : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SessionManager.Instance.ActiveSession.CurrentPlayer.Properties.TryGetValue(SessionConstants.PlayerNamePropertyKey, out var name);
    }
}

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