using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public GameObject clearFX, breakFX, doubleBreakFX, bombFX;

    private void Start()
    {
        Board.OnPieceCleared += ClearPieceFXAt;
        Board.OnTileBroke += BreakTileFXAt;
    }

    public void ClearPieceFXAt(int x, int y, int z = 0, bool isBomb = false)
    {
        PlayFXAt(isBomb? bombFX : clearFX, new Vector3(x, y, z));
    }

    public void BreakTileFXAt(int breakableValue, int x, int y)
    {
        PlayFXAt(breakableValue > 1? doubleBreakFX : breakFX, new Vector3(x, y, 0));
    }

    public void BombFXAt(int x, int y, int z = 0)
    {
        PlayFXAt(bombFX, new Vector3(x, y, z));
    }

    private void PlayFXAt(GameObject FX, Vector3 position)
    {
        if(FX != null)
        {
            Instantiate(FX, position, Quaternion.identity).GetComponent<ParticlePlayer>()?.Play();
        }
    }

    private void OnDestroy()
    {
        Board.OnPieceCleared -= ClearPieceFXAt;
        Board.OnTileBroke -= BreakTileFXAt;
    }
}
