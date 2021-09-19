using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class ScreenFader : MonoBehaviour
{
    public float solidAlpha = 1f, clearAlpha = 0f, delay = 0f, timeToFade = 1f;

    private MaskableGraphic _graphic;

    void Awake()
    {
        _graphic = GetComponent<MaskableGraphic>();
    }

    private IEnumerator FadeRoutine(float alpha)
    {
        yield return new WaitForSeconds(delay);

        _graphic.CrossFadeAlpha(alpha, timeToFade, ignoreTimeScale: true);
    }

    public void FadeOn()
    {
        StartCoroutine(FadeRoutine(solidAlpha));
    } 
    
    public void FadeOff()
    {
        StartCoroutine(FadeRoutine(clearAlpha));
    }
}
