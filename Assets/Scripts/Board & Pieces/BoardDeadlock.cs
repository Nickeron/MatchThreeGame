using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BoardDeadlock : MonoBehaviour
{
    public static Action<GamePiece[,]> OnDeadlock;

    private void OnEnable()
    {
        Board.OnFillFinished += AreThereMoves;
    }

    private void OnDisable()
    {
        Board.OnFillFinished -= AreThereMoves;
    }

    public bool AreThereMoves(GamePiece[,] allPieces)
    {
        for (int x = 0; x < allPieces.GetLength(0); x++)
        {
            for (int y = 0; y < allPieces.GetLength(1); y++)
            {
                if (HasMoveAt(allPieces, x, y, 3)) return true;
            }
        }

        Debug.LogWarning("--DEADLOCK--");
        OnDeadlock?.Invoke(allPieces);        
        return false;
    }

    // Given an (x,y) coordinate return a List of GamePieces (either a row or column) 
    List<GamePiece> GetRowColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> piecesList = new List<GamePiece>();

        for (int i = 0; i < listLength; i++)
        {
            int nextX = checkRow ? x + i : x ;
            int nextY = checkRow ? y : y + i;
            
            if (nextX < allPieces.GetLength(0) && 
                nextY < allPieces.GetLength(1) && 
                allPieces[nextX, nextY] != null) 
            { 
                piecesList.Add(allPieces[nextX, nextY]); 
            }
        }
        return piecesList;
    }

    List<GamePiece> GetMinMatches(List<GamePiece> gamePieces, int minMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (var group in gamePieces.GroupBy(n => n.matchValue))
        {
            if (group.Count() >= minMatch && group.Key != MatchValue.None)
            {
                matches = group.ToList();
            }
        }
        return matches;
    }

    IEnumerable<GamePiece> GetNeighbors(GamePiece[,] allPieces, int x, int y)
    {
        HashSet<GamePiece> neighbors = new HashSet<GamePiece>();

        foreach (Vector2 dir in GetSearchDirections())
        {
            int newX = x + (int)dir.x;
            int newY = y + (int)dir.y;

            if (!isWithinBounds(newX, newY, allPieces.GetLength(0), allPieces.GetLength(1))) continue;

            if (allPieces[newX, newY] == null) continue;

            neighbors.Add(allPieces[newX, newY]);
        }

        return neighbors;
    }

    List<GamePiece> GetAvailableMoves(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> pieces = GetRowColumnList(allPieces, x, y, listLength, checkRow);
        List<GamePiece> matches = GetMinMatches(pieces, listLength - 1);

        GamePiece unmatchedPiece = null;

        if (pieces == null || matches == null) return null;

        if (pieces.Count != listLength || matches.Count < listLength - 1) return null;

        // if we have an unmatched GamePiece, check its neighboring GamePieces
        unmatchedPiece = pieces.Except(matches).FirstOrDefault();      

        if (unmatchedPiece == null) return null;

        //Debug.Log($"Move {matches[0].matchValue} piece to {unmatchedPiece.xIndex}, {unmatchedPiece.yIndex}");

        return matches
            .Union(GetNeighbors(allPieces, unmatchedPiece.xIndex, unmatchedPiece.yIndex)
            .Except(matches)
            .Where(n => n.matchValue == matches[0].matchValue))
            .ToList();        
    }

    Vector2[] GetSearchDirections()
    {
        return new Vector2[4]
        {
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f),
        };
    }

    bool HasMoveAt(GamePiece[,] allPieces, int x, int y, int listLength = 3)
    {
        var horizontMoves = GetAvailableMoves(allPieces, x, y, listLength, true);
        var verticalMoves = GetAvailableMoves(allPieces, x, y, listLength, false);

        //Debug.Log($"Horizontal moves {horizontMoves?.Count ?? 0}, Vertical Moves {verticalMoves?.Count ?? 0}");

        return horizontMoves != null ? horizontMoves.Count >= listLength :
            verticalMoves != null && verticalMoves.Count >= listLength;
    }

    bool isWithinBounds(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}
