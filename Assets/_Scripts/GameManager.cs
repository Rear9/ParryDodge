using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerColor playerColor;
    [SerializeField] private TutorialManager tutorialManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerColor = FindFirstObjectByType<PlayerColor>();
        if (playerColor != null) playerColor.FadeIn(1f);
        tutorialManager = FindFirstObjectByType<TutorialManager>();
        if (tutorialManager != null) tutorialManager.StartTutorial();
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
