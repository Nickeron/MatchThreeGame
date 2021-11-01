using System;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image icnMessage;
    public Text txtMessage, txtButton;


    internal Action ButtonPressed;

    private RectXformMover mover;

    private void Awake()
    {
        mover = GetComponent<RectXformMover>();
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

        mover.MoveOn();
    }

    public void OnUserClick()
    {
        if (ButtonPressed != null)
        {
            ButtonPressed();
        }        
    }
}
