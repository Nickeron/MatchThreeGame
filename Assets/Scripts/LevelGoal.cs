using UnityEngine;

using Array = System.Array;

public class LevelGoal : MonoBehaviour
{
    public int[] scoreGoals = new int[3] { 1000, 2000, 3000 };
    public int movesLeft = 30;
    private bool _isWinner = false;
    private int _starsCollected = 0;

    public UITextEvent RemainingMovesUpate;
    public static event System.Action<bool> OnGameOver;
    public static event System.Action<int> StarCollected;

    private void OnEnable()
    {
        ScoreManager.ScoredPoints += ScoredPoints;
        Board.OnUserPlayed += UserPlayed;
    }

    private void OnDisable()
    {
        ScoreManager.ScoredPoints -= ScoredPoints;
        Board.OnUserPlayed -= UserPlayed;
    }

    void Start()
    {
        Array.Sort(scoreGoals);
        RemainingMovesUpate.Raise(movesLeft.ToString());
    }

    int GetStarsFromScore(int score)
    {
        int starCount = Array.FindIndex(scoreGoals, starScore => starScore > score);
        return starCount > -1 ? starCount : scoreGoals.Length;
    }

    public void UserPlayed()
    {
        movesLeft--;
        RemainingMovesUpate.Raise(movesLeft.ToString());

        // Check for game over
        if (movesLeft == 0)
        {
            OnGameOver?.Invoke(_isWinner);
        }
    }

    void ScoredPoints(int newScore)
    {
        _isWinner = newScore >= scoreGoals[0];

        UpdateStarCount(newScore);

        // Check for game over with IsWinner = true
        if (newScore >= scoreGoals[scoreGoals.Length - 1])
        {
            OnGameOver?.Invoke(true);
        }
    }

    void UpdateStarCount(int score)
    {
        int newStarsCount = GetStarsFromScore(score);
        if (newStarsCount == _starsCollected) return;

        _starsCollected = newStarsCount;
        StarCollected?.Invoke(_starsCollected);
    }
}
