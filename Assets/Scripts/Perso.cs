using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class Perso {
    private static readonly Random Rng = new(SystemInfo.deviceUniqueIdentifier.GetHashCode());
    public static int HwId => Rng.Next();
    private static Dictionary<string, bool> boolCache = new();

    public static bool GetBool(string key = "", bool timed = false) {
        if (boolCache.TryGetValue(key, out bool b)) return b;
        
        var value = ((HwId * (timed && (DateTime.Now.Hour < 16) ? -1 : 1)) & 1) == 0;
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

    public static T GetItem<T>(T[] array) {
        if (array == null || array.Length == 0) throw new ArgumentException("array must not be null or empty");
        return array[Math.Abs(HwId) % array.Length];
    }
}
