using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BoardDeadlock : MonoBehaviour
{
    private void OnEnable()
    {
        Board.OnRefillFinished += IsDeadLocked;
    }

    private void OnDisable()
    {
        Board.OnRefillFinished -= IsDeadLocked;
    }

    public bool IsDeadLocked(GamePiece[,] allPieces, int listLength = 3)
    {
        for (int i = 0; i < allPieces.GetLength(0); i++)
        {
            for (int j = 0; j < allPieces.GetLength(1); j++)
            {
                if (HasMoveAt(allPieces, i, j, listLength)) return false;
            }
        }
        Debug.LogWarning("--DEADLOCK--");
        return true;
    }

    // Given an (x,y) coordinate return a List of GamePieces (either a row or column) 
    List<GamePiece> GetRowColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        List<GamePiece> piecesList = new List<GamePiece>();

        for (int i = 0; i < listLength; i++)
        {
            int nextX = checkRow ? x + i : x ;
            int nextY = checkRow ? y : y + i;
            
            if (nextX < allPieces.GetLength(0) && nextY < allPieces.GetLength(1)) 
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

        Debug.Log($"Move {matches[0].matchValue} piece to {unmatchedPiece.xIndex}, {unmatchedPiece.yIndex}");

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

        Debug.Log($"Horizontal moves {horizontMoves?.Count ?? 0}, Vertical Moves {verticalMoves?.Count ?? 0}");
        
        return horizontMoves != null ? horizontMoves.Count >= listLength :
            verticalMoves != null ? verticalMoves.Count >= listLength : false;
    }

    bool isWithinBounds(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}
