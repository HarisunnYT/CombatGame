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

    public PlayerAction Select;
    public PlayerAction Menu;
    public PlayerAction Back;
    public PlayerAction Chat;

    public PlayerAction CommunicationWheelOpen;
    public PlayerAction CommunicationWheelUp;
    public PlayerAction CommunicationWheelDown;
    public PlayerAction CommunicationWheelRight;
    public PlayerAction CommunicationWheelLeft;

    public System.Guid GUID;
    public UnityInputDevice InputDevice;

    public static List<InputProfile> InputProfiles = new List<InputProfile>();

    public delegate void InputChangedEvent(InputDevice previousDevice, InputDevice newDevice);
    public event InputChangedEvent OnInputChanged;

    public InputProfile(System.Guid controllerGUID, bool allowAllControllers = false)
    {
        Left = CreatePlayerAction("Move Left");
        Right = CreatePlayerAction("Move Right");
        Up = CreatePlayerAction("Up");
        Down = CreatePlayerAction("Down");

        Jump = CreatePlayerAction("Jump");

        Attack1 = CreatePlayerAction("Attack1");
        Attack2 = CreatePlayerAction("Attack2");
        Attack3 = CreatePlayerAction("Attack3");

        Menu = CreatePlayerAction("Menu");
        Select = CreatePlayerAction("Select");
        Back = CreatePlayerAction("Back");
        Chat = CreatePlayerAction("Chat");

        CommunicationWheelOpen = CreatePlayerAction("CommunicationWheelOpen");
        CommunicationWheelUp = CreatePlayerAction("CommunicationWheelUp");
        CommunicationWheelDown = CreatePlayerAction("CommunicationWheelDown");
        CommunicationWheelRight = CreatePlayerAction("CommunicationWheelRight");
        CommunicationWheelLeft = CreatePlayerAction("CommunicationWheelLeft");

        Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);

        if (controllerGUID == default)
            AddKeyboardBindings();
        
        if (controllerGUID != default || allowAllControllers)
            AddControllerBindings();

        //assign device
        for (int i = 0; i < InControl.InputManager.Devices.Count; i++)
        {
            if (allowAllControllers || InControl.InputManager.Devices[i].GUID == controllerGUID)
            {
                if (InControl.InputManager.Devices[i] is UnityInputDevice)
                {
                    InputDevice = InControl.InputManager.Devices[i] as UnityInputDevice;

                    if (!IncludeDevices.Contains(InputDevice))
                        IncludeDevices.Add(InputDevice);

                    if (!allowAllControllers)
                        break;
                }
            }
        }

        GUID = controllerGUID;
        InputProfiles.Add(this);

        InputManager.OnDeviceAttached += OnDeviceAttached;
    }

    private void OnDeviceAttached(InputDevice obj)
    {
        if (InputDevice != null && InputDevice.JoystickId == ((UnityInputDevice)obj).JoystickId)
        {
            OnInputChanged?.Invoke(InputDevice, obj);
            IncludeDevices.Remove(InputDevice);

            InputProfiles.Remove(this);

            InputDevice = obj as UnityInputDevice;
            GUID = obj.GUID;

            IncludeDevices.Add(InputDevice);
            InputProfiles.Add(this);
        }
    }

    public void RemoveController(System.Guid controllerGUID)
    {
        for (int i = 0; i < InputManager.Devices.Count; i++)
        {
            if (InputManager.Devices[i].GUID == controllerGUID)
            {
                IncludeDevices.Remove(InputManager.Devices[i]);

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

        Menu.AddDefaultBinding(Key.Escape);
        Select.AddDefaultBinding(Mouse.LeftButton);
        Back.AddDefaultBinding(Key.Escape);
        Chat.AddDefaultBinding(Key.T);

        //for some reason it doesn't work if you add multiple keys in the same binding, weird
        CommunicationWheelOpen.AddDefaultBinding(Key.Key1);
        CommunicationWheelOpen.AddDefaultBinding(Key.Key2);
        CommunicationWheelOpen.AddDefaultBinding(Key.Key3);
        CommunicationWheelOpen.AddDefaultBinding(Key.Key4);

        CommunicationWheelUp.AddDefaultBinding(Key.Key1);
        CommunicationWheelRight.AddDefaultBinding(Key.Key2);
        CommunicationWheelDown.AddDefaultBinding(Key.Key3);
        CommunicationWheelLeft.AddDefaultBinding(Key.Key4);
    }

    private void AddControllerBindings()
    {
        Left.AddDefaultBinding(InputControlType.LeftStickLeft);
        Right.AddDefaultBinding(InputControlType.LeftStickRight);
        Up.AddDefaultBinding(InputControlType.LeftStickUp);
        Down.AddDefaultBinding(InputControlType.LeftStickDown);

        Jump.AddDefaultBinding(InputControlType.Action1);

        Attack1.AddDefaultBinding(InputControlType.Action3);
        Attack2.AddDefaultBinding(InputControlType.Action4);
        Attack3.AddDefaultBinding(InputControlType.Action2);

        Select.AddDefaultBinding(InputControlType.Action1);
        Back.AddDefaultBinding(InputControlType.Action2);
        Menu.AddDefaultBinding(InputControlType.Command);
        Chat.AddDefaultBinding(InputControlType.DPadDown);

        CommunicationWheelOpen.AddDefaultBinding(InputControlType.DPadUp);
        CommunicationWheelUp.AddDefaultBinding(InputControlType.DPadUp);
        CommunicationWheelRight.AddDefaultBinding(InputControlType.DPadRight);
        CommunicationWheelDown.AddDefaultBinding(InputControlType.DPadDown);
        CommunicationWheelLeft.AddDefaultBinding(InputControlType.DPadLeft);
    }

    public void Deinitialise()
    {
        InputProfiles.Remove(this);
    }
}
