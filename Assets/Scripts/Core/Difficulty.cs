using System;
using UnityEngine;

public enum Difficulty { Easy, Hard }

[Serializable]
public class DifficultySettings
{
    public float spawnInterval = 3f;
    public int maxAlive = 6;
    [Range(0f, 1f)] public float shooterChance = 0.35f;
    public float damageMultiplier = 1f;

    public DifficultySettings(float interval, int max, float shooters, float damage)
    {
        spawnInterval = interval;
        maxAlive = max;
        shooterChance = shooters;
        damageMultiplier = damage;
    }
}
