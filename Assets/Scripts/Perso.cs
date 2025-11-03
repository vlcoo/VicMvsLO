using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Personalization engine.
/// </summary>
public static class Perso {
    private static readonly Random Rng = new(SystemInfo.deviceUniqueIdentifier.GetHashCode());
    public static int HwId => Rng.Next();
    private static Dictionary<string, bool> boolCache = new();
    private static Dictionary<string, int> intCache = new();

    public static bool GetBool(string key = "") {
        if (boolCache.TryGetValue(key, out bool b)) return b;
        
        var value = (HwId & 1) == 0;
        if (key != "") boolCache[key] = value;
        return value;
    }

    public static int GetIntRange(int min, int max) {
        if (max <= min) throw new ArgumentException("max must be greater than min");
        var value = min + (Math.Abs(HwId) % (max - min));
        return value;
    }

    public static float GetFloatRange(float min, float max) {
        if (max <= min) throw new ArgumentException("max must be greater than min");
        return min + ((HwId & 0xFF) / (float)0xFF) * (max - min);
    }

    public static T GetItem<T>(T[] array, string key = "") {
        if (array == null || array.Length == 0) throw new ArgumentException("array must not be null or empty");
        if (intCache.TryGetValue(key, out int i)) 
            return i >= array.Length ? throw new ArgumentException("cached index is oob. please provide the same array as the first time") : array[i];
        
        var index = Math.Abs(HwId) % array.Length;
        if (key != "") intCache[key] = index;
        var value = array[index];
        return value;
    }
}
