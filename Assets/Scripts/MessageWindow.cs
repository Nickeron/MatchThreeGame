using Sirenix.OdinInspector;

using System;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image icnMessage;
    public Text txtMessage, txtButton;

    [HorizontalGroup("Message Window Icons", LabelWidth = 50)]
    public Sprite icnLose, icnWin, icnGoal;

    internal static Action ButtonPressed;

    private void OnEnable()
    {
        GameManager.OnDisplayMessage += SwitchMessage;
    }
    
    private void OnDisable()
    {
        GameManager.OnDisplayMessage -= SwitchMessage;
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

    public void SwitchMessage(MessageType type)
    {
        switch (type)
        {
            case MessageType.Win:
                ShowMessage(icnWin, $"You WIN!\n{1000}", "Next");
                return;
            case MessageType.Lose:
                ShowMessage(icnLose, $"You lost..\n{1000}", "Replay");
                return;
            case MessageType.Goal:
            default:
                ShowMessage(icnGoal, $"Score Goal\n{1000}", "Start");
                return;
        }
    }
}

public enum MessageType
{
    Win,
    Lose,
    Goal
}
