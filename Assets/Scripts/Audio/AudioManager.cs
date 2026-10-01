using UnityEngine;

// Singleton + Observer: plays every game sound by listening to gameplay events.
// One source for one-shot effects, one looping source for music.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Required Sounds")]
    [SerializeField] private AudioClip playerShoot;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip enemySpawn;
    [SerializeField] private AudioClip enemyShoot;
    [SerializeField] private AudioClip meleeAttack;

    [Header("Music")]
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;

    [Header("Listens To")]
    [SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.clip = music;
    }

    private void OnEnable()
    {
        PlayerShooter.Shot += OnPlayerShoot;
        ShooterEnemy.Shot += OnEnemyShoot;
        MeleeEnemy.MeleeAttacked += OnMeleeAttack;
        EnemyBase.AnySpawned += OnEnemySpawned;
        if (playerHealth) playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        PlayerShooter.Shot -= OnPlayerShoot;
        ShooterEnemy.Shot -= OnEnemyShoot;
        MeleeEnemy.MeleeAttacked -= OnMeleeAttack;
        EnemyBase.AnySpawned -= OnEnemySpawned;
        if (playerHealth) playerHealth.Died -= OnPlayerDied;
        if (GameManager.Instance) GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void Start()
    {
        if (GameManager.Instance) GameManager.Instance.StateChanged += OnStateChanged;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip) sfxSource.PlayOneShot(clip, volume);
    }

    private void OnPlayerShoot() => Play(playerShoot, 0.7f);
    private void OnEnemyShoot() => Play(enemyShoot, 0.8f);
    private void OnMeleeAttack() => Play(meleeAttack);
    private void OnEnemySpawned(EnemyBase enemy) => Play(enemySpawn, 0.6f);
    private void OnPlayerDied() => Play(playerDeath);

    // Music plays only during a round.
    private void OnStateChanged(GameState state)
    {
        if (!music) return;
        if (state == GameState.Playing && !musicSource.isPlaying) musicSource.Play();
        else if (state != GameState.Playing) musicSource.Stop();
    }
}
