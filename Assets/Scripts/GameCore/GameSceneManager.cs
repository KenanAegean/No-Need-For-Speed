using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject readyPanel;
    public GameObject inGamePanel;
    public GameObject endGamePanel;
    public GameObject escMenuPanel;

    [Header("Ready Panel Components")]
    public Button hostButton;
    public Button clientButton;
    public Button mainMenuButton;
    public Button exitButton;
    public TMP_Text playerTextReady;

    [Header("In Game Panel Components")]
    public TMP_Text playerTextInGame;

    [Header("End Game Panel Components")]
    public TMP_Text winnerText;
    public Button restartButton;
    public Button menuButton;
    public Button exitButtonEnd;

    [Header("ESC Menu Components")]
    public Button resetCarButton;
    public Button exitSessionButton;
    public Button exitGameButton;
    public TMP_Text playerTextESCMenu;

    [Header("Game State")]
    public int totalPlayers = 2; // Total players in the game
    public int totalToursRequired = 3; // Number of tours needed to finish
    private int currentTours = 0;

    private NetworkVariable<int> playersReady = new NetworkVariable<int>(0); // Track ready players
    private bool isGamePaused = true;

    private void Start()
    {
        ShowStartPanel();

        Debug.Log("Initializing GameSceneManager...");

        // Verify if NetworkManager is available
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }

        // Verify buttons
        if (hostButton == null || clientButton == null)
        {
            Debug.LogError("Host and Client buttons are not assigned.");
            return;
        }

        // Add button listeners
        hostButton.onClick.AddListener(JoinAsHost);
        clientButton.onClick.AddListener(JoinAsClient);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(ReturnToMainMenu);
        exitButtonEnd.onClick.AddListener(ExitGame);
        resetCarButton.onClick.AddListener(ResetCar);
        exitSessionButton.onClick.AddListener(ExitSession);
        exitGameButton.onClick.AddListener(ExitGame);

        // Verify other components
        if (playerTextReady == null)
        {
            Debug.LogError("Player Text Ready is not assigned.");
            return;
        }

        playersReady.OnValueChanged += OnPlayersReadyChanged;

        Debug.Log("GameSceneManager initialized successfully.");
    }

    private void Update()
    {
        // Toggle ESC menu with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (escMenuPanel.activeSelf)
            {
                HideESCMenuPanel();
            }
            else
            {
                ShowESCMenuPanel();
            }
        }
    }

    #region Panel Management
    public void ShowStartPanel()
    {
        SetActivePanel(startPanel);
        PauseGame();
        playersReady.Value = 0;
        currentTours = 0;
    }

    public void ShowReadyPanel()
    {
        SetActivePanel(readyPanel);
        PauseGame();
        playersReady.Value = 0;
        UpdateReadyPanelText();
    }

    public void ShowInGamePanel()
    {
        SetActivePanel(inGamePanel);
        ResumeGame();
        UpdateInGamePanelText(currentTours, totalToursRequired);
    }

    public void ShowEndGamePanel()
    {
        SetActivePanel(endGamePanel);
        PauseGame();
        winnerText.text = "Game Over!";
    }

    public void ShowESCMenuPanel()
    {
        escMenuPanel.SetActive(true);
        UpdateESCMenuText();
    }

    public void HideESCMenuPanel()
    {
        escMenuPanel.SetActive(false);
        inGamePanel.SetActive(true);
    }

    private void SetActivePanel(GameObject panel)
    {
        startPanel.SetActive(false);
        readyPanel.SetActive(false);
        inGamePanel.SetActive(false);
        endGamePanel.SetActive(false);
        escMenuPanel.SetActive(false);

        panel.SetActive(true);
    }
    #endregion

    #region Button Logic

    public void JoinAsHost()
    {
        ResumeGame();
        Debug.Log("Starting Host...");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully.");
            IncrementReadyCount();
            ResumeGame();
        }
        else
        {
            Debug.LogError("Failed to start Host.");
        }
        readyPanel.SetActive(false);// delete later
    }

    public void JoinAsClient()
    {
        ResumeGame();
        Debug.Log("Starting Client...");
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            IncrementReadyCount();
            ResumeGame();
        }
        else
        {
            Debug.LogError("Failed to start Client.");
        }
        readyPanel.SetActive(false); //delete later
    }


    private void IncrementReadyCount()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }

        if (IsServer)
        {
            playersReady.Value++; // Increase ready count on the server
            Debug.Log($"Server incremented ready count: {playersReady.Value}");
        }
        else
        {
            Debug.Log("Client requesting ready count increment...");
            NotifyPlayerJoinedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyPlayerJoinedServerRpc()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (GameSceneManager.Instance == null)
            {
                Debug.LogError("GameSceneManager.Instance is null when attempting to increment ready count!");
                return;
            }

            playersReady.Value++;
            Debug.Log($"Server incremented ready count. Current count: {playersReady.Value}");
        }
    }

    private void OnPlayersReadyChanged(int oldValue, int newValue)
    {
        Debug.Log($"Ready count changed: {oldValue} -> {newValue}");

        playerTextReady.text = $"Players Ready: {newValue}/{totalPlayers}";

        if (newValue >= totalPlayers)
        {
            Debug.Log("All players are ready. Starting the game...");
            StartGame();
        }
    }

    private void StartGame()
    {
        Debug.Log("All players have joined. Starting the game...");

        // Deactivate the ready panel and activate in-game UI
        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Ready Panel is not assigned in GameSceneManager!");
        }

        if (inGamePanel != null)
        {
            inGamePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("In-Game Panel is not assigned in GameSceneManager!");
        }

        ResumeGame();

        // Add additional logic to start gameplay if necessary
    }


    public void RestartGame()
    {
        currentTours = 0;
        ShowReadyPanel();
    }

    public void ResetCar()
    {
        Debug.Log("Car reset logic triggered.");
        // Add your car reset logic here
    }

    public void ExitSession()
    {
        Debug.Log("Exit session logic triggered.");
        ReturnToMainMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        playersReady.Value = 0;
        currentTours = 0;
        Time.timeScale = 1f;
        Debug.Log("Game reset and returning to main menu.");
        ShowStartPanel();
    }
    #endregion

    #region UI Updates
    private void UpdateReadyPanelText()
    {
        playerTextReady.text = $"Players Ready: {playersReady.Value}/{totalPlayers}";
    }

    public void UpdateInGamePanelText(int currentTours, int totalToursRequired)
    {
        if (playerTextInGame != null)
        {
            playerTextInGame.text = $"Game In Progress... Tours: {currentTours}/{totalToursRequired}";
        }
        else
        {
            Debug.LogError("Player Text In Game is not assigned in GameSceneManager!");
        }
    }

    private void UpdateESCMenuText()
    {
        playerTextESCMenu.text = $"Players Ready: {playersReady.Value}/{totalPlayers}";
    }
    #endregion

    #region Game State
    private void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
    }
    #endregion
}
