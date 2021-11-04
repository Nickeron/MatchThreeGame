using System;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image icnMessage;
    public Text txtMessage, txtButton;

    internal static Action ButtonPressed;

    private void OnEnable()
    {
        GameManager.OnDisplayMessage += ShowMessage;
    }
    
    private void OnDisable()
    {
        GameManager.OnDisplayMessage -= ShowMessage;
    }

    public void ShowMessage(Sprite sprite = null, string message = "", string btnMessage = "Start")
    {
        if(icnMessage != null)
        {
            icnMessage.sprite = sprite;
        }

        if(txtMessage != null)
        {
            txtMessage.text = message;
        }

        if(txtButton != null)
        {
            txtButton.text = btnMessage;
        }

        GetComponent<RectXformMover>().MoveOn();
    }

    public void OnUserClick()
    {
        if (ButtonPressed != null)
        {
            ButtonPressed();
        }        
    }
}
