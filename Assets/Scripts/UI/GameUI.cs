using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Observer: listens to the GameManager and PlayerHealth and shows the right screen.
public class GameUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject placingHint;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject crosshair;

    [Header("Menu")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;

    [Header("HUD")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    [Header("End Screen")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private GameManager game;

    private void Start()
    {
        game = GameManager.Instance;
        game.StateChanged += OnStateChanged;
        game.ScoreChanged += OnScoreChanged;
        playerHealth.HealthChanged += OnHealthChanged;

        easyButton.onClick.AddListener(() => game.StartGame(Difficulty.Easy));
        hardButton.onClick.AddListener(() => game.StartGame(Difficulty.Hard));
        restartButton.onClick.AddListener(game.Restart);
        menuButton.onClick.AddListener(game.BackToMenu);

        OnHealthChanged(playerHealth.Current, playerHealth.Max);
        OnStateChanged(game.State);
    }

    private void OnDestroy()
    {
        if (game) { game.StateChanged -= OnStateChanged; game.ScoreChanged -= OnScoreChanged; }
        if (playerHealth) playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (game.State == GameState.Playing) timeText.text = Leaderboard.FormatTime(game.TimeSurvived);
    }

    private void OnStateChanged(GameState state)
    {
        menuPanel.SetActive(state == GameState.Menu);
        placingHint.SetActive(state == GameState.Placing);
        hudPanel.SetActive(state == GameState.Playing);
        endPanel.SetActive(state == GameState.GameOver);
        crosshair.SetActive(state == GameState.Playing);

        if (state == GameState.GameOver) ShowResults();
    }

    private void OnScoreChanged(int score) => scoreText.text = $"Score {score}";

    private void OnHealthChanged(int current, int max)
    {
        healthFill.anchorMax = new Vector2((float)current / max, 1f);
        healthText.text = current.ToString();
    }

    private void ShowResults()
    {
        finalScoreText.text = $"Score  {game.Score}";
        finalTimeText.text = $"Time  {Leaderboard.FormatTime(game.TimeSurvived)}";

        List<Leaderboard.Entry> entries = Leaderboard.Load();
        var sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
            sb.AppendLine($"{i + 1}.   {entries[i].score} pts   {Leaderboard.FormatTime(entries[i].time)}");
        leaderboardText.text = sb.ToString();
    }
}
