using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

[System.Serializable]
public class PlayerStats
{
    public int totalDeaths = 0;
    public string lastWaveDiedOn = "";
    public int totalParries = 0;
    public int totalDodges = 0;
}

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    private PlayerStats _stats;
    private float _sessionStartTime;
    private const string TutorialKey = "TutorialCompleted";
    private const string DeathsKey = "TotalDeaths";

    public bool replayTutorial = false;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeAnalytics();
        LoadLocalStats();
        _sessionStartTime = Time.time;
    }

    private async Task InitializeAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Analytics init failed: {e.Message}");
        }
    }

    private void OnApplicationQuit() => SaveLocalStats();
    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveLocalStats(); }

    // --- Local storage for tutorial + total deaths ---
    private void LoadLocalStats()
    {
        _stats = new PlayerStats();

        if (PlayerPrefs.HasKey(DeathsKey))
            _stats.totalDeaths = PlayerPrefs.GetInt(DeathsKey);
    }

    private void SaveLocalStats()
    {
        PlayerPrefs.SetInt(DeathsKey, _stats.totalDeaths);
        PlayerPrefs.Save();
    }

    // --- Tutorial ---
    public bool ShouldPlayTutorial() => !PlayerPrefs.HasKey(TutorialKey) || replayTutorial;

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TutorialKey, 1);
        PlayerPrefs.Save();
        replayTutorial = false;
    }

    // --- Death tracking (local + analytics) ---
    public void RecordDeath(string waveName)
    {
        _stats.totalDeaths++;
        _stats.lastWaveDiedOn = waveName;
        SaveLocalStats();

        var deathParams = new Dictionary<string, object>
        {
            { "wave_name", waveName },
            { "total_deaths", _stats.totalDeaths },
            { "total_parries", _stats.totalParries },
            { "total_dodges", _stats.totalDodges }
        };
        AnalyticsService.Instance.RecordEvent("player_death", deathParams);
        print("Analytics log");
        AnalyticsService.Instance.Flush();

    }

    // --- Other stat increments ---
    public void RecordParry() => _stats.totalParries++;
    public void RecordDodge() => _stats.totalDodges++;

    // --- Utility ---
    public PlayerStats GetStats() => _stats;
}
