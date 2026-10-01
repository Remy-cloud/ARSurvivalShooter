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
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject placingHint;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject crosshair;

    [Header("Menu")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button leaderboardButton;

    [Header("Leaderboard Screen")]
    [SerializeField] private TMP_Text menuBoardText;
    [SerializeField] private Button backButton;

    [Header("HUD")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private float warningTime = 10f;

    [Header("End Screen")]
    [SerializeField] private TMP_Text endTitleText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalKillsText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private static readonly Color WinColor = new Color(0.3f, 0.85f, 0.45f);
    private static readonly Color LoseColor = new Color(0.9f, 0.25f, 0.25f);
    private static readonly Color WarningColor = new Color(1f, 0.35f, 0.3f);

    private GameManager game;

    private void Start()
    {
        game = GameManager.Instance;
        game.StateChanged += OnStateChanged;
        game.ScoreChanged += OnScoreChanged;
        playerHealth.HealthChanged += OnHealthChanged;

        easyButton.onClick.AddListener(() => game.StartGame(Difficulty.Easy));
        hardButton.onClick.AddListener(() => game.StartGame(Difficulty.Hard));
        leaderboardButton.onClick.AddListener(() => ShowLeaderboard(true));
        backButton.onClick.AddListener(() => ShowLeaderboard(false));
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
        if (game.State != GameState.Playing) return;
        float remaining = game.TimeRemaining;
        timeText.text = Leaderboard.FormatTime(Mathf.Ceil(remaining));
        timeText.color = remaining <= warningTime ? WarningColor : Color.white;
    }

    private void OnStateChanged(GameState state)
    {
        menuPanel.SetActive(state == GameState.Menu);
        leaderboardPanel.SetActive(false);
        placingHint.SetActive(state == GameState.Placing);
        hudPanel.SetActive(state == GameState.Playing);
        endPanel.SetActive(state == GameState.GameOver);
        crosshair.SetActive(state == GameState.Playing);

        if (state == GameState.GameOver) ShowResults();
    }

    // The leaderboard screen is part of the menu, so it just swaps with the menu panel.
    private void ShowLeaderboard(bool show)
    {
        if (show) menuBoardText.text = BuildBoard();
        leaderboardPanel.SetActive(show);
        menuPanel.SetActive(!show);
    }

    private void OnScoreChanged(int score) => scoreText.text = $"Score {score}";

    private void OnHealthChanged(int current, int max)
    {
        healthFill.anchorMax = new Vector2((float)current / max, 1f);
        healthText.text = current.ToString();
    }

    private void ShowResults()
    {
        endTitleText.text = game.Survived ? "YOU SURVIVED!" : "GAME OVER";
        endTitleText.color = game.Survived ? WinColor : LoseColor;
        finalScoreText.text = game.Score.ToString();
        finalKillsText.text = game.EnemiesDefeated.ToString();
        finalTimeText.text = Leaderboard.FormatTime(game.TimeSurvived);
        leaderboardText.text = BuildBoard();
    }

    private static string BuildBoard()
    {
        List<Leaderboard.Entry> entries = Leaderboard.Load();
        if (entries.Count == 0) return "<align=center><color=#999999>No games yet</color></align>";

        var sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
            sb.AppendLine($"{i + 1}.<pos=18%>{entries[i].score} pts<pos=65%>{Leaderboard.FormatTime(entries[i].time)}");
        return sb.ToString();
    }
}
