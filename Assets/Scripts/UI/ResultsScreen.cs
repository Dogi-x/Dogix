using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class ResultsScreen : NetworkBehaviour
{
    public static ResultsScreen Instance { get; private set; }

    [SerializeField] private GameObject       panel;
    [SerializeField] private TextMeshProUGUI  winnerText;
    [SerializeField] private TextMeshProUGUI  killsText;
    [SerializeField] private TextMeshProUGUI  damageText;
    [SerializeField] private TextMeshProUGUI  revivesText;
    [SerializeField] private TextMeshProUGUI  survivalText;
    [SerializeField] private Button           lobbyButton;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panel?.SetActive(false);
    }

    public void Show(TeamType winner)
    {
        panel?.SetActive(true);

        bool localWon = TeamManager.Instance != null &&
                        TeamManager.Instance.GetTeam(Runner.LocalPlayer) == winner;

        winnerText.text  = localWon ? "VICTORY!" : "DEFEAT";
        winnerText.color = localWon ? Color.yellow : Color.red;

        var s = LocalStats.Current;
        killsText.text    = $"Kills: {s.Kills}";
        damageText.text   = $"Damage: {s.TotalDamage:F0}";
        revivesText.text  = $"Revives: {s.Revives}";
        survivalText.text = $"Survived: {FormatTime(s.SurvivalSeconds)}";

        lobbyButton?.onClick.RemoveAllListeners();
        lobbyButton?.onClick.AddListener(() => Runner.Shutdown());
    }

    private static string FormatTime(float s)
    {
        int m = (int)(s / 60), sec = (int)(s % 60);
        return $"{m:00}:{sec:00}";
    }
}

public class LocalStats
{
    public static LocalStats Current { get; private set; } = new();
    public int   Kills;
    public float TotalDamage;
    public int   Revives;
    public float SurvivalSeconds;
    public static void Reset() => Current = new();
}
