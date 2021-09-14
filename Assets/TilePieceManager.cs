using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePieceManager : MonoBehaviour
{
    public static TilePieceManager Instance;
    public GameObject tileNormalPrefab, tileObstaclePrefab, tileBreakablePrefab, tileDoubleBreakablePrefab;
    public GameObject[] gamePiecePrefabs;

    private void Awake()
    {
        Instance = this;
    }

    internal GameObject GetProperPiece(TileType tileType)
    {
        switch (tileType)
        {            
            case TileType.Normal: return tileNormalPrefab;
            case TileType.Obstacle: return tileObstaclePrefab;
            case TileType.Breakable: return tileBreakablePrefab;
            case TileType.DoubleBreakable: return tileDoubleBreakablePrefab;
            default: return tileNormalPrefab;
        }
    }

    internal GameObject GetRandomGamePiece()
    {
        int randomIdx = Random.Range(0, gamePiecePrefabs.Length);

        if (gamePiecePrefabs[randomIdx] == null)
        {
            Debug.LogWarning($"Board: {randomIdx} does not contain a valid Gamepiece prefab");
        }
        return gamePiecePrefabs[randomIdx];
    }
}
