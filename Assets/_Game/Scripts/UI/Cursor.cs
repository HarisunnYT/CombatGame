using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    private float cursorMoveSpeed = 10;

    [SerializeField]
    private Image cursorIcon;

    [SerializeField]
    private RectTransform messageBox;

    [SerializeField]
    private TMP_Text messageText;

    public int PlayerIndex { get; private set; }
    public System.Guid ControllerID { get; private set; }
    public Camera AssignedCamera { get; private set; }
    public InputProfile InputProfile { get; private set; }

    private Vector3 previousCursorPosition;

    private GraphicRaycaster assignedRaycaster;
    private Button previousHighlightedButton;

    private EventSystem eventSystem;

    private bool messageBoxFacingRight = true;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        ResetCamera();
    }

    public void AssignDevice(int playerIndex, System.Guid controllerID, Color cursorColor = default)
    {
        ControllerID = controllerID;
        PlayerIndex = playerIndex;

        cursorIcon.color = cursorColor == default ? Color.white : cursorColor;

        InputProfile = new InputProfile(controllerID);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        Debug.Log(Screen.width / Screen.height);

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
            HideMessage();

            foreach (RaycastResult result in results)
            {
                BetterButton button = result.gameObject.GetComponentInParent<BetterButton>();
                if (button)
                {
                    if (button.interactable)
                    {
                        button.Select();
                        previousHighlightedButton = button;
                        break;
                    }
                    else
                    {
                        ShowMessage(button.NonInteractableMessage);
                    }
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

        if (messageBox.gameObject.activeSelf)
            SetMessageBoxSide();

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

    private void SetMessageBoxSide()
    {
        bool isFullyVisible = messageBox.IsFullyVisibleFrom();
        if (isFullyVisible)
            return;

        bool right = !messageBoxFacingRight;
        messageBoxFacingRight = right;

        messageBox.pivot = right ? Vector2.zero : new Vector2(1, 0);
        messageBox.anchoredPosition = right ? new Vector2(0, messageBox.anchoredPosition.y) : new Vector2(-100, messageBox.anchoredPosition.y);
    }

    private void ShowMessage(string text)
    {
        messageText.text = text;
        messageBox.gameObject.SetActive(true);
    }

    private void HideMessage()
    {
        messageBox.gameObject.SetActive(false);
    }
}
