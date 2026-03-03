using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    #region Variables
    public enum MovementState
    {
        Default,
        Sprinting,
        Crouching,
        Airborne
    }

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpForce = 1.6f;
    [SerializeField] float sprintJumpMultiplier = 1.1f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float crouchingHeight = 1.1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] float maxSlopeAngle = 50f;
    [SerializeField] LayerMask groundMask;

    [Header("Ceiling Check")]
    [SerializeField] float ceilingCheckRadius = 0.25f;
    [SerializeField] LayerMask ceilingMask;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PauseMenuHandler pauseHandler;

    private Vector3 velocity;

    public MovementState state { get; private set; }
    public bool IsPaused { get; set; }

    private float currentSpeed;

    public bool grounded { get; private set; }
    private RaycastHit groundHit;

    private float originalHeight;
    private Vector3 originalCenter;

    public string debugString = "";
    #endregion

    private void Awake()
    {
        originalHeight = controller.height;
        originalCenter = controller.center;
        //pauseHandler = GameObject.FindGameObjectWithTag("Pause").GetComponent<PauseMenuHandler>();
        if (pauseHandler != null) { debugString += "Connection succesfull"; }
    }

    private void Update()
    {
        if (!IsOwner) return;

        //Ground Check
        grounded = IsGrounded(out groundHit);

        UpdateMovementState();

        HandleMovement();
        HandleGravity();
        HandleCrouch();
        oobTeleport();

    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;

        if (!IsPaused) moveInput = input.Move;

        Vector3 moveDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
    }

    private void oobTeleport()
    {
        if (transform.position.y < -50) transform.position = new Vector3(0, 20, 0);
    }

    private void HandleGravity()
    {
        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        if (grounded && input.JumpPressed && state != MovementState.Crouching)
        {
            float jumpStrength = jumpForce;

            if (state == MovementState.Sprinting)
                jumpStrength *= sprintJumpMultiplier;

            velocity.y = Mathf.Sqrt(jumpStrength * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        float targetHeight = state == MovementState.Crouching
        ? crouchingHeight
        : standingHeight;

        float previousHeight = controller.height;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        float heightDelta = controller.height - previousHeight;

        controller.center += Vector3.up * (heightDelta / 2f);
    }


    #region BG Work
    //Sets the state of the player
    private void UpdateMovementState()
    {
        if (!grounded)
        {
            state = MovementState.Airborne;
            currentSpeed = walkSpeed;
            return;
        }

        if (input.CrouchHeld || !HasCeilingClearance())
        {
            state = MovementState.Crouching;
            currentSpeed = crouchSpeed;
        }
        else if (input.SprintHeld && input.Move.y > 0f)
        {
            state = MovementState.Sprinting;
            currentSpeed = sprintSpeed;
        }
        else
        {
            state = MovementState.Default;
            currentSpeed = walkSpeed;
        }
    }


    //Finds where to check the ground from
    private Vector3 GetGroundCheckOrigin()
    {
        float bottomOffset = controller.center.y - controller.height * 0.5f + controller.radius;

        return transform.position + Vector3.up * bottomOffset;
    }


    //Checks for the ground
    private bool IsGrounded(out RaycastHit hit)
    {
        Vector3 origin = GetGroundCheckOrigin();

        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= maxSlopeAngle;
        }

        return false;
    }


    //Ceiling check for Uncrouch
    private bool HasCeilingClearance()
    {
        float standHeightDelta = standingHeight - crouchingHeight;
        if (standHeightDelta <= 0f)
            return true;

        Vector3 checkPosition = transform.position + Vector3.up * (controller.center.y + controller.height / 2f + standHeightDelta);

        return !Physics.CheckSphere(checkPosition, ceilingCheckRadius, ceilingMask, QueryTriggerInteraction.Ignore);
    }

    //DEBUGGIN VISUALS :3
    private void OnDrawGizmosSelected()
    {
        //Ground Check
        Gizmos.color = Color.yellow;
        Vector3 GC_origin = GetGroundCheckOrigin();
        Gizmos.DrawWireSphere(GC_origin + Vector3.down * groundCheckDistance, groundCheckRadius);


        //Uncrouch Check
        float standDelta = standingHeight - crouchingHeight;
        if (standDelta <= 0f) return;

        Gizmos.color = Color.red;
        Vector3 UC_origin = transform.position + Vector3.up * (controller.center.y + controller.height / 2f + standDelta);
        Gizmos.DrawWireSphere(UC_origin, ceilingCheckRadius);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
        }
    }
    #endregion
}
