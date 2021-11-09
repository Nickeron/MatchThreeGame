using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static event Action GameStart;
    public static event Action<bool> GameOver;
    public static event Action<MessageType> OnDisplayMessage;
    public UITextEvent LevelNameSet;

    [HideInInspector]
    public static int Level;

    private static bool isGameOver;

    private bool _isBoardRefilling = true;

    private const string LEVEL_STRING = "Level";

    private void OnEnable()
    {
        isGameOver = false;
        SceneManager.sceneLoaded += SetupGame;
        LevelGoal.OnGameOver += HandleGameOver;
        Board.OnRefill += IsBoardRefilling;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SetupGame;
        LevelGoal.OnGameOver -= HandleGameOver;
        Board.OnRefill -= IsBoardRefilling;
    }

    void SetupGame(Scene activeScene, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        Level = PlayerPrefs.GetInt(LEVEL_STRING);
        Debug.Log($"Starting Level {Level}");

        LevelNameSet.Raise($"{LEVEL_STRING} {Board.lvlBoard?.levelNumber}");

        ActOnUserClick(StartTheGame);
        OnDisplayMessage?.Invoke(MessageType.Goal);
    }

    public static bool CanUserPlay() => !isGameOver;

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

    public void HandleGameOver(bool isWinner)
    {
        // Check for game over
        if (!isGameOver)
        {
            isGameOver = true;

            StartCoroutine(EndGameRoutine(isWinner));
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

    IEnumerator EndGameRoutine(bool isWinner)
    {
        yield return new WaitForSeconds(2f);
        yield return new WaitUntil(() => !_isBoardRefilling);

        GameOver?.Invoke(isWinner);
        ActOnUserClick(LoadLevel);
        if (isWinner)
        {
            PlayerPrefs.SetInt(LEVEL_STRING, ++Level);
            OnDisplayMessage?.Invoke(MessageType.Win);
        }
        else
        {
            OnDisplayMessage?.Invoke(MessageType.Lose);
        }

        yield return null;
    }

    void IsBoardRefilling(bool state)
    {
        _isBoardRefilling = state;
    }

    public void StartTheGame()
    {
        UnsubscribeFromActionList(StartTheGame);

        GameStart?.Invoke();
    }

    void LoadLevel()
    {
        UnsubscribeFromActionList(LoadLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
