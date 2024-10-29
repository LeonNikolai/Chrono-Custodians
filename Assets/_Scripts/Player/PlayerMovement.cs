using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] Animator _animator;
    public float stamina;
    public float xRotation;
    [SerializeField] private float moveSpeed, jumpForce, gravity, mouseSensitivity;
    [SerializeField] private float staminaUseAmount, staminaRegainAmount, interactRadius;
    [SerializeField] private float slideSpeed, slopeLimit, slideFriction;
    [SerializeField] private TMP_Text staminaText, speedText;
    [SerializeField] private Transform rotate;
    public Transform CameraTransform => rotate;
    private CharacterController characterController;
    private IHighlightable currentHighlightable;
    private Vector3 velocity = Vector3.zero, moveDirection = Vector3.zero;
    private float moveSpeedCurrent, staminaRegainTimer;
    private bool Grounded { 
        get => grounded;
        set
        {
            if (grounded == value) return;
            grounded = value;
            if(_animator) _animator.SetBool("Grounded", grounded);
        }
    }
    private bool grounded;
    
    public MovementStateManager movementState = new();
    private MovementModifierManager movementModifier = new();

    //private NetworkVariable<int> _currentModifier = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private void Awake()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (_player == null) _player = GetComponent<Player>();
    }
    private void Start()
    {
        if (!IsOwner) return;
        InitalizeMovement();
    }

    private void InitalizeMovement()
    {
        //ChangePosition(new Vector3(0f, 1f, 0f));
        Walk();
        stamina = 100f;
        Hud.Stamina = stamina / 100f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        Debug.Log("PlayerMovement OnNetworkSpawn : " + transform.position);
        InitalizeMovement();
        Player.Input.Player.Jump.performed += ctx => InputJump();
        Player.Input.Player.Crouch.performed += ctx => InputCrouch();
        Player.Input.Player.Crouch.canceled += ctx => Walk();
        Player.Input.Player.Interact.performed += ctx => InputInteract();
        xRotation = rotate.localRotation.eulerAngles.x;
        ChangePositionAndRotation(PlayerSpawner.getSpawnPointTransform());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsOwner) return;
        Player.Input.Player.Jump.performed -= ctx => InputJump();
        Player.Input.Player.Crouch.performed -= ctx => InputCrouch();
        Player.Input.Player.Crouch.canceled -= ctx => Walk();
        Player.Input.Player.Interact.performed -= ctx => InputInteract();
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (!IsOwner) { return; }
        if (Player.IsSpectating)
        {
            transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f);
            return;
        }
        CheckGround();
        CheckRaycast();
        CheckSprint();
        PlayerStamina();
        PlayerMove();
        PlayerCamera();
        if (transform.position.y < -200f)
        {
            ChangePositionAndRotation(PlayerSpawner.getSpawnPointTransform());
        }
    }

    private void Walk()
    {
        ChangeState(MovementState.Walking, 1f);
    }

    private void CheckGround()
    {
        Vector3 collision = new Vector3(characterController.bounds.center.x, characterController.bounds.min.y, characterController.bounds.center.z)
                            + Vector3.up * characterController.radius;

        if (Physics.CapsuleCast(collision, collision + Vector3.up * characterController.height,
            characterController.radius, Vector3.down, out RaycastHit groundHit, 0.1f) && velocity.y < 0f)
        {
            Grounded = true;
            velocity.y = 0f;
            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);

            if (slopeAngle > slopeLimit)
            {
                Grounded = false;
                Vector3 slideDirection = new Vector3(groundHit.normal.x, -groundHit.normal.y, groundHit.normal.z);
                slideDirection = Vector3.ProjectOnPlane(slideDirection, groundHit.normal).normalized;
                velocity += slideDirection * slideSpeed;
            }
            
            velocity.x = Mathf.Lerp(velocity.x, 0, slideFriction * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0, slideFriction * Time.deltaTime);
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    IInteractable currentInteractible;
    IInteractable CurrentInteractible
    {
        get => currentInteractible;
        set
        {
            if (currentInteractible == value)
            {
                UpdateHud();
            }
            currentInteractible = value;
            UpdateHud();
        }
    }

    private void UpdateHud()
    {
        if (currentInteractible == null)
        {
            Hud.CrosshairTooltip = "";
            return;
        }
        if (currentInteractible is IInteractionMessage message)
        {
            Hud.CrosshairTooltip = currentInteractible.Interactable ? message.InteractionMessage : message.CantInteractMessage;
            return;
        }
        Hud.CrosshairTooltip = currentInteractible is IInteractable && currentInteractible.Interactable ? "Press E to interact" : "Can't interact";
    }

    IHighlightable CurrentHighlightable
    {
        set
        {
            if (currentHighlightable == value) return;
            currentHighlightable?.HightlightExit();
            currentHighlightable = value;
            currentHighlightable?.HightlightEnter();
        }
    }

    private void CheckRaycast()
    {
        Ray ray = new(rotate.transform.position, rotate.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRadius))
        {
            if (hit.collider.TryGetComponent<IHighlightable>(out var highlightable))
            {
                CurrentHighlightable = highlightable;
                if (highlightable is IInteractable interactable)
                {
                    CurrentInteractible = interactable;
                }
                return;
            }
            else if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                CurrentHighlightable = null;
                CurrentInteractible = interactable;
                return;
            }
        }
        CurrentInteractible = null;
        CurrentHighlightable = null;
    }

    private void CheckSprint()
    {
        if (Player.Input.Player.Sprint.IsPressed() && movementState.currentState != MovementState.Jetpack && stamina > 0f)
        {
            movementModifier.ActivateModifier(MovementModifier.Sprinting);
            stamina -= staminaUseAmount * Time.deltaTime;
            staminaRegainTimer = 2f;
            Hud.Stamina = stamina / 100f;
        }
        else
        {
            movementModifier.DeactivateModifier(MovementModifier.Sprinting);
        }
    }

    private void PlayerStamina()
    {
        staminaRegainTimer -= Time.deltaTime;
        if (staminaRegainTimer <= 0f && stamina < 100f)
        {
            staminaRegainTimer = 0f;
            stamina += staminaRegainAmount * Time.deltaTime;
            Hud.Stamina = stamina / 100f;
        }
        if (staminaText) staminaText.text = $"Stamina: {stamina:F0} / 100";
    }

    private void PlayerMove()
    {
        if (movementState.currentState == MovementState.Jetpack && Player.Input.Player.Jump.IsPressed())
        {
            InputJump();
        }

        Vector2 input = Player.Input.Player.Move.ReadValue<Vector2>();

        if (input != Vector2.zero)
        {
            moveDirection =
                input.x * movementState.data.direction.X * transform.right +
                input.y * movementState.data.direction.Y * transform.up +
                input.y * movementState.data.direction.Z * transform.forward;
        }

        float lerpFactor = 1 - Mathf.Exp(-7.5f * Time.deltaTime);

        if (input != Vector2.zero)
        {
            moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, moveSpeed
                                        * movementState.data.speed
                                        * movementModifier.movementMultiplier, lerpFactor);
        }
        else
        {
            moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, 0f, lerpFactor * 2);
        }

        if (speedText) speedText.text = $"Speed: {moveSpeedCurrent:F2}";

        _animator.SetFloat("Speed", moveSpeedCurrent);
        characterController.Move(moveSpeedCurrent * Time.deltaTime * moveDirection);
    }

    private void PlayerCamera()
    {
        float mouseX = Player.Input.Player.Look.ReadValue<Vector2>().x * mouseSensitivity;
        float mouseY = Player.Input.Player.Look.ReadValue<Vector2>().y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        rotate.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void InputJump()
    {
        if (Grounded)
        {
            velocity.y = Mathf.Sqrt(-jumpForce * gravity);
            Grounded = false;
            _animator.SetTrigger("Jump");
        }

        else if (movementState.currentState == MovementState.Jetpack && stamina > 0f)
        {
            velocity.y = Mathf.Sqrt(-jumpForce * gravity * 0.1f);
            stamina -= staminaUseAmount * Time.deltaTime;
            staminaRegainTimer = 2f;
        }
    }

    private void InputCrouch()
    {
        if (Grounded)
        {
            ChangeState(MovementState.Crouching, 1f);
        }
    }

    private void InputInteract()
    {
        if (CurrentInteractible is not null && CurrentInteractible.Interactable)
        {
            CurrentInteractible.Interact(_player);
        }
        //ChangeState(MovementState.Jetpack, 1f);s
    }

    public void ChangeModifier(MovementModifier modifier, bool enable)
    {
        if (enable)
        {
            movementModifier.ActivateModifier(modifier);
        }
        else
        {
            movementModifier.DeactivateModifier(modifier);
        }
    }

    public void ChangeState(MovementState state, float gravityModifier)
    {
        gravity = -20f;
        movementState.GetMovementData(state);
        gravity *= gravityModifier;
    }

    public void ChangePosition(Vector3 position)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;
    }

    [Rpc(SendTo.Owner)]
    public void TeleportRpc(Vector3 position)
    {
        ChangePosition(position);
    }

    public void ChangePositionAndRotation(Transform target)
    {
        characterController.enabled = false;
        transform.position = target.position;
        transform.rotation = Quaternion.Euler(0f, target.rotation.eulerAngles.y, 0f);
        characterController.enabled = true;
    }
    public void ChangePositionAndRotation(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
        characterController.enabled = true;
    }
}