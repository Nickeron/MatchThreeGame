using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    public int _multiBonus = 20;
    int _currentScore = 0, _counterValue = 0, _increment = 5;
    int _multiCain, _bonus;

    public Text txtScore;

    void Start()
    {
        UpdateScoreText(_currentScore);
    }

    void UpdateScoreText(int scoreValue)
    {
        if(txtScore != null)
        {
            txtScore.text = scoreValue.ToString();
        }
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
    public void IncreaseMultiplier()
    {
        _multiCain++;
    }   

    public void AddPoints(int value)
    {
        Debug.Log($"Adding {value} points to Score");
        _currentScore += (value * _multiCain) + _bonus;
        StartCoroutine(CountScoreRoutine());
    }
    #endregion

    IEnumerator CountScoreRoutine()
    {
        int iterations = 0;
        while(_counterValue < _currentScore && iterations < 10000)
        {
            UpdateScoreText(_counterValue);
            _counterValue += _increment;
            iterations++;
            yield return null;
        }

        _counterValue = _currentScore;
        UpdateScoreText(_counterValue);
    }
}
