using UnityEngine;

using UIText = TMPro.TextMeshProUGUI;

[RequireComponent(typeof(UIText))]
public class UITextEventListener : MonoBehaviour
{
    [Tooltip("Event to register with")]
    public UITextEvent Event;
    

    void OnEnable()
    {
        Event.RegisterListener(this);
    }

    void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(string updateText)
    {
        GetComponent<UIText>().text = updateText;
    }
}
