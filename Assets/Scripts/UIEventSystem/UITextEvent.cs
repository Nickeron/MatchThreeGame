using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu]
public class UITextEvent : ScriptableObject
{
    private readonly List<UITextEventListener> eventListeners = new List<UITextEventListener>();

    public void Raise(string updateText)
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised(updateText);
        }
    }

    public void RegisterListener(UITextEventListener listener)
    {
        if (eventListeners.Contains(listener)) return;

        eventListeners.Add(listener);
    }

    public void UnregisterListener(UITextEventListener listener)
    {
        if (!eventListeners.Contains(listener)) return;

        eventListeners.Remove(listener);
    }
}
