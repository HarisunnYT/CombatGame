using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    private float cursorMoveSpeed = 10;

    private InputProfile inputProfile;

    private EventSystem eventSystem;

    private int playerIndex;

    public System.Guid ControllerID { get; private set; }

    private Vector3 previousCursorPosition;

    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    public void AssignDevice(int playerIndex, System.Guid controllerID)
    {
        ControllerID = controllerID;
        this.playerIndex = playerIndex;

        inputProfile = new InputProfile(controllerID);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (previousCursorPosition != transform.position || inputProfile.Select.WasPressed)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = transform.position;

            List<RaycastResult> results = new List<RaycastResult>();

            PanelManager.Instance.Raycaster.Raycast(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                Button button = result.gameObject.GetComponentInParent<Button>();
                button?.Select();
            }

            //clicked pressed from either mouse or controller
            if (inputProfile.Select.WasPressed)
            {
                CursorManager.Instance.SetLastInteractedPlayer(playerIndex);

                foreach (RaycastResult result in results)
                {
                    Button button = result.gameObject.GetComponentInParent<Button>();
                    button?.onClick?.Invoke();
                }
            }
        }

        previousCursorPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (playerIndex == 0) //0 means it's the person on the PC
        {
            transform.position = Input.mousePosition;
        }
        else
        {
            transform.position += new Vector3(inputProfile.Move.X * cursorMoveSpeed, inputProfile.Move.Y * cursorMoveSpeed, 0);
        }
    }
}
