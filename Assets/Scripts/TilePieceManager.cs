using Sirenix.OdinInspector;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class TilePieceManager : SerializedMonoBehaviour
{
    public static TilePieceManager Instance;
    [BoxGroup("Tiles")]
    public GameObject tileNormalPrefab, tileObstaclePrefab, tileBreakablePrefab, tileDoubleBreakablePrefab;
    [BoxGroup("Bombs")]
    public GameObject adjacentBombPrefab, columnBombPrefab, rowBombPrefab;
    [BoxGroup("Normal Game Pieces")]
    public GameObject[] gamePiecePrefabs;
    [BoxGroup("Colors and Values")]
    [TableList]
    public List<ColorValue> colorValues = new List<ColorValue>();

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
            Debug.LogWarning($"TilePieceManager: {randomIdx} does not contain a valid Gamepiece prefab");
        }
        return gamePiecePrefabs[randomIdx];
    }

    internal ColorValue GetRandomColorValue()
    {
        int randomIdx = Random.Range(0, colorValues.Count);

        if (colorValues[randomIdx] == null)
        {
            Debug.LogWarning($"TilePieceManager: {randomIdx} does not contain a valid color value");
        }
        return colorValues[randomIdx];
    }

    internal Color GetColor(MatchValue match)
    {
        return colorValues.FirstOrDefault(c => c.match == match).color;
    }
}

[System.Serializable]
public class ColorValue
{
    public MatchValue match;
    public Color color = Color.white;    
}
