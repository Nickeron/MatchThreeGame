using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Board : MonoBehaviour
{
    public int borderSize = 0;
    public int fillYOffset = 10;
    public float fillFallTime = 0.1f;

    public float swapTime = 0.2f;

    Tile[,] _allTiles;
    GamePiece[,] _allGamePieces;

    public Tile _clickedTile;
    public Tile _targetTile;

    bool _playerInputEnabled = true;

    public StartingObject[] startingPieces;

    GameObject _clickedTileBomb;
    GameObject _targetTileBomb;

    private LevelBoardSO _lvlBoard;
    private ParticleManager _particleManager;
    private const string BOARD_LOCATION = "SO/BoardLvl_";
    private int currentLevel = 1;

    private void Awake()
    {
        LoadBoardForLevel();
    }

    void Start()
    {
        _allTiles = new Tile[_lvlBoard.width, _lvlBoard.height];
        _allGamePieces = new GamePiece[_lvlBoard.width, _lvlBoard.height];

        SetupTiles();
        SetupGamePieces();
        SetupCamera();
        FillBoard(fillYOffset, fillFallTime);

        _particleManager = FindObjectOfType<ParticleManager>();
    }

    #region SETUP
    void LoadBoardForLevel()
    {
        _lvlBoard = Resources.Load($"{BOARD_LOCATION}{currentLevel}") as LevelBoardSO;
    }
    void SetupCamera()
    {
        float horizCenter = (_lvlBoard.height - 1) / 2f;
        float vertiCenter = (_lvlBoard.width - 1) / 2f;

        Camera.main.transform.position = new Vector3(vertiCenter, horizCenter, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)_lvlBoard.height / 2f + (float)borderSize;
        float horizontalSize = ((float)_lvlBoard.width / 2f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    void SetupTiles()
    {
        for (int x = 0; x < _lvlBoard.width; x++)
        {
            for (int y = 0; y < _lvlBoard.height; y++)
            {
                CreateTile(TilePieceManager.Instance.GetProperPiece(_lvlBoard.startingBoard[x, y]), x, y, 0);
            }
        }
    }

    void SetupGamePieces()
    {
        foreach (StartingObject sPiece in startingPieces)
        {
            if (sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity);
                CreateGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillFallTime);
            }
        }
    }
    #endregion SETUP

    #region CREATE
    private void CreateTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);

            tile.name = $"Tile ({x}, {y})";

            _allTiles[x, y] = tile.GetComponent<Tile>();
            _allTiles[x, y].Init(x, y, this);

            tile.transform.parent = transform;
        }
    }

    private GamePiece CreateGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float fallTime = 0.1f)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            prefab.transform.parent = transform;

            GamePiece randomPiece = prefab.GetComponent<GamePiece>();
            randomPiece.Init(this);
            PlaceGamePiece(randomPiece, x, y);

            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                randomPiece.Move(x, y, fallTime);
            }

            return randomPiece;
        }
        return null;
    }

    private GameObject CreateBomb(GameObject prefab, Tile pos)
    {
        if (prefab != null && IsWithinBounds(pos.xIndex, pos.yIndex))
        {
            Bomb bombInstance = Instantiate(prefab, new Vector3(pos.xIndex, pos.yIndex, 0), Quaternion.identity).GetComponent<Bomb>();
            bombInstance?.Init(this);
            bombInstance?.SetCoord(pos.xIndex, pos.yIndex);
            bombInstance.transform.parent = transform;
            return bombInstance.gameObject;
        }
        return null;
    }
    #endregion CREATE

    #region INSERT
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning($"Board: Invalid GamePiece!");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoord(x, y);

        if (IsWithinBounds(x, y)) _allGamePieces[x, y] = gamePiece;
    }

    void FillBoard(int falseOffset = 0, float fallTime = 0.1f)
    {
        int maxIterations = 100;

        for (int i = 0; i < _lvlBoard.width; i++)
        {
            for (int j = 0; j < _lvlBoard.height; j++)
            {
                if (IsSpaceAvailable(i, j))
                {
                    // Fill the position with a collectible depending on the conditions
                    if (j == _lvlBoard.height - 1 && TilePieceManager.Instance.CanAddCollectible())
                        FillRandomAt(i, j, falseOffset, fallTime, isCollectible: true);

                    // Otherwise fill the position with a regular gamepiece
                    else
                    {
                        FillRandomAt(i, j, falseOffset, fallTime);
                        int iterations = 0;

                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAt(i, j);
                            FillRandomAt(i, j, falseOffset, fallTime);

                            iterations++;
                            if (iterations >= maxIterations)
                            {
                                Debug.LogError("While broke, searching for a random piece");
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float fallTime = 0.1f, bool isCollectible = false)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject randomObject = isCollectible ? TilePieceManager.Instance.GetRandomCollectible() : TilePieceManager.Instance.GetRandomGamePiece();
            return CreateGamePiece(Instantiate(randomObject, Vector3.zero, Quaternion.identity), x, y, falseYOffset, fallTime);
        }
        return null;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(fillYOffset, fillFallTime);
        yield return null;
    }

    private GameObject InsertBomb(Tile pos, Vector2 swapDirection, List<GamePiece> gamePieces, GamePiece targetPiece)
    {
        GameObject bomb = null;

        if (gamePieces.Count >= 4)
        {
            if (IsCornerMatch(gamePieces))
            {
                // Insert Adjacent Bomb
                if (TilePieceManager.Instance.adjacentBombPrefab != null)
                {
                    bomb = CreateBomb(TilePieceManager.Instance.adjacentBombPrefab, pos);
                }
            }
            else
            {
                // Insert Color Bomb
                if (gamePieces.Count >= 5 && TilePieceManager.Instance.colorBombPrefab != null)
                {
                    bomb = CreateBomb(TilePieceManager.Instance.colorBombPrefab, pos);
                }
                else if (swapDirection.x != 0)
                {
                    // Insert row bomb
                    if (TilePieceManager.Instance.rowBombPrefab != null)
                    {
                        bomb = CreateBomb(TilePieceManager.Instance.rowBombPrefab, pos);
                    }
                }
                else
                {
                    //Insert column bomb
                    if (TilePieceManager.Instance.columnBombPrefab != null)
                    {
                        bomb = CreateBomb(TilePieceManager.Instance.columnBombPrefab, pos);
                    }
                }
            }
            GamePiece bombPiece = bomb.GetComponent<GamePiece>();
            if (bombPiece != null && !IsColorBomb(bombPiece))
                bombPiece.SetColor(targetPiece);
        }
        return bomb;
    }

    private void EnablePossibleBombs()
    {
        if (_clickedTileBomb != null)
        {
            EnableBomb(_clickedTileBomb);
            _clickedTileBomb = null;
        }

        if (_targetTileBomb != null)
        {
            EnableBomb(_targetTileBomb);
            _targetTileBomb = null;
        }
    }

    private void EnableBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (IsWithinBounds(x, y))
        {
            //Debug.Log($"Enabling bomb at {x},{y}");
            _allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }
    #endregion INSERT

    #region CHECKS
    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < _lvlBoard.width && y < _lvlBoard.height);
    }

    bool IsSpaceAvailable(int x, int y, bool notNull = false)
    {
        return ((notNull ^ _allGamePieces[x, y] == null) && _allTiles[x, y].type != TileType.Obstacle);
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false, horizontal = false;
        int xStart = -1, yStart = -1;

        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                // If this is the first piece we check, set it as first and skip checking
                if (xStart == -1 || yStart == -1)
                {
                    xStart = gamePiece.xIndex;
                    yStart = gamePiece.yIndex;
                    continue;
                }

                if (gamePiece.xIndex != xStart && gamePiece.yIndex == yStart)
                    horizontal = true;
                if (gamePiece.xIndex == xStart && gamePiece.yIndex != yStart)
                    vertical = true;
            }
        }
        return horizontal && vertical;
    }

    // Used to check whether the column has finished collapsing
    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                if (gamePiece.transform.position.y - (float)gamePiece.yIndex > 0.05f)
                {
                    //Debug.Log($"{gamePiece.transform.position.y - (float)gamePiece.yIndex}");
                    return false;
                }
                gamePiece.transform.position = new Vector2(gamePiece.transform.position.x, gamePiece.yIndex);
            }
        }
        return true;
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        // Only need to check left and down since we are filling  
        // the board from left to right and from down-up
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null) leftMatches = new List<GamePiece>();
        if (downMatches == null) downMatches = new List<GamePiece>();

        return leftMatches.Count > 0 || downMatches.Count > 0;
    }

    bool IsNextTo(Tile endTile)
    {
        return (Mathf.Abs(_clickedTile.xIndex - endTile.xIndex) == 1 && _clickedTile.yIndex == endTile.yIndex) ||
               (Mathf.Abs(_clickedTile.yIndex - endTile.yIndex) == 1 && _clickedTile.xIndex == endTile.xIndex);
    }

    bool IsColorBomb(GamePiece piece)
    {
        return piece?.GetComponent<Bomb>()?.type == BombType.Color;
    }

    List<GamePiece> CheckForColorBombs(GamePiece clicked, GamePiece target)
    {
        List<GamePiece> affected = new List<GamePiece>();
        if (IsColorBomb(clicked))
        {
            if (!IsColorBomb(target))
            {
                clicked.matchValue = target.matchValue;
                return FindAllMatchValue(clicked.matchValue);
            }
            else
            {
                foreach (GamePiece piece in _allGamePieces)
                {
                    affected.Add(piece);
                }
                return affected;
            }
        }
        else if (IsColorBomb(target))
        {
            target.matchValue = clicked.matchValue;
            return FindAllMatchValue(clicked.matchValue);
        }
        return affected;
    }
    #endregion CHECKS

    #region INTERACTION
    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (_clickedTile != null && IsNextTo(tile))
        {
            _targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (_clickedTile != null && _targetTile != null)
        {
            SwitchTiles(_clickedTile, _targetTile);
        }

        _clickedTile = null;
        _targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (_playerInputEnabled)
        {
            GamePiece clickedPiece = _allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = _allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                //Debug.Log($"Moving {clickedTile.name} to {targetTile.name}");

                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> coloredBombMatches = CheckForColorBombs(clickedPiece, targetPiece);

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && coloredBombMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);

                    // Check if there is a bomb after the switch, for both tiles' matches
                    _clickedTileBomb = InsertBomb(clickedTile, swipeDirection, clickedPieceMatches, targetPiece);
                    _targetTileBomb = InsertBomb(targetTile, swipeDirection, targetPieceMatches, clickedPiece);

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).Union(coloredBombMatches).ToList());
                }
            }
        }
    }
    #endregion INTERACTION

    #region MATCHES
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = _allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX, nextY, maxValue = _lvlBoard.width > _lvlBoard.height ? _lvlBoard.width : _lvlBoard.height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY)) break;

            GamePiece nextPiece = _allGamePieces[nextX, nextY];

            if (nextPiece == null) break;

            //Debug.Log($"Comparing {startPiece.matchValue} with {nextPiece.matchValue}");
            if (nextPiece.matchValue == startPiece.matchValue &&
                !matches.Contains(nextPiece) &&
                nextPiece.matchValue != MatchValue.None)
            {
                //Debug.LogWarning($"It's a match! at {nextPiece.xIndex}, {nextPiece.yIndex}");
                matches.Add(nextPiece);
            }
            else break;
        }

        if (matches.Count >= minLength) return matches;
        //else Debug.Log($"Could not find any matches");

        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        return FindDirectionMatches(startX, startY, new Vector2(0, 1));
    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        return FindDirectionMatches(startX, startY, new Vector2(1, 0));
    }

    List<GamePiece> FindDirectionMatches(int startX, int startY, Vector2 firstDirection, int minLength = 3)
    {
        //Debug.Log($"Searching in the direction {firstDirection} and {-firstDirection}");

        List<GamePiece> directionMatches = FindMatches(startX, startY, firstDirection, 2);
        List<GamePiece> oppositeDirMatches = FindMatches(startX, startY, -firstDirection, 2);

        if (directionMatches == null)
            directionMatches = new List<GamePiece>();

        if (oppositeDirMatches == null)
            oppositeDirMatches = new List<GamePiece>();

        var combinedMatches = directionMatches.Union(oppositeDirMatches).ToList();

        //Debug.Log($"Found {combinedMatches.Count} matches");

        return combinedMatches.Count >= minLength ? combinedMatches : null;
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertiMatches = FindVerticalMatches(x, y, minLength);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertiMatches == null)
        {
            vertiMatches = new List<GamePiece>();
        }

        var combinedMatches = horizMatches.Union(vertiMatches).ToList();
        return combinedMatches;
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < _lvlBoard.width; i++)
        {
            for (int j = 0; j < _lvlBoard.height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches.Union(matches).ToList();
            }
        }

        return combinedMatches;
    }

    List<GamePiece> FindAllMatchValue(MatchValue colorValue)
    {
        List<GamePiece> coloredPieces = new List<GamePiece>();

        for (int i = 0; i < _lvlBoard.width; i++)
        {
            for (int j = 0; j < _lvlBoard.height; j++)
            {
                if (_allGamePieces[i, j] != null && _allGamePieces[i, j].matchValue == colorValue)
                {
                    coloredPieces.Add(_allGamePieces[i, j]);
                }
            }
        }
        return coloredPieces;
    }

    List<GamePiece> FindCollectiblesAt(int row)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < _lvlBoard.width; i++)
        {
            if (_allGamePieces[i, row] != null && _allGamePieces[i, row].GetComponent<Collectible>() != null)
            {
                foundCollectibles.Add(_allGamePieces[i, row]);
            }
        }
        Debug.Log($"Found {foundCollectibles.Count} collectibles at {row}");
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int y = 0; y < _lvlBoard.height; y++)
        {
            foundCollectibles = foundCollectibles.Union(FindCollectiblesAt(y)).ToList();
        }
        return foundCollectibles;
    }
    #endregion MATCHES

    #region HIGHLIGHT
    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);

        List<GamePiece> combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece);
            }
        }
    }

    private void HighlightTileOff(int x, int y)
    {
        if (_allTiles[x, y].type != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    private void HighlightTileOn(GamePiece piece)
    {
        SpriteRenderer spriteRenderer = _allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
        spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
    }

    void HighlightAllMatches()
    {
        for (int i = 0; i < _lvlBoard.width; i++)
        {
            for (int j = 0; j < _lvlBoard.height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece);
            }
        }
    }
    #endregion HIGHLIGHT

    #region CLEARING
    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = _allGamePieces[x, y];

        if (pieceToClear != null)
        {
            _allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighlightTileOff(x, y);
    }

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                _particleManager?.ClearPieceFXAt(piece.xIndex, piece.yIndex, isBomb: bombedPieces.Contains(piece));
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < _lvlBoard.width; i++)
        {
            for (int j = 0; j < _lvlBoard.height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    void BreakTileAt(int x, int y)
    {
        Tile breakableTile = _allTiles[x, y];
        if (breakableTile != null)
        {
            _particleManager?.BreakTileFXAt(breakableTile.breakableValue, x, y);
            breakableTile.BreakTile();
        }
    }

    void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null && _allTiles[piece.xIndex, piece.yIndex].type == TileType.Breakable)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < _lvlBoard.height - 1; i++)
        {
            if (IsSpaceAvailable(column, i))
            {
                for (int j = i + 1; j < _lvlBoard.height; j++)
                {
                    if (_allGamePieces[column, j] != null)
                    {
                        _allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        _allGamePieces[column, i] = _allGamePieces[column, j];
                        _allGamePieces[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(_allGamePieces[column, i]))
                        {
                            movingPieces.Add(_allGamePieces[column, i]);
                        }

                        _allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> collapsingColumns = GetColumns(gamePieces);

        foreach (int col in collapsingColumns)
        {
            movingPieces = movingPieces.Union(CollapseColumn(col)).ToList();
        }

        return movingPieces;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        _playerInputEnabled = false;

        List<GamePiece> matches = gamePieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            yield return StartCoroutine(RefillRoutine());

            matches = FindAllMatches();
        }
        while (matches.Count > 0);

        _playerInputEnabled = true;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces, float delayBetweenMoves = 0.2f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePieces);

        yield return new WaitForSeconds(delayBetweenMoves);

        bool isFinished = false;

        while (!isFinished)
        {
            // Checking for bombs as well!
            var bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            // Doing it twice, to enable CHAIN BOMBS!!
            bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();           

            ClearPieceAt(gamePieces, bombPieces);
            BreakTileAt(gamePieces);
            EnablePossibleBombs();

            yield return new WaitForSeconds(delayBetweenMoves);

            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenMoves);

            matches = FindMatchesAt(movingPieces);
            // Checking for COLLECTIBLES at the bottom row
            var collectedPieces = FindCollectiblesAt(0);
            TilePieceManager.Instance.Collected(collectedPieces.Count);
            Debug.Log($"Checked for collectibles at 0. Found {collectedPieces.Count}");

            // Add those to be cleared
            matches = matches.Union(collectedPieces).ToList();
            Debug.Log($"Found {matches.Count} new matches!");

            if (matches.Count == 0)
            {
                Debug.Log("No other matches found!");
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
    }
    #endregion CLEARING

    #region GETTERS
    List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece gamePiece in gamePieces)
        {
            if (!columns.Contains(gamePiece.xIndex))
            {
                columns.Add(gamePiece.xIndex);
            }
        }
        return columns;
    }

    List<GamePiece> GetRowPieces(int row)
    {
        return GetLanePieces(row);
    }

    List<GamePiece> GetColumnPieces(int column)
    {
        return GetLanePieces(column, isRow: false);
    }

    List<GamePiece> GetLanePieces(int lane, bool isRow = true)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        int limit = isRow ? _lvlBoard.width : _lvlBoard.height;

        for (int index = 0; index < limit; index++)
        {
            GamePiece gamePiece = isRow ? _allGamePieces[index, lane] : _allGamePieces[lane, index];

            if (gamePiece != null)
            {
                gamePieces.Add(gamePiece);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i, j) && _allGamePieces[i, j] != null)
                {
                    gamePieces.Add(_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();

        foreach (GamePiece gamePiece in gamePieces)
        {
            if (gamePiece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bombPiece = gamePiece.GetComponent<Bomb>();

                if (bombPiece != null)
                {
                    switch (bombPiece.type)
                    {
                        case BombType.None:
                            break;
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bombPiece.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bombPiece.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bombPiece.xIndex, bombPiece.yIndex, 1);
                            break;
                        case BombType.Color:
                            //TODO: Fill this in
                            break;
                    }
                    piecesToClear.RemoveAll(piece => !piece.GetComponent<Collectible>()?.clearedByBomb ?? false);
                    allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
                }
            }
        }
        return allPiecesToClear;
    }
    #endregion GETTERS
}
