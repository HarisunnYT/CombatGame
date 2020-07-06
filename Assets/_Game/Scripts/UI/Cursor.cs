using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    private float cursorMoveSpeed = 10;

    public int PlayerIndex { get; private set; }
    public System.Guid ControllerID { get; private set; }
    public Camera AssignedCamera { get; private set; }
    public InputProfile InputProfile { get; private set; }

    private Vector3 previousCursorPosition;

    private GraphicRaycaster assignedRaycaster;
    private Button previousHighlightedButton;

    private EventSystem eventSystem;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        ResetCamera();
    }

    public void AssignDevice(int playerIndex, System.Guid controllerID)
    {
        ControllerID = controllerID;
        PlayerIndex = playerIndex;

        InputProfile = new InputProfile(controllerID);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (PlayerIndex == 0) //0 means it's the person on the PC
        {
            transform.position = Input.mousePosition;
        }
        else
        {
            transform.position += new Vector3(InputProfile.Move.X * cursorMoveSpeed, InputProfile.Move.Y * cursorMoveSpeed, 0) * Time.deltaTime;
        }

        if (previousCursorPosition != transform.position || InputProfile.Select.WasPressed)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = transform.position;

            List<RaycastResult> results = new List<RaycastResult>();

            if (assignedRaycaster)
                assignedRaycaster.Raycast(pointerEventData, results);
            else
                PanelManager.Instance.Raycaster.Raycast(pointerEventData, results);

            eventSystem.SetSelectedGameObject(null);

            foreach (RaycastResult result in results)
            {
                Button button = result.gameObject.GetComponentInParent<Button>();
                if (button && button.interactable)
                {
                    button.Select();
                    previousHighlightedButton = button;
                    break;
                }
            }

            //clicked pressed from either mouse or controller
            if (InputProfile.Select.WasPressed)
            {
                CursorManager.Instance.SetLastInteractedPlayer(PlayerIndex);

                foreach (RaycastResult result in results)
                {
                    ISubmitHandler submitHandler = result.gameObject.GetComponentInParent<ISubmitHandler>();
                    submitHandler?.OnSubmit(null);

                    if (submitHandler != null) //we don't want to click more than one button at a time
                        break;
                }
            }
        }

        if (AssignedCamera == null)
            ResetCamera();

        //clamp cursor to camera bounds
        Vector3 pos = AssignedCamera.ScreenToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, 0.0f, 0.95f);
        pos.y = Mathf.Clamp(pos.y, 0.05f, 1.0f);
        transform.position = AssignedCamera.ViewportToScreenPoint(pos);

        previousCursorPosition = transform.position;
    }

    public void AssignRaycaster(GraphicRaycaster assignedRaycaster)
    {
        this.assignedRaycaster = assignedRaycaster;
    }

    public void ResetRaycaster()
    {
        assignedRaycaster = PanelManager.Instance.Raycaster;
    }

    public void AssignCamera(Camera camera)
    {
        AssignedCamera = camera;
    }

    public void ResetCamera()
    {
        AssignedCamera = Camera.main;
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
