using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BoardDeadlock : MonoBehaviour
{
    public bool IsDeadLocked(GamePiece[,] allPieces, int listLength = 3)
    {
        for (int i = 0; i < allPieces.GetLength(0); i++)
        {
            for (int j = 0; j < allPieces.GetLength(1); j++)
            {
                if (HasMoveAt(allPieces, i, j, listLength)) return false;
            }
        }

        return true;
    }

    List<GamePiece> GetRowColumnList(GamePiece[,] allPieces, int x, int y, int listLength = 3, bool checkRow = true)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        List<GamePiece> piecesList = new List<GamePiece>();

        for (int i = 0; i < listLength; i++)
        {
            if (checkRow)
            {
                if (x + i < width && y < height) { piecesList.Add(allPieces[x + i, y]); }
            }
            else
            {
                if (x < width && y + i < height) { piecesList.Add(allPieces[x, y + i]); }
            }
        }
        return piecesList;
    }
    List<GamePiece> GetMinMatches(List<GamePiece> gamePieces, int minMatch = 2)
    {
        List<GamePiece> matches = new List<GamePiece>();

        var groups = gamePieces.GroupBy(n => n.matchValue);

        foreach (var group in groups)
        {
            if (group.Count() > minMatch && group.Key != MatchValue.None)
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

        if (pieces.Count != listLength || matches.Count != listLength - 1) return null;

        unmatchedPiece = pieces.Except(matches).FirstOrDefault();

        if (unmatchedPiece == null) return null;

        return matches
            .Union(GetNeighbors(allPieces, unmatchedPiece.xIndex, unmatchedPiece.yIndex)
            .Except(matches)
            .Where(n => n.matchValue == matches[0].matchValue))
            .ToList();

        //Debug.Log($"Move {matches[0].matchValue} piece to {unmatchedPiece.xIndex}, {unmatchedPiece.yIndex}");
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
        var matches = GetAvailableMoves(allPieces, x, y, listLength, true)
            .Union(GetAvailableMoves(allPieces, x, y, listLength, false))
            .ToList();

        if (matches == null) return false;

        return matches.Count >= listLength;
    }

    bool isWithinBounds(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
}
