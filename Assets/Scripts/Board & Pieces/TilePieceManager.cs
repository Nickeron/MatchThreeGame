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
    public GameObject[] adjacentBombPrefab, columnBombPrefab, rowBombPrefab;
    [BoxGroup("Bombs")]
    public GameObject colorBombPrefab;
    [BoxGroup("Normal Game Pieces")]
    public GameObject[] gamePiecePrefabs;
    [BoxGroup("Collectibles")]
    public GameObject[] collectiblePrefabs;
    [BoxGroup("Collectibles")]
    public int maxCollectibles = 3, collectibleCount = 0;
    [BoxGroup("Collectibles")]
    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;
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
        return GetRandomObject(gamePiecePrefabs);
    }

    internal GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }

    private GameObject GetRandomObject(GameObject[] objects)
    {
        int randomIdx = Random.Range(0, objects.Length);

        if (objects[randomIdx] == null)
        {
            Debug.LogWarning($"TilePieceManager: {objects} at {randomIdx} does not contain a valid prefab");
        }
        return objects[randomIdx];
    }

    public GameObject CreateBomb(Tile pos, Board board, BombType type, MatchValue match)
    {
        Bomb bombInstance = Instantiate(GetBombByType(type, match), 
            new Vector3(pos.xIndex, pos.yIndex, 0), 
            Quaternion.identity)
            .GetComponent<Bomb>();

        bombInstance?.Init(board);
        bombInstance?.SetCoord(pos.xIndex, pos.yIndex);
        bombInstance.transform.parent = board.transform;
        return bombInstance.gameObject;
    }

    private GameObject GetBombByType(BombType type, MatchValue match)
    {
        switch (type)
        {
            case BombType.Column:
                return GetBombByValue(columnBombPrefab, match);
            case BombType.Row:
                return GetBombByValue(rowBombPrefab, match);
            case BombType.Adjacent:
                return GetBombByValue(adjacentBombPrefab, match);
            case BombType.Color:
                return colorBombPrefab;
            case BombType.None:
            default:
                return null;
        }
    }

    private GameObject GetBombByValue(GameObject[] bombsOfType, MatchValue match)
    {
        return bombsOfType?.FirstOrDefault(x => x.GetComponent<GamePiece>().matchValue == match);
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

    internal bool CanAddCollectible()
    {
        return (Random.Range(0f, 1f) <= chanceForCollectible &&
                collectiblePrefabs.Count() > 0 &&
                collectibleCount < maxCollectibles);
    }

    internal void CollectibleCreated()
    {
        collectibleCount++;
    }

    internal void CollectibleCollected()
    {
        collectibleCount--;
    }
}

[System.Serializable]
public class ColorValue
{
    public MatchValue match;
    public Color color = Color.white;
}
