using System;

using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public int remainingCurrency = 30;
    private bool _isWinner = false;
    private int _starsCollected = 0;

    public UITextEvent RemainingCurrencyUpdate;
    public static Action<bool> OnGameOver;
    public static Action<int> StarCollected;
    public static Action<int> OnScoreChange;

    private Action CurrencyReducer;

    private void OnEnable()
    {
        ScoreManager.OnScoreChange += ScoredPoints;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreChange -= ScoredPoints;
        CurrencyReducer -= ReduceCurrency;
    }

    void Start()
    {
        SwitchCurrencyReduction();
        RemainingCurrencyUpdate.Raise(remainingCurrency.ToString());
    }

    void SwitchCurrencyReduction()
    {
        switch (Board.lvlBoard.levelCurrency)
        {
            case LevelCurrency.Moves:
                CurrencyReducer = Board.OnUserPlayed;
                break;
            case LevelCurrency.Seconds:
                CurrencyReducer = Board.OnUserPlayed;
                break;
        }
        CurrencyReducer += ReduceCurrency;
    }

    int GetStarsFromScore(int score)
    {
        int starCount = Array.FindIndex(Board.lvlBoard.scoreGoals, starScore => starScore > score);
        return starCount > -1 ? starCount : Board.lvlBoard.scoreGoals.Length;
    }

    public void ReduceCurrency()
    {
        remainingCurrency--;
        RemainingCurrencyUpdate.Raise(remainingCurrency.ToString());

        // Check for game over
        if (remainingCurrency == 0)
        {
            OnGameOver?.Invoke(_isWinner);
        }
    }

    void ScoredPoints(int newScore)
    {
        _isWinner = newScore >= Board.lvlBoard.scoreGoals[0];

        UpdateStarCount(newScore);
        // Check for game over with IsWinner = true
        if (newScore >= Board.lvlBoard.scoreGoals[Board.lvlBoard.scoreGoals.Length - 1])
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

public enum LevelCurrency
{
    Moves,
    Seconds
}
