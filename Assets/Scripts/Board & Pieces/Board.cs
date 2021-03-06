using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[RequireComponent(typeof(BoardDeadlock))]
[RequireComponent(typeof(BoardShuffle))]
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

    public static Action<bool> OnBonusUpdate;
    public static Action<int> OnBonusCalculate;
    public static Action<int, int, int, bool> OnPieceCleared;
    public static Action<int, int, int> OnTileBroke;
    public static Action OnUserPlayed;
    public static Func<GamePiece[,], bool> OnFillFinished;
    public static Action<bool> OnRefill;

    public StartingObject[] startingPieces;

    GameObject _clickedTileBomb;
    GameObject _targetTileBomb;

    internal static LevelBoardSO lvlBoard;

    private const string BOARD_LOCATION = "SO/BoardLvl_";

    private void OnEnable()
    {
        LoadBoardForLevel();

        // Make sure that score goals are in ascending order
        Array.Sort(lvlBoard.scoreGoals);

        _allTiles = new Tile[lvlBoard.width, lvlBoard.height];
        _allGamePieces = new GamePiece[lvlBoard.width, lvlBoard.height];

        GameManager.GameStart += SetupBoard;
        BoardShuffle.OnBoardShuffled += FillBoardFromList;
    }

    private void OnDisable()
    {
        GameManager.GameStart -= SetupBoard;
        BoardShuffle.OnBoardShuffled -= FillBoardFromList;
    }

    #region SETUP
    internal void SetupBoard()
    {
        StartCoroutine(SetupBpardRoutine());
    }

    IEnumerator SetupBpardRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        SetupTiles();
        SetupGamePieces();
        SetupCamera();
        FillBoard(fillYOffset, fillFallTime);
    }

    void LoadBoardForLevel()
    {
        lvlBoard = Resources.Load($"{BOARD_LOCATION}{GameManager.Level}") as LevelBoardSO;

        if (lvlBoard == null)
        {
            Debug.LogWarning($"Level Board for lvl {GameManager.Level}, does not exist in Resources.");
            GameManager.GoBackOneLevel();
            LoadBoardForLevel();
        }
    }

    void SetupCamera()
    {
        float horizCenter = (lvlBoard.height - 1) / 2f;
        float vertiCenter = (lvlBoard.width - 1) / 2f;

        Camera.main.transform.position = new Vector3(vertiCenter, horizCenter, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)lvlBoard.height / 2f + (float)borderSize;
        float horizontalSize = ((float)lvlBoard.width / 2f + (float)borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    void SetupTiles()
    {
        for (int x = 0; x < lvlBoard.width; x++)
        {
            for (int y = 0; y < lvlBoard.height; y++)
            {
                CreateTile(TilePieceManager.Instance.GetProperPiece(lvlBoard.startingBoard[x, y]), x, y, 0);
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
                randomPiece.Move(x, y, fallTime, nameof(CreateGamePiece));
            }

            return randomPiece;
        }
        return null;
    }

    private GameObject CreateBomb(Tile pos, BombType type, MatchValue match = MatchValue.Wild)
    {
        if (IsWithinBounds(pos.xIndex, pos.yIndex))
        {
            return TilePieceManager.Instance.CreateBomb(pos, this, type, match);
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
        ForEachPiece((x, y) =>
        {
            // Fill the position with a collectible depending on the conditions
            if (y == lvlBoard.height - 1 && TilePieceManager.Instance.CanAddCollectible())
            {
                FillRandomAt(x, y, falseOffset, fallTime, isCollectible: true);
            }
            else
            {
                // Otherwise fill the position with a regular gamepiece
                FillRandomAt(x, y, falseOffset, fallTime);
                int iterations = 0;

                while (HasMatchOnFill(x, y) || iterations < 100)
                {
                    ClearPieceAt(x, y, wasChosen: false);
                    FillRandomAt(x, y, falseOffset, fallTime);

                    iterations++;
                }
            }
        });
    }

    void FillBoardFromList(List<GamePiece> gamePieces)
    {
        Queue<GamePiece> unusedPieces = new Queue<GamePiece>(gamePieces);

        ForEachPiece((x, y) =>
        {
            _allGamePieces[x, y] = unusedPieces.Dequeue();

            int iterations = 0;

            while (HasMatchOnFill(x, y) || iterations < 100)
            {
                unusedPieces.Enqueue(_allGamePieces[x, y]);
                _allGamePieces[x, y] = unusedPieces.Dequeue();
                iterations++;
            }
        });

        StartCoroutine(MovePieces());
    }

    void ForEachPiece(Action<int, int> boardMethod, bool spaceNotNull = false)
    {
        for (int x = 0; x < lvlBoard.width; x++)
        {
            for (int y = 0; y < lvlBoard.height; y++)
            {
                if (!IsSpaceAvailable(x, y, spaceNotNull)) continue;

                boardMethod(x, y);
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
                    bomb = CreateBomb(pos, BombType.Adjacent, targetPiece.matchValue);
                }
            }
            else
            {
                // Insert Color Bomb
                if (gamePieces.Count >= 5 && TilePieceManager.Instance.colorBombPrefab != null)
                {
                    bomb = CreateBomb(pos, BombType.Color);
                }
                else if (swapDirection.x != 0)
                {
                    // Insert row bomb
                    if (TilePieceManager.Instance.rowBombPrefab != null)
                    {
                        bomb = CreateBomb(pos, BombType.Row, targetPiece.matchValue);
                    }
                }
                else
                {
                    //Insert column bomb
                    if (TilePieceManager.Instance.columnBombPrefab != null)
                    {
                        bomb = CreateBomb(pos, BombType.Column, targetPiece.matchValue);
                    }
                }
            }
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
        return (x >= 0 && y >= 0 && x < lvlBoard.width && y < lvlBoard.height);
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
        if (!_playerInputEnabled) yield return null;

        GamePiece clickedPiece = _allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = _allGamePieces[targetTile.xIndex, targetTile.yIndex];

        if (targetPiece == null || clickedPiece == null) yield return null;

        //Debug.Log($"Moving {clickedTile.name} to {targetTile.name}");

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, nameof(SwitchTilesRoutine));
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, nameof(SwitchTilesRoutine));

        yield return new WaitForSeconds(swapTime);

        List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
        List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
        List<GamePiece> coloredBombMatches = CheckForColorBombs(clickedPiece, targetPiece);

        if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0 && coloredBombMatches.Count == 0)
        {
            clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime, nameof(SwitchTilesRoutine));
            targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime, nameof(SwitchTilesRoutine));

            yield return null;
        }

        yield return new WaitForSeconds(swapTime);

        Vector2 swipeDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);

        // Check if there is a bomb after the switch, for both tiles' matches
        _clickedTileBomb = InsertBomb(clickedTile, swipeDirection, clickedPieceMatches, targetPiece);
        _targetTileBomb = InsertBomb(targetTile, swipeDirection, targetPieceMatches, clickedPiece);

        ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).Union(coloredBombMatches).ToList());

        OnUserPlayed?.Invoke();
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

        int nextX, nextY, maxValue = lvlBoard.width > lvlBoard.height ? lvlBoard.width : lvlBoard.height;

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

        for (int i = 0; i < lvlBoard.width; i++)
        {
            for (int j = 0; j < lvlBoard.height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches.Union(matches).ToList();
            }
        }
        //Debug.Log($"Found {combinedMatches.Count} matches!");
        return combinedMatches;
    }

    List<GamePiece> FindAllMatchValue(MatchValue colorValue)
    {
        List<GamePiece> coloredPieces = new List<GamePiece>();

        for (int i = 0; i < lvlBoard.width; i++)
        {
            for (int j = 0; j < lvlBoard.height; j++)
            {
                if (_allGamePieces[i, j] != null && _allGamePieces[i, j].matchValue == colorValue)
                {
                    coloredPieces.Add(_allGamePieces[i, j]);
                }
            }
        }
        return coloredPieces;
    }

    List<GamePiece> FindCollectiblesAt(int row, bool clearedAtBottomOnly = false)
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < lvlBoard.width; i++)
        {
            Collectible collectible = _allGamePieces[i, row]?.GetComponent<Collectible>();
            if (_allGamePieces[i, row] != null && collectible != null)
            {
                if (!clearedAtBottomOnly || collectible.clearedAtBottom)
                    foundCollectibles.Add(_allGamePieces[i, row]);
            }
        }
        //Debug.Log($"Found {foundCollectibles.Count} collectibles at {row}");
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int y = 0; y < lvlBoard.height; y++)
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
        for (int i = 0; i < lvlBoard.width; i++)
        {
            for (int j = 0; j < lvlBoard.height; j++)
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
    void ClearPieceAt(int x, int y, bool wasChosen = true)
    {
        GamePiece pieceToClear = _allGamePieces[x, y];

        if (pieceToClear != null)
        {
            pieceToClear.Initialized(wasChosen);
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
                OnBonusCalculate?.Invoke(gamePieces.Count);

                ClearPieceAt(piece.xIndex, piece.yIndex);

                OnPieceCleared?.Invoke(piece.xIndex, piece.yIndex, 0, bombedPieces.Contains(piece));
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < lvlBoard.width; i++)
        {
            for (int j = 0; j < lvlBoard.height; j++)
            {
                ClearPieceAt(i, j, false);
            }
        }
    }

    void BreakTileAt(int x, int y)
    {
        Tile breakableTile = _allTiles[x, y];
        if (breakableTile != null)
        {
            OnTileBroke?.Invoke(breakableTile.breakableValue, x, y);
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

        for (int i = 0; i < lvlBoard.height - 1; i++)
        {
            if (IsSpaceAvailable(column, i))
            {
                for (int j = i + 1; j < lvlBoard.height; j++)
                {
                    if (_allGamePieces[column, j] != null)
                    {
                        _allGamePieces[column, j].Move(column, i, collapseTime * (j - i), nameof(CollapseColumn));
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

    List<GamePiece> CollapseColumn(List<int> collapsingColumns)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

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

        OnRefill?.Invoke(true);

        List<GamePiece> matches = gamePieces;
        OnBonusUpdate?.Invoke(false);

        do
        {
            OnBonusUpdate?.Invoke(true);
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));

            yield return StartCoroutine(RefillRoutine());

            yield return new WaitForSeconds(1);

            matches = FindAllMatches();
        }
        while (matches.Count > 0);

        // After refilling, we check for a deadlock by firing the event deadlock uses.
        // If true, we have a deadlock, so we handle it.
        if (OnFillFinished?.Invoke(_allGamePieces) ?? true)
        {
            OnRefill?.Invoke(false);
            _playerInputEnabled = GameManager.CanUserPlay();
        }
    }

    IEnumerator MovePieces()
    {
        ForEachPiece((x, y) => _allGamePieces[x, y].Move(x, y, swapTime, nameof(MovePieces)), spaceNotNull: true);

        yield return new WaitForSeconds(1f);

        //Debug.Log("Pieces Moved to their positions");
        yield return ClearAndRefillBoardRoutine(FindAllMatches());
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces, float delayBetweenMoves = 0.2f)
    {
        //HighlightPieces(gamePieces);
        yield return new WaitForSeconds(delayBetweenMoves);

        int tries = 5;

        do
        {
            // Check for bombs!
            var bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            // Doing it twice, to enable CHAIN BOMBS!!
            bombPieces = GetBombedPieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            // Track the collapsing columns before clearing, so that
            // we don't get a null reference
            List<int> collapsingColumns = GetColumns(gamePieces);

            ClearPieceAt(gamePieces, bombPieces);
            BreakTileAt(gamePieces);
            EnablePossibleBombs();

            yield return new WaitForSeconds(delayBetweenMoves);

            List<GamePiece> movingPieces = CollapseColumn(collapsingColumns);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenMoves);

            List<GamePiece> matches = FindMatchesAt(movingPieces);

            // Checking for COLLECTIBLES at the bottom row
            matches = matches.Union(FindCollectiblesAt(0, true)).ToList();
            //Debug.Log($"Found {matches.Count} new matches!");

            if (matches.Count == 0)
            {
                //Debug.Log("No other matches found!");
                break;
            }
            else
            {
                tries--;
                OnBonusUpdate?.Invoke(true);
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        } while (tries > 0);
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

        int limit = isRow ? lvlBoard.width : lvlBoard.height;

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
