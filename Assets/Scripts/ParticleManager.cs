using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject clearFX, breakFX, doubleBreakFX;

    public void ClearPieceFXAt(int x, int y, int z = 0)
    {
        PlayFXAt(clearFX, new Vector3(x, y, z));
    }

    public void BreakTileFXAt(int breakableValue, int x, int y, int z = 0)
    {
        PlayFXAt(breakableValue > 1? doubleBreakFX : breakFX, new Vector3(x, y, z));
    }

    private void PlayFXAt(GameObject FX, Vector3 position)
    {
        if(FX != null)
        {
            Instantiate(FX, position, Quaternion.identity).GetComponent<ParticlePlayer>()?.Play();
        }
    }
}
