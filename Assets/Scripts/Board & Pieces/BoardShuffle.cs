using System.Collections.Generic;

using UnityEngine;

public class BoardShuffle : MonoBehaviour
{
    List<GamePiece> RemoveNormalPieces(GamePiece[,] allPieces)
    {
        List<GamePiece> normalPieces = new List<GamePiece>();

        ParseBoardPieces(allPieces, (x, y) =>
        {
            var gamePiece = allPieces[x, y];

            // Check that it is not a bomb or a collectible.
            if (gamePiece.GetComponent<Bomb>() == null && gamePiece.GetComponent<Collectible>() == null)
            {
                normalPieces.Add(gamePiece);
                allPieces[x, y] = null;
            }
        });
        return normalPieces;
    }

    void MovePieces(GamePiece[,] allPieces, float swapTime = 0.5f)
    {
        ParseBoardPieces(allPieces, (x, y) => allPieces[x, y].Move(x, y, swapTime));
    }

    void ShuffleList(List<GamePiece> shufflePieces)
    {
        for (int pos = 0; pos < shufflePieces.Count - 1; pos++)
        {
            int randPos = Random.Range(pos, shufflePieces.Count);

            if (randPos == pos) continue;

            SwapPositions(shufflePieces, randPos, pos);
        }
    }

    #region HELPER METHODS
    void ParseBoardPieces(GamePiece[,] allPieces, System.Action<int, int> MethodOnPiece)
    {
        for (int x = 0; x < allPieces.GetLength(0); x++)
        {
            for (int y = 0; y < allPieces.GetLength(1); y++)
            {
                if (allPieces[x, y] != null) MethodOnPiece(x, y);
            }
        }
    }

    void SwapPositions<T>(IList<T> swappableList, int firstPos, int secPos)
    {
        var temp = swappableList[firstPos];
        swappableList[firstPos] = swappableList[secPos];
        swappableList[secPos] = temp;
    }
    #endregion HELPER METHODS
}
