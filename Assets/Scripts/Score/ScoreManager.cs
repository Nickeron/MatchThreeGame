using System;
using System.Collections;

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> OnScoreChange;
    public UITextEvent ScoreUpdate;

    public int _multiBonus = 20;
    int _currentScore = 0, _counterValue = 0, _increment = 5;
    int _multiCain, _bonus;

    void Start()
    {
        ScoreUpdate.Raise(_counterValue.ToString());
        GamePiece.PieceCleared += AddPoints;
        Board.OnBonusUpdate += HandleBonusUpdate;
    }

    public bool CheckScore(int value)
    {
        return _currentScore >= value;
    }

    #region RESETS
    public void ResetMultiplier()
    {
        _multiCain = 0;
    }
    
    public void CalculateBonus(int nPieces)
    {
        _bonus = nPieces * _multiBonus;
    }
    #endregion

    #region INCREASE  

    private void HandleBonusUpdate(bool increase)
    {
        // Increase or reset the bonus
        _multiCain = increase ? _multiCain + 1 : 0;
    }

    public void AddPoints(Vector3 _, int value)
    {
        //Debug.Log($"Adding {value} points to Score, for piece cleared at {_}");
        _currentScore += (value * _multiCain) + _bonus;
        OnScoreChange?.Invoke(_currentScore);
        StartCoroutine(CountScoreRoutine());
    }
    #endregion

    IEnumerator CountScoreRoutine()
    {
        int iterations = 0;
        while(_counterValue < _currentScore && iterations < 10000)
        {
            ScoreUpdate.Raise(_counterValue.ToString());
            _counterValue += _increment;
            iterations++;
            yield return null;
        }

        _counterValue = _currentScore;
        ScoreUpdate.Raise(_counterValue.ToString());
    }

    private void OnDisable()
    {
        GamePiece.PieceCleared -= AddPoints;
        Board.OnBonusUpdate -= HandleBonusUpdate;
    }
}
