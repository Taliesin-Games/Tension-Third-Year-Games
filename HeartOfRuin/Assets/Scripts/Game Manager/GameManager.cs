using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

[RequireComponent(typeof(MusicManager))]
[RequireComponent(typeof(LevelManager))]
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    #region Actions
    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGameWin;
    /// <summary>
    /// Occurs when the tension values are updated.
    /// </summary>
    /// <remarks>This event is triggered whenever the tension changes, providing the updated tension
    /// completion ratio (0 to 1) and the current tension value. Subscribers can use this event to respond to changes in
    /// tension, such as  updating UI elements or triggering other logic.</remarks>
    public event Action<float, float> OnTensionChanged; // float TensionCompletionRatio, float currentTension
    public event Action OnTensionFull;
    #endregion

    #region Configuration
    [Header("Game Settings")]
    [Tooltip("A count up timer used to track certain game conditions such as boss emergence.\n" +
        "With no modifiers this is 1 per 1 second.\n\n" +
        "This is a default settings and can be changed")]
    [SerializeField] float tensionLimit = 120f;
    [Range(0.1f, 10f)]
    [SerializeField] float defaultTensionRate = 1f;

    [Header("Canvase References")]
    [SerializeField] GameObject playCanvas;     // TODO consider how we find and assign these references automatically.
    #endregion

    #region Cached References
    EnemySpawner enemySpawner;
    #endregion

    #region Runtime Variables
    // Game State Settings
    bool gameOver = false;
    bool gameWon = false;

    // Gamplay Variables
    float currentTension = 0f;
    float tensionRate; // Multiplier for tension increase rate.
    #endregion

    #region Properties
    public bool GameIsOver { get => gameOver; }
    public bool GameWon { get => gameWon; }
    public float TensionCompletionRatio { get => currentTension / tensionLimit; }
    #endregion

    private void Awake()
    {
        // Setup singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep this instance across scenes

        // Subscrite to sceneLoaded once.
        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindRefereces();
    }

    private void FindRefereces()
    {
        enemySpawner = GetComponent<EnemySpawner>();
        if (!enemySpawner) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (!enemySpawner) Debugger.LogError("EnemySpawner not found by GameManager.");
        
    }

    void Update()
    {
        if(gameOver || gameWon) return;
        ManageTension();

    }
    void ManageTension()
    {
        currentTension += Time.deltaTime * tensionRate;
        OnTensionChanged?.Invoke(TensionCompletionRatio, currentTension);

        if (currentTension >= tensionLimit)
        {
            OnTensionFull?.Invoke();
        }
    }
    
    public void OnEnemyDefeated()
    {
        if (
            !enemySpawner ||
            (EnemySpawner.EnemyCount <= 0 && EnemySpawner.IsSpawningComplete)
            )
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        OnGameWin?.Invoke();
        gameWon = true;
        playCanvas.SetActive(false);

        LevelManager.LoadWinScreen();
    }
   
    public void GameOver()
    {
        OnGameOver?.Invoke();
        playCanvas.SetActive(false);
        
        LevelManager.LoadGameOver();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RunPerSceneSetup();
    }
    private void RunPerSceneSetup()
    {
        tensionRate = defaultTensionRate;
        currentTension = 0f;
        gameOver = false;
        gameWon = false;
        OnGameStart?.Invoke();
    }
    private void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks if destroyed manually
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
