using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class InputProfile : PlayerActionSet
{
    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Up;
    public PlayerAction Down;

    public PlayerTwoAxisAction Move;

    public PlayerAction Jump;
    public PlayerAction Attack1;
    public PlayerAction Attack2;
    public PlayerAction Attack3;

    public InputDevice device = null;

    private int playerIndex;
    private int slot;

    public InputProfile(int playerIndex, System.Guid controllerGUID)
    {
        Left = CreatePlayerAction("Move Left");
        Right = CreatePlayerAction("Move Right");
        Up = CreatePlayerAction("Up");
        Down = CreatePlayerAction("Down");

        Jump = CreatePlayerAction("Jump");
        Attack1 = CreatePlayerAction("Attack1");
        Attack2 = CreatePlayerAction("Attack2");
        Attack3 = CreatePlayerAction("Attack3");

        Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);

        this.playerIndex = playerIndex;

        if (controllerGUID == default)
            AddKeyboardBindings();
        else
            AddControllerBindings();

        //assign device
        for (int i = 0; i < InControl.InputManager.Devices.Count; i++)
        {
            if (InControl.InputManager.Devices[i].GUID == controllerGUID)
            {
                slot = i;

                device = InControl.InputManager.Devices[i];
                IncludeDevices.Add(device);

                break;
            }
        }
    }

    private void AddKeyboardBindings()
    {
        Left.AddDefaultBinding(Key.A);
        Right.AddDefaultBinding(Key.D);
        Up.AddDefaultBinding(Key.W);
        Down.AddDefaultBinding(Key.S);

        Jump.AddDefaultBinding(Key.Space);

        Attack1.AddDefaultBinding(Mouse.LeftButton);
        Attack2.AddDefaultBinding(Mouse.MiddleButton);
        Attack3.AddDefaultBinding(Mouse.RightButton);

    }

    private void AddControllerBindings()
    {
        Left.AddDefaultBinding(InputControlType.DPadLeft);
        Left.AddDefaultBinding(InputControlType.LeftStickLeft);

        Right.AddDefaultBinding(InputControlType.DPadRight);
        Right.AddDefaultBinding(InputControlType.LeftStickRight);

        Up.AddDefaultBinding(InputControlType.DPadUp);
        Up.AddDefaultBinding(InputControlType.LeftStickUp);

        Down.AddDefaultBinding(InputControlType.DPadDown);
        Down.AddDefaultBinding(InputControlType.LeftStickDown);

        Jump.AddDefaultBinding(InputControlType.Action1);

        Attack1.AddDefaultBinding(InputControlType.Action3);
        Attack2.AddDefaultBinding(InputControlType.Action4);
        Attack3.AddDefaultBinding(InputControlType.Action2);
    }
}
