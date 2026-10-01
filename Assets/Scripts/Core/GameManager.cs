using System;
using UnityEngine;

// Singleton + State pattern: owns the game flow Menu -> Placing -> Playing -> GameOver.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TapToPlaceArena placer;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooter shooter;
    [SerializeField] private ProjectilePool[] bulletPools;

    [Header("Difficulty")]
    [SerializeField] private DifficultySettings easy = new DifficultySettings(3.5f, 4, 0.25f, 1f);
    [SerializeField] private DifficultySettings hard = new DifficultySettings(1.8f, 8, 0.45f, 1.5f);

    [Header("Round")]
    [SerializeField] private float timeLimit = 90f;

    public GameState State { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public int Score { get; private set; }
    public float TimeSurvived { get; private set; }
    public float TimeRemaining => Mathf.Max(0f, timeLimit - TimeSurvived);
    public int EnemiesDefeated { get; private set; }
    public bool Survived { get; private set; }

    public event Action<GameState> StateChanged;
    public event Action<int> ScoreChanged;

    private Arena arena;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        placer.ArenaPlaced += OnArenaPlaced;
        playerHealth.Died += OnPlayerDied;
        EnemyBase.AnyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        placer.ArenaPlaced -= OnArenaPlaced;
        playerHealth.Died -= OnPlayerDied;
        EnemyBase.AnyKilled -= OnEnemyKilled;
    }

    private void Start()
    {
        placer.enabled = false;
        shooter.CanShoot = false;
        SetState(GameState.Menu);
    }

    private void Update()
    {
        if (State != GameState.Playing) return;
        TimeSurvived = Mathf.Min(TimeSurvived + Time.deltaTime, timeLimit);
        if (TimeSurvived >= timeLimit) EndRound(true);
    }

    public void StartGame(Difficulty difficulty)
    {
        Difficulty = difficulty;
        if (arena) BeginRound();
        else
        {
            placer.enabled = true;
            SetState(GameState.Placing);
        }
    }

    public void Restart() => StartGame(Difficulty);

    // Arena stays placed, so the next Easy/Hard tap starts a round straight away.
    public void BackToMenu()
    {
        if (State == GameState.GameOver) SetState(GameState.Menu);
    }

    private void OnArenaPlaced(Arena placed)
    {
        arena = placed;
        placer.enabled = false;
        BeginRound();
    }

    private void BeginRound()
    {
        DifficultySettings s = Difficulty == Difficulty.Hard ? hard : easy;

        Score = 0;
        TimeSurvived = 0f;
        EnemiesDefeated = 0;
        Survived = false;
        ScoreChanged?.Invoke(Score);

        playerHealth.DamageMultiplier = s.damageMultiplier;
        playerHealth.ResetHealth();
        spawner.Configure(s);
        spawner.Begin(arena);
        shooter.CanShoot = true;

        SetState(GameState.Playing);
    }

    private void OnEnemyKilled(EnemyBase enemy)
    {
        if (State != GameState.Playing) return;
        Score += enemy.ScoreValue;
        EnemiesDefeated++;
        ScoreChanged?.Invoke(Score);
    }

    private void OnPlayerDied() => EndRound(false);

    // Ends the round: survived = the timer ran out before the player died.
    private void EndRound(bool survived)
    {
        if (State != GameState.Playing) return;

        Survived = survived;
        shooter.CanShoot = false;
        spawner.WipeAll();
        foreach (ProjectilePool pool in bulletPools) pool.ReturnAll();
        Leaderboard.Add(Score, TimeSurvived);

        SetState(GameState.GameOver);
    }

    private void SetState(GameState state)
    {
        State = state;
        StateChanged?.Invoke(state);
    }
}
