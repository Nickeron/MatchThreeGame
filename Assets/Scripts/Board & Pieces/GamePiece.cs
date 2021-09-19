using System.Collections;

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GamePiece : MonoBehaviour
{
    public int xIndex, yIndex, scoreValue = 20;

    public InterpType interpolation = InterpType.SmootherStep;
    public MatchValue matchValue;

    private Board _board;
    private bool _isMoving = false, _initialized = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        }
    }

    public void Init(Board board)
    {
        _board = board;
    }

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY, float timeToMove)
    {
        if (_isMoving)
        {
            Debug.LogWarning("Cannot start move now. Already moving");
            return;
        }

        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        _isMoving = true;

        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            elapsedTime += Time.deltaTime;

            transform.position = Vector3.Lerp(startPosition, destination, InterpolateTime(elapsedTime, timeToMove));

            yield return null;
        }

        _board?.PlaceGamePiece(this, (int)destination.x, (int)destination.y);

        _isMoving = false;
    }

    private float InterpolateTime(float elapsedTime, float timeToMove)
    {
        float t = Mathf.Clamp01(elapsedTime / timeToMove);

        switch (interpolation)
        {
            case InterpType.Linear:
                return t;
            case InterpType.EaseOut:
                return Mathf.Sin(t * Mathf.PI * 0.5f);
            case InterpType.EaseIn:
                return 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
            case InterpType.SmoothStep:
                return t * t * (3 - 2 * t);
            case InterpType.SmootherStep:
                return t * t * t * (t * (t * 6 - 15) + 10);
        }
        return t;
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

    private void ScorePoints()
    {
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(scoreValue);
        }
    }

    public void Initialized(bool isInitialized)
    {
        _initialized = isInitialized;
    }

    private void OnDestroy()
    {
        if(_initialized)
            ScorePoints();
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
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Wild,
    None
}
