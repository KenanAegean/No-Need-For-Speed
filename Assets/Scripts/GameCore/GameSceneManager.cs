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
    public GameObject joinPanel;
    public GameObject inGamePanel;
    public GameObject endGamePanel;
    public GameObject escMenuPanel;

    [Header("Join Panel Components")]
    public Button hostButton;
    public Button clientButton;
    public Button mainMenuButton;
    public Button exitButton;

    [Header("In Game Panel Components")]
    public TMP_Text playerTextInGame;
    public TMP_Text readyText;
    public TMP_Text countdownText;

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
        currentTours = 0;
    }

    public void ShowJoinPanel()
    {
        SetActivePanel(joinPanel);
        PauseGame();
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
    }

    public void ShowESCMenuPanel()
    {
        inGamePanel.SetActive(false);
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
        joinPanel.SetActive(false);
        inGamePanel.SetActive(false);
        endGamePanel.SetActive(false);
        escMenuPanel.SetActive(false);

        panel.SetActive(true);
    }
    #endregion

    #region Button Logic

    public void JoinAsHost()
    {
        //ResumeGame();
        Debug.Log("Starting Host...");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully.");
            joinPanel.SetActive(false);
            StartGame();
        }
        else
        {
            //Debug.LogError("Failed to start Host.");
        }
        //joinPanel.SetActive(false);// delete later
    }

    public void JoinAsClient()
    {
        //ResumeGame();
        Debug.Log("Starting Client...");
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            joinPanel.SetActive(false);
            StartGame();
        }
        else
        {
            //Debug.LogError("Failed to start Client.");
        }
        joinPanel.SetActive(false); //delete later
    }


    

    private void StartGame()
    {
        Debug.Log("All players have joined. Starting the game...");

        // Deactivate the join panel and activate in-game UI
        if (joinPanel != null)
        {
            joinPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Join Panel is not assigned in GameSceneManager!");
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
        // Disconnect the player
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Player disconnected during restart.");
        }

        // Reset game state if necessary
        currentTours = 0;

        // Show the join panel
        ShowJoinPanel();
        Debug.Log("Restarting game and showing JoinPanel.");
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
        // Disconnect the player
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Player disconnected and returning to Main Menu.");
        }

        // Reset game state if necessary
        currentTours = 0;

        // Show the start panel
        ShowStartPanel();
        Debug.Log("Returning to MainMenu and showing StartPanel.");
    }

    public void HandleGlobalDisconnect()
    {
        // Disconnect all players and shutdown the network
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("All players disconnected.");
        }

        // Return to the main menu
        ReturnToMainMenu();
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                // Host disconnects: Disconnect everyone and return to the main menu
                Debug.Log("Host disconnected. Disconnecting all players.");
                HandleGlobalDisconnect();
            }
            else
            {
                // A client disconnected while the host is still active
                Debug.Log($"Client {clientId} disconnected. Host will also disconnect and return to the main menu.");
                HandleGlobalDisconnect();
            }
        }
        else
        {
            // Client is not the host and is disconnecting
            Debug.Log($"Client {clientId} disconnected. Disconnecting and returning to the main menu.");
            HandleGlobalDisconnect();
        }
    }



    #endregion

    #region UI Updates

    public void UpdateInGamePanelText(int currentTours, int totalToursRequired)
    {
        if (playerTextInGame != null)
        {
            //playerTextInGame.text = $"Game In Progress... Tours: {currentTours}/{totalToursRequired}";
        }
        else
        {
            Debug.LogError("Player Text In Game is not assigned in GameSceneManager!");
        }
    }

    public void UpdatePlayerTourText(string playerName, int currentTours, int totalTours)
    {
        if (playerTextInGame != null)
        {
            Debug.Log($"Updating UI: {playerName} - Tours: {currentTours}/{totalTours}");
            playerTextInGame.text = $"{playerName}: {currentTours}/{totalTours}";
        }
        else
        {
            Debug.LogError("playerTextInGame is not assigned in GameSceneManager!");
        }
    }


    public void ShowWinner(string winnerName)
    {
        if (winnerText != null)
        {
            winnerText.text = $"{winnerName} wins the race!";
        }
        ShowEndGamePanel();
    }

    public void UpdateReadyText(int ready, int total)
    {
        readyText.text = $"{ready}/{total} Ready";
    }

    public void UpdateCountdownText(string message)
    {
        countdownText.text = message;
    }

    private void UpdateESCMenuText()
    {
        //add logic
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
