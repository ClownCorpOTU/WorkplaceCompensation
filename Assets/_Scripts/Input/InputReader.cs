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
            IsJumpButtonPressed = true;
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
            IsPauseButtonPressed = true;
        else if (context.canceled)
            IsPauseButtonPressed = false;
    }
    
    private void OnLift(InputAction.CallbackContext context)
    {
        if (context.performed)
            IsLiftButtonPressed = true;
        else if (context.canceled)
            IsLiftButtonPressed = false;
    }
}