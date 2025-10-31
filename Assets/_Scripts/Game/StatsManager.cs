using UnityEngine;
using Unity.Services.Core;
using Abertay.Analytics;
using System.Collections.Generic;
using System;

[System.Serializable]
public class PlayerStats
{
    public string gameState = "Play";
    public int totalDeaths;
    public int totalCompletions;
    public string lastWaveDiedOn = "";
    public int totalParries;
    public int totalDodges;
}

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }
    private PlayerStats _stats;
    private UIManager _ui;
    private const string TutorialKey = "TutorialCompleted";
    private const string PlayerIdKey = "PlayerID";
    private const string StatsKey = "PlayerStats";
    private string _playerID;

    public bool replayTutorial = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load or create persistent stats
        if (PlayerPrefs.HasKey(StatsKey))
            _stats = JsonUtility.FromJson<PlayerStats>(PlayerPrefs.GetString(StatsKey));
        else
            _stats = new PlayerStats();

        // Load or create persistent player ID
        if (!PlayerPrefs.HasKey(PlayerIdKey))
            PlayerPrefs.SetString(PlayerIdKey, Guid.NewGuid().ToString());
        _playerID = PlayerPrefs.GetString(PlayerIdKey);

        _ui = FindFirstObjectByType<UIManager>();
        AnalyticsManager.Initialise("v0.5");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && _ui != null)
        {
            _stats.gameState = "Paused/Closing";
            RecordFull(_ui.GetCurrentWaveName());
            SaveStats();
        }
    }

    // --- tutorial ---
    public bool ShouldPlayTutorial() => !PlayerPrefs.HasKey(TutorialKey) || replayTutorial;

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TutorialKey, 1);
        PlayerPrefs.Save();
        replayTutorial = false;
    }

    // --- general stats ---
    public void RecordFull(string waveName)
    {
        var parameters = new Dictionary<string, object>
        {
            { "player_id", _playerID },
            { "time", Time.time },
            { "game_state", _stats.gameState},
            { "wave_name", waveName },
            { "total_deaths", _stats.totalDeaths },
            { "total_parries", _stats.totalParries },
            { "total_dodges", _stats.totalDodges },
            { "total_completions", _stats.totalCompletions }
        };

        foreach (var key in parameters.Keys)
            print($"Key: {key} -  Value: {parameters[key]}");

        AnalyticsManager.SendCustomEvent("Stats", parameters);
        StartCoroutine(GlobalStatLogger.SendToGoogleSheet(parameters));
    }

    private void SaveStats()
    {
        string json = JsonUtility.ToJson(_stats);
        PlayerPrefs.SetString(StatsKey, json);
        PlayerPrefs.Save();
    }

    // --- stat increments ---
    public void RecordParry()
    {
        _stats.totalParries++;
        SaveStats();
    }

    public void RecordDodge()
    {
        _stats.totalDodges++;
        SaveStats();
    }

    public void RecordCompletion()
    {
        _stats.totalCompletions++;
        SaveStats();
        RecordFull(_ui != null ? _ui.GetCurrentWaveName() : "Unknown");
    }

    public void RecordDeath(string waveName)
    {
        _stats.totalDeaths++;
        _stats.gameState = "Death";
        _stats.lastWaveDiedOn = waveName;
        SaveStats();
        RecordFull(waveName);
    }

    public PlayerStats GetStats() => _stats;
}
