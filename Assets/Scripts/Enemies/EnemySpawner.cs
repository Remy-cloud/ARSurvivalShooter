using System.Collections.Generic;
using UnityEngine;

// Spawns enemies at the arena's spawn points and keeps track of them.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyFactory factory;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxAlive = 6;
    [SerializeField, Range(0f, 1f)] private float shooterChance = 0.35f;

    private readonly List<EnemyBase> alive = new List<EnemyBase>();
    private Arena arena;
    private Transform player;
    private bool spawning;
    private float nextSpawnTime;

    public int AliveCount => alive.Count;

    private void Awake() => player = Camera.main.transform;

    public void Configure(DifficultySettings settings)
    {
        spawnInterval = settings.spawnInterval;
        maxAlive = settings.maxAlive;
        shooterChance = settings.shooterChance;
    }

    public void Begin(Arena placedArena)
    {
        arena = placedArena;
        spawning = true;
        nextSpawnTime = Time.time + 1.5f;
    }

    public void Stop() => spawning = false;

    public void WipeAll()
    {
        Stop();
        foreach (EnemyBase e in alive.ToArray())
            if (e) e.Remove();
        alive.Clear();
    }

    private void Update()
    {
        if (!spawning || Time.time < nextSpawnTime || alive.Count >= maxAlive) return;
        nextSpawnTime = Time.time + spawnInterval;
        SpawnOne();
    }

    private void SpawnOne()
    {
        Transform point = arena.GetRandomSpawnPoint();
        EnemyType type = Random.value < shooterChance ? EnemyType.Shooter : EnemyType.Melee;

        EnemyBase enemy = factory.Create(type, point.position, Quaternion.identity);
        enemy.Removed += OnEnemyRemoved;
        enemy.Init(player);
        alive.Add(enemy);
    }

    private void OnEnemyRemoved(EnemyBase enemy) => alive.Remove(enemy);
}
