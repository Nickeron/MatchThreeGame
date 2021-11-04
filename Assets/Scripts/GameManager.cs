using System.Collections;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System;

public class GameManager : Singleton<GameManager>
{
    public static event Action<bool> GameOver;
    public static event Action<Sprite, string, string> OnDisplayMessage;

    public int movesLeft = 30;
    public int scoreGoal = 1000;


    public TextMeshProUGUI txtLevelName, txtRemainingMoves;

    [HorizontalGroup("Message Window Icons", LabelWidth = 50)]
    public Sprite icnLose, icnWin, icnGoal;

    [HideInInspector]
    public static int Level;

    private Board _board;
    private ScreenFader _screenFader;

    public static bool isGameOver { get; private set; } = false;
    private bool _isWinner = false;

    private const string LEVEL_STRING = "Level";

    void Start()
    {
        SetupGame(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += SetupGame;
    }

    void SetupGame(Scene activeScene, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        Level = PlayerPrefs.GetInt(LEVEL_STRING);
        Debug.Log($"Starting Level {Level}");

        _screenFader = FindObjectOfType<ScreenFader>().GetComponent<ScreenFader>();
        _board = FindObjectOfType<Board>().GetComponent<Board>();

        if (txtLevelName != null)
        {
            txtLevelName.text = $"{LEVEL_STRING} {Board.lvlBoard?.levelNumber}";
        }
        UpdateMoves();

        ActOnUserClick(StartTheGame);
        OnDisplayMessage?.Invoke(icnGoal, $"Score Goal\n{scoreGoal}", "Start");
    }

    public static void GoBackOneLevel()
    {
        if (Level > 1)
        {
            Debug.LogWarning("Stepping down 1 level.");
            PlayerPrefs.SetInt(LEVEL_STRING, --Level);
        }
        else
        {
            Level = 1;
            PlayerPrefs.SetInt(LEVEL_STRING, Level);
        }

    }

    public void UserPlayed()
    {
        movesLeft--;
        UpdateMoves();

        // Check for game over
        if (movesLeft == 0 && !isGameOver)
        {
            isGameOver = true;
            _isWinner = false;

            StartCoroutine(EndGameRoutine());
        }
    }

    private void UpdateMoves()
    {
        if (txtRemainingMoves != null)
        {
            txtRemainingMoves.text = movesLeft.ToString();
        }
    }

    IEnumerator StartGameRoutine()
    {
        _screenFader?.FadeOff();

        yield return new WaitForSeconds(0.5f);

        _board?.SetupBoard();

        ScoreManager.ScoredPoints += ScoredPoints;
    }

    void ScoredPoints(int newScore)
    {
        // Check for game over
        if (newScore >= scoreGoal && !isGameOver)
        {
            isGameOver = true;
            _isWinner = true;

            StartCoroutine(EndGameRoutine());
        }
    }

    void ActOnUserClick(Action gameFunction)
    {
        MessageWindow.ButtonPressed += gameFunction;
    }

    void UnsubscribeFromActionList(Action gameFunction)
    {
        MessageWindow.ButtonPressed -= gameFunction;
    }

    IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(2f);
        yield return new WaitUntil(() => !_board.isRefilling);

        _screenFader?.FadeOn();
        GameOver?.Invoke(_isWinner);
        ActOnUserClick(LoadLevel);
        if (_isWinner)
        {
            PlayerPrefs.SetInt(LEVEL_STRING, ++Level);
            OnDisplayMessage?.Invoke(icnWin, $"You WIN!\n{scoreGoal}", "Next");
        }
        else
        {
            OnDisplayMessage?.Invoke(icnLose, $"You lost..\n{scoreGoal}", "Replay");
        }

        yield return null;
    }

    public void StartTheGame()
    {
        UnsubscribeFromActionList(StartTheGame);
        StartCoroutine(StartGameRoutine());
    }

    void LoadLevel()
    {
        UnsubscribeFromActionList(LoadLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
