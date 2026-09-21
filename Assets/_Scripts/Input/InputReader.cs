using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public Vector2 MoveValue { get; private set; }
    public Vector2 LookValue { get; private set; }
    public bool IsJumpButtonPressed { get; private set; }
    public bool IsReviveButtonPressed { get; private set; }
    public bool IsGrabButtonPressed { get; private set; }
    public bool IsLeftGrabButtonPressed { get; private set; }
    public bool IsRightGrabButtonPressed { get; private set; }
    public bool IsPauseButtonPressed { get; private set; }
    public bool IsLiftButtonPressed { get; private set; }

    public bool IsSelectItem1Pressed { get; private set; }
    public bool IsSelectItem2Pressed { get; private set; }
    public bool IsSelectItem3Pressed { get; private set; }
    public bool IsSelectItem4Pressed { get; private set; }
    public bool IsUseItemPressed { get; private set; }

    
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction reviveAction;
    private InputAction grabAction;
    private InputAction leftHandGrabAction;
    private InputAction rightHandGrabAction;
    private InputAction pauseAction;
    private InputAction liftAction;
    
    private InputAction selectItem1Action;
    private InputAction selectItem2Action;
    private InputAction selectItem3Action;
    private InputAction selectItem4Action;
    private InputAction useItemAction;
    
    
    public event System.Action OnPausePressed;

    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Get actions from the current action map
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        reviveAction = playerInput.actions["Revive"];
        grabAction = playerInput.actions["Grab"];
        leftHandGrabAction = playerInput.actions["LeftHandInteract"];
        rightHandGrabAction = playerInput.actions["RightHandInteract"];
        pauseAction = playerInput.actions["Pause"];
        liftAction = playerInput.actions["Lift"];
        
        selectItem1Action = playerInput.actions["SelectItem1"];
        selectItem2Action = playerInput.actions["SelectItem2"];
        selectItem3Action = playerInput.actions["SelectItem3"];
        selectItem4Action = playerInput.actions["SelectItem4"];
        useItemAction = playerInput.actions["UseItem"];

        // Subscribe to input events
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;

        jumpAction.performed += OnJump;
        jumpAction.canceled += OnJump;
        
        reviveAction.performed += OnRevive;
        reviveAction.canceled += OnRevive;
        
        grabAction.performed += OnGrab;
        grabAction.canceled += OnGrab;
        
        leftHandGrabAction.performed += OnLeftHandInteract;
        leftHandGrabAction.canceled += OnLeftHandInteract;
        
        rightHandGrabAction.performed += OnRightHandInteract;
        rightHandGrabAction.canceled += OnRightHandInteract;

        pauseAction.performed += OnPause;
        pauseAction.canceled += OnPause;
        
        liftAction.performed += OnLift;
        liftAction.canceled += OnLift;
        
        selectItem1Action.performed += OnSelectItem1;
        selectItem1Action.canceled += OnSelectItem1;
        
        selectItem2Action.performed += OnSelectItem2;
        selectItem2Action.canceled += OnSelectItem2;
        
        selectItem3Action.performed += OnSelectItem3;
        selectItem3Action.canceled += OnSelectItem3;
        
        selectItem4Action.performed += OnSelectItem4;
        selectItem4Action.canceled += OnSelectItem4;
        
        useItemAction.performed += OnUseItem;
        useItemAction.canceled += OnUseItem;
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;

        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJump;
        
        reviveAction.performed -= OnRevive;
        reviveAction.canceled -= OnRevive;
        
        grabAction.performed -= OnGrab;
        grabAction.canceled -= OnGrab;
        
        leftHandGrabAction.performed -= OnLeftHandInteract;
        leftHandGrabAction.canceled -= OnLeftHandInteract;
        
        rightHandGrabAction.performed -= OnRightHandInteract;
        rightHandGrabAction.canceled -= OnRightHandInteract;
        
        pauseAction.performed -= OnPause;
        pauseAction.canceled -= OnPause;
        
        liftAction.performed -= OnLift;
        liftAction.canceled -= OnLift;
        
        selectItem1Action.performed -= OnSelectItem1;
        selectItem1Action.canceled -= OnSelectItem1;
        
        selectItem2Action.performed -= OnSelectItem2;
        selectItem2Action.canceled -= OnSelectItem2;
        
        selectItem3Action.performed -= OnSelectItem3;
        selectItem3Action.canceled -= OnSelectItem3;
        
        selectItem4Action.performed -= OnSelectItem4;
        selectItem4Action.canceled -= OnSelectItem4;
        
        useItemAction.performed -= OnUseItem;
        useItemAction.canceled -= OnUseItem;

        playerInput.actions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveValue = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        LookValue = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsJumpButtonPressed = true;
            //testing score popup using jump ScorePopupManager.Instance.ShowScore(2);
        }
        else if (context.canceled)
            IsJumpButtonPressed = false;
    }
    
    private void OnRevive(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsReviveButtonPressed = true;
        else if (context.canceled)
            IsReviveButtonPressed = false;
    }
    
    private void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsGrabButtonPressed = true;
        else if (context.canceled)
            IsGrabButtonPressed = false;
    }
    
    private void OnLeftHandInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsLeftGrabButtonPressed = true;
        else if (context.canceled)
            IsLeftGrabButtonPressed = false;
    }
    
    private void OnRightHandInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsRightGrabButtonPressed = true;
        else if (context.canceled)
            IsRightGrabButtonPressed = false;
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsPauseButtonPressed = true;
            OnPausePressed?.Invoke();
        }
        else if (context.canceled)
        {
            IsPauseButtonPressed = false;
        }
    }
    
    private void OnLift(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsLiftButtonPressed = true;
        else if (context.canceled)
            IsLiftButtonPressed = false;
    }
    
    private void OnSelectItem1(InputAction.CallbackContext context)
    {
        if (context.performed) IsSelectItem1Pressed = true;
        else if (context.canceled) IsSelectItem1Pressed = false;
    }

    private void OnSelectItem2(InputAction.CallbackContext context)
    {
        if (context.performed) IsSelectItem2Pressed = true;
        else if (context.canceled) IsSelectItem2Pressed = false;
    }
    
    private void OnSelectItem3(InputAction.CallbackContext context)
    {
        if (context.performed) IsSelectItem3Pressed = true;
        else if (context.canceled) IsSelectItem3Pressed = false;
    }
    
    private void OnSelectItem4(InputAction.CallbackContext context)
    {
        if (context.performed) IsSelectItem4Pressed = true;
        else if (context.canceled) IsSelectItem4Pressed = false;
    }
    
    private void OnUseItem(InputAction.CallbackContext context)
    {
        if (context.performed) IsUseItemPressed = true;
        else if (context.canceled) IsUseItemPressed = false;
    }
}