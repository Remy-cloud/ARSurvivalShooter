using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds the Start menu, HUD and End screen, and wires the GameManager.
public static class GameUISetup
{
    static readonly Color PanelColor = new Color(0.05f, 0.05f, 0.06f, 0.82f);
    static readonly Color ButtonColor = new Color(0.16f, 0.16f, 0.18f, 0.95f);
    static readonly Color Accent = new Color(1f, 0.76f, 0.3f);
    static readonly Color Danger = new Color(0.9f, 0.25f, 0.25f);
    static readonly Color HealthColor = new Color(0.3f, 0.8f, 0.4f);
    static readonly Color Muted = new Color(0.7f, 0.7f, 0.72f);

    [MenuItem("Tools/AR Shooter/Step 5 - Game Manager + UI")]
    public static void Build()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (!canvas) { Debug.LogError("No Canvas in the scene."); return; }
        Transform root = canvas.transform;

        foreach (string old in new[] { "MenuPanel", "LeaderboardPanel", "PlacingHint", "HUD", "EndPanel" })
        {
            Transform t = root.Find(old);
            if (t) Object.DestroyImmediate(t.gameObject);
        }

        // Start menu
        RectTransform menu = Panel(root, "MenuPanel", PanelColor);
        Label(menu, "Title", "AR SURVIVAL\nSHOOTER", 110, Accent, new Vector2(0.5f, 0.7f), new Vector2(1000, 300), FontStyles.Bold);
        Label(menu, "Subtitle", "Choose difficulty", 50, Color.white, new Vector2(0.5f, 0.565f), new Vector2(900, 80));
        Button easy = MakeButton(menu, "EasyButton", "EASY", new Vector2(0.5f, 0.485f));
        Button hard = MakeButton(menu, "HardButton", "HARD", new Vector2(0.5f, 0.4f));
        Button boardButton = MakeButton(menu, "LeaderboardButton", "LEADERBOARD", new Vector2(0.5f, 0.27f), Color.white);

        // Leaderboard screen (opened from the menu)
        RectTransform boardPanel = Panel(root, "LeaderboardPanel", PanelColor);
        Label(boardPanel, "Title", "LEADERBOARD", 100, Accent, new Vector2(0.5f, 0.74f), new Vector2(1000, 140), FontStyles.Bold);
        Label(boardPanel, "Subtitle", "Last 5 games", 44, Muted, new Vector2(0.5f, 0.68f), new Vector2(900, 70));
        TMP_Text header = Label(boardPanel, "Header", "#<pos=18%>SCORE<pos=65%>TIME", 38, Muted, new Vector2(0.5f, 0.605f), new Vector2(640, 60), FontStyles.Bold);
        header.alignment = TextAlignmentOptions.Left;
        TMP_Text menuBoard = Label(boardPanel, "Entries", "", 50, Color.white, new Vector2(0.5f, 0.45f), new Vector2(640, 330));
        menuBoard.alignment = TextAlignmentOptions.TopLeft;
        Button back = MakeButton(boardPanel, "BackButton", "BACK", new Vector2(0.5f, 0.24f));

        // Placing hint
        RectTransform hint = Rect(root, "PlacingHint", new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(900, 110));
        Image hintBg = hint.gameObject.AddComponent<Image>();
        hintBg.color = new Color(0f, 0f, 0f, 0.6f);
        hintBg.raycastTarget = false;
        Label(hint, "Text", "Scan the floor and tap to place", 46, Color.white, new Vector2(0.5f, 0.5f), new Vector2(900, 110));

        // HUD
        RectTransform hud = Panel(root, "HUD", Color.clear);
        hud.GetComponent<Image>().raycastTarget = false;

