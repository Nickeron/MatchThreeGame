using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ScoreMeter : MonoBehaviour
{
    private void OnEnable()
    {
        ScoreManager.OnScoreChange += UpdateScoreMeter;
    }
    
    private void OnDisable()
    {
        ScoreManager.OnScoreChange -= UpdateScoreMeter;
    }

    void UpdateScoreMeter (int newScore)
    {
        GetComponent<Slider>().value = (float) newScore / LevelGoal.scoreGoals[LevelGoal.scoreGoals.Length - 1];
    }
}
