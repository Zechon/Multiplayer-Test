using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Player_Inputs inputs;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool PausePressed { get; private set; }

    private void Awake()
    {
        inputs = new Player_Inputs();
    }

    private void OnEnable()
    {
        inputs.Enable();
        RegisterCallbacks();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
        inputs.Disable();
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        PausePressed = false;
    }

    private void RegisterCallbacks()
    {
        inputs.Player.Move.performed += OnMove;
        inputs.Player.Move.canceled += OnMove;

        inputs.Player.Jump.performed += OnJumpPerformed;
        inputs.Player.Jump.canceled += OnJumpCanceled;

        inputs.Player.Crouch.performed += OnCrouchPerformed;
        inputs.Player.Crouch.canceled += OnCrouchCanceled;

        inputs.Player.Sprint.performed += OnSprintPerformed;
        inputs.Player.Sprint.canceled += OnSprintCanceled;

        inputs.Player.Pause.performed += OnPausePerformed;
        inputs.Player.Pause.canceled += OnPauseCanceled;
    }

    private void UnregisterCallbacks()
    {
        inputs.Player.Move.performed -= OnMove;
        inputs.Player.Move.canceled -= OnMove;

        inputs.Player.Jump.performed -= OnJumpPerformed;
        inputs.Player.Jump.canceled -= OnJumpCanceled;

        inputs.Player.Crouch.performed -= OnCrouchPerformed;
        inputs.Player.Crouch.canceled -= OnCrouchCanceled;

        inputs.Player.Sprint.performed -= OnSprintPerformed;
        inputs.Player.Sprint.canceled -= OnSprintCanceled;

        inputs.Player.Pause.performed -= OnPausePerformed;
        inputs.Player.Pause.canceled -= OnPauseCanceled;
    }

    #region On XYZ
    private void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        JumpPressed = true;
        JumpHeld = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        JumpHeld = false;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        CrouchHeld = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        CrouchHeld = false;
    }
    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        SprintHeld = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        SprintHeld = false;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        PausePressed = true;
    }

    private void OnPauseCanceled(InputAction.CallbackContext ctx)
    {
        PausePressed = false;
    }
    #endregion
}
