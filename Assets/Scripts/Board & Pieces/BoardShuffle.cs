using System.Collections.Generic;

using UnityEngine;

public class BoardShuffle : MonoBehaviour
{
    public static event System.Action<List<GamePiece>> OnBoardShuffled;

    private void OnEnable()
    {
        BoardDeadlock.OnDeadlock += ShuffleBoard;
    }

    private void OnDisable()
    {
        BoardDeadlock.OnDeadlock -= ShuffleBoard;
    }

    void ShuffleBoard(GamePiece[,] allPieces)
    {
        OnBoardShuffled?.Invoke(ShuffleList(RemoveNormalPieces(allPieces)));
    }

    List<GamePiece> RemoveNormalPieces(GamePiece[,] allPieces)
    {
        List<GamePiece> normalPieces = new List<GamePiece>();

        ForEachPiece(allPieces, (x, y) =>
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

    List<GamePiece> ShuffleList(List<GamePiece> shufflePieces)
    {
        for (int pos = 0; pos < shufflePieces.Count - 1; pos++)
        {
            int randPos = Random.Range(pos, shufflePieces.Count);

            if (randPos == pos) continue;

            SwapPositions(shufflePieces, randPos, pos);
        }
        return shufflePieces;
    }

    #region HELPER METHODS
    void ForEachPiece(GamePiece[,] allPieces, System.Action<int, int> MethodOnPiece)
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
