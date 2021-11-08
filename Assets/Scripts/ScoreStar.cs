using System.Collections;

using UnityEngine;
using UnityEngine.UI;

// plays UI effects when player reaches scoring goal
public class ScoreStar : MonoBehaviour
{
    // reference to the icon 
    public Image star;

    // reference to activation particle effect
    public ParticlePlayer starFX;

    // delay between particles and turning icon on
    public float delay = 0.5f;

    // have we been activated already?
    public bool activated = false;


    void Start()
    {
        SetActive(false);
        StartCoroutine(TestRoutine());
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
    public void Activate()
    {
        // only activate once
        if (activated)
        {
            return;
        }

        // invoke ActivateRoutine coroutine
        StartCoroutine(ActivateRoutine());
    }

    IEnumerator ActivateRoutine()
    {
        // we are activated
        activated = true;

        // play the ParticlePlayer
        if (starFX != null)
        {
            starFX.Play();
        }

        yield return new WaitForSeconds(delay);

        // turn on the icon
        SetActive(true);
    }

    // test ScoreStar after 3 seconds
    IEnumerator TestRoutine()
    {
        yield return new WaitForSeconds(3f);
        Activate();
    }

}
