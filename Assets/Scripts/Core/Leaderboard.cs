using System;
using System.Collections.Generic;
using UnityEngine;

// Keeps the last 5 sessions in PlayerPrefs so they survive closing the app.
public static class Leaderboard
{
    [Serializable]
    public struct Entry
    {
        public int score;
        public float time;
    }

    [Serializable]
    private class Data { public List<Entry> entries = new List<Entry>(); }

    private const string Key = "leaderboard";
    private const int MaxEntries = 5;

    public static List<Entry> Load()
    {
        string json = PlayerPrefs.GetString(Key, "");
        if (string.IsNullOrEmpty(json)) return new List<Entry>();
        return JsonUtility.FromJson<Data>(json).entries;
    }

    public static void Add(int score, float time)
    {
        List<Entry> entries = Load();
        entries.Insert(0, new Entry { score = score, time = time });
        if (entries.Count > MaxEntries) entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new Data { entries = entries }));
        PlayerPrefs.Save();
    }

    public static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }
}
