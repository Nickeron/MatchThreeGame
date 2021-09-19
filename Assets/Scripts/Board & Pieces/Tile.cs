using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable,
    DoubleBreakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public TileType type = TileType.Normal;

    Board _board;
    SpriteRenderer _spriteRenderer;

    public int breakableValue = 0;
    public Sprite[] breakableSprites;
    public Color normalColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        _board = board;

        SetBreakableSprite();
    }

    private void OnMouseDown()
    {
        _board?.ClickTile(this);
    }

    private void OnMouseEnter()
    {
        _board?.DragToTile(this);
    }

    private void OnMouseUp()
    {
        _board?.ReleaseTile();
    }

    public void BreakTile()
    {
        if (type != TileType.Breakable) return;

        StartCoroutine(BreakTileRoutine());
    }

    IEnumerator BreakTileRoutine()
    {
        breakableValue = Mathf.Clamp(--breakableValue, 0, breakableValue);
        yield return new WaitForSeconds(0.25f);

        SetBreakableSprite();

        if (breakableValue == 0)
        {
            type = TileType.Normal;
            _spriteRenderer.color = normalColor;
        }
    }

    private void SetBreakableSprite()
    {
        if (type == TileType.Breakable)
        {
            if (breakableSprites[breakableValue] != null)
            {
                _spriteRenderer.sprite = breakableSprites[breakableValue];
            }
        }
    }
}
