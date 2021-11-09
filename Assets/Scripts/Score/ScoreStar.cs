using System.Collections;

using UnityEngine;
using UnityEngine.UI;

// plays UI effects when player reaches scoring goal
public class ScoreStar : MonoBehaviour
{
    // reference to the icon 
    public Image star;

    [Range(1, 3)]
    public int Index = 1;

    // reference to activation particle effect
    public GameObject starFX;

    // delay between particles and turning icon on
    public float delay = 0.5f;

    // have we been activated already?
    bool activated = false;


    void Start()
    {
        SetActive(false);
        SetPosition();       
    }

    private void OnEnable()
    {
        LevelGoal.StarCollected += Activate;
    }
    
    private void OnDisable()
    {
        LevelGoal.StarCollected -= Activate;
    }

    // turn the icon on or off
    void SetActive(bool state)
    {
        if (star != null)
        {
            star.gameObject.SetActive(state);
        }
    }

    // activate the star 
    public void Activate(int activeIndex)
    {
        // only activate once
        if (activated || Index != activeIndex)
        {
            return;
        }

        // invoke ActivateRoutine coroutine
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        activated = true;

        // play the ParticlePlayer
        Instantiate(starFX, transform.position, Quaternion.identity).GetComponent<ParticlePlayer>()?.Play();

        yield return new WaitForSeconds(delay);

        // turn on the icon
        SetActive(true);
    }

    void SetPosition()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(
            CalculateXPos(transform.parent.GetComponent<RectTransform>().rect.width), 0);
    }

    float CalculateXPos(float sliderWidth)
    {
        return (sliderWidth * GetScorePercentage()) - (sliderWidth * 0.5f);
    }

    float GetScorePercentage()
    {
        return (float)LevelGoal.scoreGoals[Index - 1] / LevelGoal.scoreGoals[LevelGoal.scoreGoals.Length - 1];
    }
}
