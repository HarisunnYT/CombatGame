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
    private Vector3 previousMousePosition;

    private bool forceUpdateNextFrame = false;

    private GraphicRaycaster assignedRaycaster;
    private Button previousHighlightedButton;

    private EventSystem eventSystem;

    private bool messageBoxFacingRight = true;

    private ScrollRect currentScrollable;
    private PointerEventData fakeEventData;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        fakeEventData = new PointerEventData(eventSystem) { button = PointerEventData.InputButton.Left };

        ResetCamera();
    }

    public void AssignDevice(int playerIndex, System.Guid controllerID, Color cursorColor = default)
    {
        ControllerID = controllerID;
        PlayerIndex = playerIndex;

        cursorIcon.color = cursorColor == default ? Color.white : cursorColor;

        InputProfile = new InputProfile(controllerID);
        InputProfile.OnInputChanged += OnInputChanged;

        gameObject.SetActive(true);
    }

    private void OnInputChanged(InControl.InputDevice previousDevice, InControl.InputDevice newDevice)
    {
        if (previousDevice.GUID == ControllerID)
            ControllerID = newDevice.GUID;
    }

    private void Update()
    {
        //0 means it's the person on the PC
        if (PlayerIndex == 0 && (previousMousePosition != Input.mousePosition || FightManager.Instance)) //this is so cursor isn't modified by keyboard when cursor active in fight
        {
            transform.position = Input.mousePosition;
            previousMousePosition = Input.mousePosition;
        }
        else
        {
            transform.position += new Vector3(InputProfile.Move.X * cursorMoveSpeed, InputProfile.Move.Y * cursorMoveSpeed, 0) * Time.unscaledDeltaTime;
        }

        if (previousCursorPosition != transform.position || InputProfile.Select.WasPressed || forceUpdateNextFrame)
        {
            if (currentScrollable == null)
                UpdateSelectedButton();
        }

        if (AssignedCamera == null)
            ResetCamera();

        if (messageBox.gameObject.activeSelf)
            SetMessageBoxSide();

        if (InputProfile.Select)
        {
            if (currentScrollable != null)
            {
                currentScrollable.OnDrag(fakeEventData);
                currentScrollable.OnScroll(fakeEventData);
            }
        }
        else if (InputProfile.Select.WasReleased)
        {
            if (currentScrollable != null)
                currentScrollable.OnEndDrag(fakeEventData);
            currentScrollable = null;
        }

        //clamp cursor to camera bounds
        Vector3 pos = AssignedCamera.ScreenToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, 0.0f, 0.95f);
        pos.y = Mathf.Clamp(pos.y, 0.05f, 1.0f);
        transform.position = AssignedCamera.ViewportToScreenPoint(pos);

        previousCursorPosition = transform.position;
        fakeEventData.position = previousCursorPosition;
    }

    private void UpdateSelectedButton()
    {
        forceUpdateNextFrame = false;

        List<RaycastResult> results = new List<RaycastResult>();

        if (assignedRaycaster)
            assignedRaycaster.Raycast(fakeEventData, results);
        else
            PanelManager.Instance.Raycaster.Raycast(fakeEventData, results);

        eventSystem.SetSelectedGameObject(null);
        HideMessage();

        foreach (RaycastResult result in results)
        {
            IInteractableMessage interactable = result.gameObject.GetComponentInParent<IInteractableMessage>();
            if (interactable != null)
            {
                if (interactable.Interactable && !string.IsNullOrEmpty(interactable.GetInteractableMessage()))
                    ShowMessage(interactable.GetInteractableMessage());
                else if (!interactable.Interactable && !string.IsNullOrEmpty(interactable.GetNonInteractableMessage()))
                    ShowMessage(interactable.GetNonInteractableMessage());
            }

            BetterButton button = result.gameObject.GetComponentInParent<BetterButton>();
            if (button)
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

                if (submitHandler == null) //if there isn't a submit handler, try and get a scroll rect
                {
                    currentScrollable = result.gameObject.GetComponentInParent<ScrollRect>();
                    if (currentScrollable != null)
                        currentScrollable.OnBeginDrag(fakeEventData);
                }

                HideMessage();

                if (submitHandler != null || currentScrollable != null) //we don't want to click more than one button at a time
                    break;
            }

            forceUpdateNextFrame = true;
        }
    }

    public void EnableAllControllerInput()
    {
        InputProfile = new InputProfile(default, true);
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
        bool isFullyVisible = messageBox.IsFullyVisibleHorizontal();
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
