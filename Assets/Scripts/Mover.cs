using UnityEngine;

public class Mover : MonoBehaviour
{
    public InterpType interpolation = InterpType.SmootherStep;

    protected float InterpolateTime(float elapsedTime, float timeToMove)
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
}
