using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<int> keys = new List<int>();
    [SerializeField]
    private List<bool> values = new List<bool>();

    public Dictionary<int, bool> dictionary = new Dictionary<int, bool>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in dictionary)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<int, bool>();

        for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
        {
            dictionary[keys[i]] = values[i];
        }
    }
}
