using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject readyPanel;
    public GameObject inGamePanel;
    public GameObject endGamePanel;
    public GameObject escMenuPanel;

    [Header("Ready Panel Components")]
    public Button readyButton;
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
    private int playersReady = 0;
    private int currentTours = 0;

    private bool isGamePaused = true;

    private void Start()
    {
        ShowStartPanel();

        // Add button listeners
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(ReturnToMainMenu);
        exitButtonEnd.onClick.AddListener(ExitGame);
        resetCarButton.onClick.AddListener(ResetCar);
        exitSessionButton.onClick.AddListener(ExitSession);
        exitGameButton.onClick.AddListener(ExitGame);
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
        playersReady = 0;
        currentTours = 0;
    }

    public void ShowReadyPanel()
    {
        SetActivePanel(readyPanel);
        PauseGame();
        playersReady = 0;
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
    public void OnReadyButtonClicked()
    {
        playersReady++;
        UpdateReadyPanelText();

        if (playersReady >= totalPlayers)
        {
            ShowInGamePanel();
        }
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
        playersReady = 0;
        currentTours = 0;
        Time.timeScale = 1f;
        Debug.Log("Game reset and returning to main menu.");
        ShowStartPanel();
    }
    #endregion

    #region UI Updates
    private void UpdateReadyPanelText()
    {
        playerTextReady.text = $"Players Ready: {playersReady}/{totalPlayers}";
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
        playerTextESCMenu.text = $"Players Ready: {playersReady}/{totalPlayers}";
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
