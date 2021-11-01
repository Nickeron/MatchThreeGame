using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectXformMover : Mover
{
    public Vector3 startPosition, onScreenPosition, endPosition;
    public float moveTime = 1f;

    private RectTransform _rectXForm;
    private bool _isMoving = false;


    void Awake()
    {
        _rectXForm = GetComponent<RectTransform>();
    }

    void Move(Vector3 startPos, Vector3 endPos, float moveTime)
    {
        if (_isMoving) return;

        StartCoroutine(MoveRoutine(startPos, endPos, moveTime));

        _isMoving = true;
    }

    private IEnumerator MoveRoutine(Vector3 startPos, Vector3 endPos, float moveTime)
    {
        if (_rectXForm != null)
        {
            _rectXForm.position = startPos;
        }

        float elapsedTime = 0f;

        do
        {
            elapsedTime += Time.deltaTime;

            if (_rectXForm != null)
            {
                _rectXForm.anchoredPosition = Vector3.Lerp(startPos, endPos, InterpolateTime(elapsedTime, moveTime));
            }
            yield return null;

        } while (Vector3.Distance(_rectXForm.anchoredPosition, endPos) >= 0.01f);

        _isMoving = false;
    }

    public void MoveOn()
    {
        Move(startPosition, onScreenPosition, moveTime);
    }
    
    public void MoveOff()
    {
        Move(onScreenPosition, endPosition, moveTime);
    }
}
