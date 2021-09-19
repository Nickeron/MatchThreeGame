using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : GamePiece
{
    public bool clearedByBomb = false, clearedAtBottom = true;

    private void Awake()
    {
        matchValue = MatchValue.None;
        TilePieceManager.Instance.CollectibleCreated();
    }

    private void OnDestroy()
    {
        TilePieceManager.Instance.CollectibleCollected();
    }
}