        RectTransform barBg = Rect(hud, "HealthBar", new Vector2(0, 1), new Vector2(0, 1), new Vector2(60, -170), new Vector2(400, 48));
        barBg.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform fill = Rect(barBg, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fill.gameObject.AddComponent<Image>().color = HealthColor;
        TMP_Text healthText = Label(barBg, "HealthText", "100", 36, Color.white, new Vector2(0.5f, 0.5f), new Vector2(400, 48), FontStyles.Bold);

        TMP_Text scoreText = Label(hud, "ScoreText", "Score 0", 56, Accent, new Vector2(1, 1), new Vector2(500, 70), FontStyles.Bold);
        Place(scoreText.rectTransform, new Vector2(1, 1), new Vector2(-60, -160));
        scoreText.alignment = TextAlignmentOptions.Right;

        TMP_Text timeText = Label(hud, "TimeText", "01:30", 48, Color.white, new Vector2(1, 1), new Vector2(300, 60), FontStyles.Bold);
        Place(timeText.rectTransform, new Vector2(1, 1), new Vector2(-60, -235));
        timeText.alignment = TextAlignmentOptions.Right;

        // End screen
        RectTransform end = Panel(root, "EndPanel", PanelColor);
        TMP_Text endTitle = Label(end, "Title", "GAME OVER", 110, Danger, new Vector2(0.5f, 0.83f), new Vector2(1000, 160), FontStyles.Bold);
        TMP_Text finalScore = Stat(end, "Score", "SCORE", 0.2f, Accent);
        TMP_Text finalKills = Stat(end, "Kills", "ENEMIES\nDEFEATED", 0.5f, Color.white);
        TMP_Text finalTime = Stat(end, "Time", "TIME\nSURVIVED", 0.8f, Color.white);
        Label(end, "BoardTitle", "LAST 5 GAMES", 44, Muted, new Vector2(0.5f, 0.585f), new Vector2(900, 70), FontStyles.Bold);
        TMP_Text board = Label(end, "Leaderboard", "", 46, Color.white, new Vector2(0.5f, 0.45f), new Vector2(640, 300));
        board.alignment = TextAlignmentOptions.TopLeft;
        Button restart = MakeButton(end, "RestartButton", "RESTART", new Vector2(0.5f, 0.26f));
        Button menuButton = MakeButton(end, "MenuButton", "MENU", new Vector2(0.5f, 0.165f), Color.white);

        menu.gameObject.SetActive(true);
        boardPanel.gameObject.SetActive(false);
        hint.gameObject.SetActive(false);
        hud.gameObject.SetActive(false);
        end.gameObject.SetActive(false);

        // GameManager
        GameObject gmObject = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        GameManager gm = GetOrAdd<GameManager>(gmObject);
        var gmSo = new SerializedObject(gm);
        gmSo.FindProperty("placer").objectReferenceValue = Object.FindAnyObjectByType<TapToPlaceArena>();
        gmSo.FindProperty("spawner").objectReferenceValue = Object.FindAnyObjectByType<EnemySpawner>();
        gmSo.FindProperty("playerHealth").objectReferenceValue = Object.FindAnyObjectByType<PlayerHealth>();
        gmSo.FindProperty("shooter").objectReferenceValue = Object.FindAnyObjectByType<PlayerShooter>();
        var pools = new System.Collections.Generic.List<ProjectilePool>();
        foreach (GameObject r in canvas.gameObject.scene.GetRootGameObjects()) pools.AddRange(r.GetComponentsInChildren<ProjectilePool>());
        SerializedProperty poolsProp = gmSo.FindProperty("bulletPools");
        poolsProp.arraySize = pools.Count;
        for (int i = 0; i < pools.Count; i++) poolsProp.GetArrayElementAtIndex(i).objectReferenceValue = pools[i];
        gmSo.ApplyModifiedPropertiesWithoutUndo();

        // GameUI
        GameUI ui = GetOrAdd<GameUI>(canvas.gameObject);
        var uiSo = new SerializedObject(ui);
        uiSo.FindProperty("menuPanel").objectReferenceValue = menu.gameObject;
        uiSo.FindProperty("leaderboardPanel").objectReferenceValue = boardPanel.gameObject;
        uiSo.FindProperty("leaderboardButton").objectReferenceValue = boardButton;
        uiSo.FindProperty("menuBoardText").objectReferenceValue = menuBoard;
        uiSo.FindProperty("backButton").objectReferenceValue = back;
        uiSo.FindProperty("endTitleText").objectReferenceValue = endTitle;
        uiSo.FindProperty("finalKillsText").objectReferenceValue = finalKills;
        uiSo.FindProperty("placingHint").objectReferenceValue = hint.gameObject;
        uiSo.FindProperty("hudPanel").objectReferenceValue = hud.gameObject;
        uiSo.FindProperty("endPanel").objectReferenceValue = end.gameObject;
        Transform crosshair = root.Find("Crosshair");
        uiSo.FindProperty("crosshair").objectReferenceValue = crosshair ? crosshair.gameObject : null;
        uiSo.FindProperty("easyButton").objectReferenceValue = easy;
        uiSo.FindProperty("hardButton").objectReferenceValue = hard;
        uiSo.FindProperty("playerHealth").objectReferenceValue = Object.FindAnyObjectByType<PlayerHealth>();
        uiSo.FindProperty("healthFill").objectReferenceValue = fill;
        uiSo.FindProperty("healthText").objectReferenceValue = healthText;
        uiSo.FindProperty("scoreText").objectReferenceValue = scoreText;
        uiSo.FindProperty("timeText").objectReferenceValue = timeText;
        uiSo.FindProperty("finalScoreText").objectReferenceValue = finalScore;
        uiSo.FindProperty("finalTimeText").objectReferenceValue = finalTime;
        uiSo.FindProperty("leaderboardText").objectReferenceValue = board;
        uiSo.FindProperty("restartButton").objectReferenceValue = restart;
        uiSo.FindProperty("menuButton").objectReferenceValue = menuButton;
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("Step 5: GameManager + UI built. Save the scene (Cmd+S).");
    }

    static RectTransform Panel(Transform parent, string name, Color color)
    {
        RectTransform rt = Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        rt.gameObject.AddComponent<Image>().color = color;
        return rt;
    }

    static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
    }

    static TMP_Text Label(Transform parent, string name, string text, float size, Color color, Vector2 anchor, Vector2 box, FontStyles style = FontStyles.Normal)
    {
        RectTransform rt = Rect(parent, name, anchor, anchor, Vector2.zero, box);
        rt.pivot = new Vector2(0.5f, 0.5f);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    // A stat column on the end screen: small label on top, big value below.
    static TMP_Text Stat(Transform parent, string name, string label, float x, Color valueColor)
    {
        TMP_Text l = Label(parent, name + "Label", label, 34, Muted, new Vector2(x, 0.725f), new Vector2(300, 100), FontStyles.Bold);
        l.alignment = TextAlignmentOptions.Bottom;
        return Label(parent, name + "Value", "0", 80, valueColor, new Vector2(x, 0.66f), new Vector2(300, 100), FontStyles.Bold);
    }

    static Button MakeButton(Transform parent, string name, string text, Vector2 anchor, Color? textColor = null)
    {
        RectTransform rt = Rect(parent, name, anchor, anchor, Vector2.zero, new Vector2(600, 130));
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.gameObject.AddComponent<Image>().color = ButtonColor;
        Button b = rt.gameObject.AddComponent<Button>();
        Label(rt, "Label", text, 64, textColor ?? Accent, new Vector2(0.5f, 0.5f), new Vector2(600, 130), FontStyles.Bold);
        return b;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        return c ? c : go.AddComponent<T>();
    }
}
