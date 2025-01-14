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

    private int playersReady = 0;
    private int totalPlayers = 2; // Default to 2 players; can be adjusted for your needs

    private bool isGamePaused = true;

    private void Start()
    {
        ShowStartPanel();

        // Add listeners to buttons
        readyButton.onClick.AddListener(OnReadyButtonClicked); // Fixed here
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

    public void ShowStartPanel()
    {
        SetActivePanel(startPanel);
        PauseGame();
    }

    public void ShowReadyPanel()
    {
        SetActivePanel(readyPanel);
        PauseGame();
        playersReady = 0; // Reset ready count
        UpdateReadyPanelText();
    }

    public void ShowInGamePanel()
    {
        SetActivePanel(inGamePanel);
        ResumeGame();
        UpdateInGamePanelText();
    }

    public void ShowEndGamePanel()
    {
        ShowEndGamePanel("Default Winner");
    }

    public void ShowEndGamePanel(string winner)
    {
        SetActivePanel(endGamePanel);
        PauseGame();
        winnerText.text = $"Winner: {winner}";
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

    public void OnReadyButtonClicked()
    {
        playersReady++;
        UpdateReadyPanelText();

        // If all players are ready, start the game
        if (playersReady >= totalPlayers)
        {
            ShowInGamePanel();
        }
    }

    private void UpdateReadyPanelText()
    {
        playerTextReady.text = $"Players Ready: {playersReady}/{totalPlayers}";
    }

    private void UpdateInGamePanelText()
    {
        playerTextInGame.text = $"Game In Progress...";
    }

    private void UpdateESCMenuText()
    {
        playerTextESCMenu.text = $"Players Ready: {playersReady}/{totalPlayers}";
    }

    public void RestartGame()
    {
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
        // Add your exit session logic here
        ReturnToMainMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

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

    public void ReturnToMainMenu()
    {
        // Reset the game state (e.g., scores, player ready counts, etc.)
        playersReady = 0; // Reset ready count
        Time.timeScale = 1f; // Ensure the game is running

        // Perform any other game-specific reset logic here
        Debug.Log("Game reset and returning to main menu.");

        // Show the Start Panel
        ShowStartPanel();
    }
}
