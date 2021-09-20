using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    public int movesLeft = 30;
    public int scoreGoal = 1000;

    public ScreenFader screenFader;
    public TextMeshProUGUI txtLevelName, txtRemainingMoves;

    private Board _board;

    private bool _isReadyToBegin = false, _isGameOver = false, _isWinner = false;

    private const string LEVEL_STRING = "Level";

    // Start is called before the first frame update
    void Start()
    {
        _board = FindObjectOfType<Board>().GetComponent<Board>();

        if (txtLevelName != null)
        {
            txtLevelName.text = $"{LEVEL_STRING} {Board.lvlBoard?.levelNumber}";
        }
        UpdateMoves();
        StartCoroutine(ExecuteGameLoop());
    }

    public void UserPlayed()
    {
        movesLeft--;
        UpdateMoves();
    }

    private void UpdateMoves()
    {
        if(txtRemainingMoves != null)
        {
            txtRemainingMoves.text = movesLeft.ToString();
        }
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine(StartGameRoutine());
        yield return StartCoroutine(PlayGameRoutine());
        yield return StartCoroutine(EndGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        while (!_isReadyToBegin)
        {
            yield return new WaitForSeconds(2f);
            _isReadyToBegin = true;
        }

        screenFader?.FadeOff();

        yield return new WaitForSeconds(0.5f);

        _board?.SetupBoard();
    }

    IEnumerator PlayGameRoutine()
    {
        while (!_isGameOver)
        {
            if (movesLeft == 0)
            {
                _isGameOver = true;
                _isWinner = false;
            }
            yield return null;
        }
    }

    IEnumerator EndGameRoutine()
    {
        screenFader?.FadeOn();

        if (_isWinner)
        {
            Debug.Log("YOU WIN!");
        }
        else
        {
            Debug.Log("YOU LOSE!");
        }

        yield return null;
    }
}
