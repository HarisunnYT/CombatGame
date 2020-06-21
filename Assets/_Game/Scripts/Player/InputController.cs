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

        AddBindings();

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

    public void AddBindings()
    {
        Left.AddDefaultBinding(Key.A);
        Left.AddDefaultBinding(InputControlType.DPadLeft);
        Left.AddDefaultBinding(InputControlType.LeftStickLeft);

        Right.AddDefaultBinding(Key.D);
        Right.AddDefaultBinding(InputControlType.DPadRight);
        Right.AddDefaultBinding(InputControlType.LeftStickRight);

        Up.AddDefaultBinding(Key.W);
        Up.AddDefaultBinding(InputControlType.DPadUp);
        Up.AddDefaultBinding(InputControlType.LeftStickUp);

        Down.AddDefaultBinding(Key.S);
        Down.AddDefaultBinding(InputControlType.DPadDown);
        Down.AddDefaultBinding(InputControlType.LeftStickDown);

        Jump.AddDefaultBinding(Key.Space);
        Jump.AddDefaultBinding(InputControlType.Action1);

        Attack1.AddDefaultBinding(Mouse.LeftButton);
        Attack1.AddDefaultBinding(InputControlType.Action3);

        Attack2.AddDefaultBinding(Mouse.MiddleButton);
        Attack2.AddDefaultBinding(InputControlType.Action4);

        Attack3.AddDefaultBinding(Mouse.RightButton);
        Attack3.AddDefaultBinding(InputControlType.Action2);
    }
}
