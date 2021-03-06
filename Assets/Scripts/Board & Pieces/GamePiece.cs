using System;
using System.Collections;

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GamePiece : Mover
{
    public static event Action<Vector3, int> PieceCleared;
    public int xIndex, yIndex, scoreValue = 20;

    public MatchValue matchValue;

    private Board _board;
    private bool _isMoving = false, _initialized = false;

    public void Init(Board board)
    {
        _board = board;
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    // A string to use for debugging purposes
    string previousCaller;

    public void Move(int destX, int destY, float timeToMove, string methodCaller)
    {
        if (_isMoving)
        {
            Debug.LogWarning($"Irregular call to move from: {methodCaller}. Already moving from: {previousCaller}");
            return;
        }

        _isMoving = true;
        previousCaller = methodCaller;
        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        //Debug.Log($"Moving {matchValue} from {transform.position.x},{transform.position.y} to {destination.x},{destination.y}");

        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(startPosition, destination, InterpolateTime(elapsedTime, timeToMove));

            yield return null;
        }

        _board?.PlaceGamePiece(this, (int)destination.x, (int)destination.y);

        _isMoving = false;
    }

    public void SetColor(GamePiece matchObject)
    {
        if (matchObject != null) SetColor(matchObject.matchValue);
    }

    public void SetColor(MatchValue newValue)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = TilePieceManager.Instance.GetColor(newValue);
        }

        matchValue = newValue;
    }

    public void Initialized(bool isInitialized)
    {
        _initialized = isInitialized;
    }

    private void OnDestroy()
    {
        if (!_initialized) return;

        PieceCleared?.Invoke(transform.position, scoreValue);
    }
}

public enum InterpType
{
    Linear,
    EaseOut,
    EaseIn,
    SmoothStep,
    SmootherStep
}

public enum MatchValue
{
    Yellow,
    Blue,
    Green,
    Teal,
    Red,
    Purple,
    Orange,
    Wild,
    None
}
