using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    private ILongInteractable currentLongInteractable;
    private float currentInteractTime = 0;
    private Vector3 velocity = Vector3.zero, moveDirection = Vector3.zero;
    private float moveSpeedCurrent, staminaRegainTimer;
    private NetworkVariable<float> MoveSpeedCurrent = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> IsGrounded = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool Grounded
    {
        get => grounded;
        set
        {
            if (_animator) _animator.SetBool("Grounded", value);
            grounded = value;
            IsGrounded.Value = grounded;

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

        MoveSpeedCurrent.OnValueChanged += MoveSpeedCurrent_OnValueChanged;
        IsGrounded.OnValueChanged += IsGrounded_OnValueChanged;
        IsGrounded_OnValueChanged(IsGrounded.Value,IsGrounded.Value);
        if (!IsOwner) return;
        Debug.Log("PlayerMovement OnNetworkSpawn : " + transform.position);
        InitalizeMovement();
        Player.Input.Player.Jump.performed += ctx => InputJump();
        Player.Input.Player.Crouch.performed += ctx => InputCrouch();
        Player.Input.Player.Crouch.canceled += ctx => Walk();
        Player.Input.Player.Interact.performed += ctx => InputInteract(); 
        Player.Input.Player.Interact.started += ctx => InputLongInteract();
        Player.Input.Player.Interact.canceled += ctx => CancelLongInteract();
        xRotation = rotate.localRotation.eulerAngles.x;
        ChangePositionAndRotation(PlayerSpawner.getSpawnPointTransform());

    }

    [Rpc(SendTo.Owner)]
    public void DisableInputsRPC(bool disable)
    {
        if (!IsLocalPlayer) return;
        if (disable)
        {
            Player.Input.Player.Disable();
        }
        else
        {
            Player.Input.Player.Enable();
        }
        
    }

    private void IsGrounded_OnValueChanged(bool previousValue, bool newValue)
    {
        if (_animator) _animator.SetBool("Grounded", newValue);
    }

    private void MoveSpeedCurrent_OnValueChanged(float previousValue, float newValue)
    {
        if(_animator) _animator.SetFloat("Speed", newValue);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (!IsOwner) return;
        Player.Input.Player.Jump.performed -= ctx => InputJump();
        Player.Input.Player.Crouch.performed -= ctx => InputCrouch();
        Player.Input.Player.Crouch.canceled -= ctx => Walk();
        Player.Input.Player.Interact.performed -= ctx => InputInteract();
        Player.Input.Player.Interact.started -= ctx => InputLongInteract();
        Player.Input.Player.Interact.canceled -= ctx => CancelLongInteract();
    }


    private void Update()
    {
        if (!IsSpawned) return;
        if (!IsOwner)
        {
            return;
        }
        if (Player.IsSpectating)
        {
            transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f);
            return;
        }
        CheckGround();
        characterController.Move(velocity * Time.deltaTime);
        InteractionRaycast();
        CheckSprint();
        PlayerStamina();
        PlayerMove();
        PlayerCamera();
        if (transform.position.y < -200f)
        {
            Player.LocalPlayer.Location = LocationType.Outside;
            ChangePositionAndRotation(PlayerSpawner.getSpawnPointTransform());
        }
        if (isInteracting)
        {
            if (CurrentLongInteractible == null)
            {
                isInteracting = false;
                currentInteractTime = 0;
                Hud.LongInteract.value = 0;
                return;
            }
            currentInteractTime += Time.deltaTime;
            Debug.Log(currentInteractTime);
            Hud.LongInteract.value = currentInteractTime / currentLongInteractable.InteractTime;
            if (currentInteractTime > currentLongInteractable.InteractTime)
            {
                Debug.Log("Interacting");
                currentLongInteractable.LongInteract(_player);
                isInteracting = false;
                currentInteractTime = 0;
            }
        }
        else if (currentInteractTime > 0)
        {
            currentInteractTime = 0;
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
            characterController.radius, Vector3.down, out RaycastHit groundHit, 0.15f) && velocity.y < 0f)
        {
            velocity.y = 0f;
            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);

            if (slopeAngle > slopeLimit)
            {
                Grounded = false;
                Vector3 slideDirection = new Vector3(groundHit.normal.x, -groundHit.normal.y, groundHit.normal.z);
                slideDirection = Vector3.ProjectOnPlane(slideDirection, groundHit.normal).normalized;
                velocity += slideDirection * slideSpeed;
            }
            else
            {
                Grounded = true;
            }

            velocity.x = Mathf.Lerp(velocity.x, 0, slideFriction * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0, slideFriction * Time.deltaTime);
        }
        
        else 
        {
            Grounded = false;
        }

        velocity.y += gravity * Time.deltaTime;

    }

    IInteractable currentInteractible;
    IInteractable CurrentInteractible
    {
        get => currentInteractible;
        set
        {
            if (currentInteractible == value)
            {
                UpdateInteractableHud();
            }
            currentInteractible = value;
            UpdateInteractableHud();
        }
    }

    ILongInteractable CurrentLongInteractible
    {
        get => currentLongInteractable;
        set
        {
            if (currentLongInteractable == value)
            {
                UpdateLongInteractableHud();
            }
            currentLongInteractable = value;
            UpdateLongInteractableHud();
        }
    }

    private void UpdateLongInteractableHud()
    {
        if (currentLongInteractable == null)
        {
            // Debug.Log("Not looking at it anymore");
            Hud.LongInteract.value = 0;
            Hud.LongInteract.gameObject.SetActive(false); 
            Hud.CrosshairTooltip = "";
            return;
        }
        else
        {
            Debug.Log("Looking at it");
            Hud.LongInteract.gameObject.SetActive(true);
        }
        if (currentLongInteractable is IInteractionMessage message)
        {
            Hud.CrosshairTooltip = message.InteractionMessage;
            return;
        }
        Hud.CrosshairTooltip = currentLongInteractable is ILongInteractable ? "Hold E to interact" : "Can't interact";
    }

    private void UpdateInteractableHud()
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

    private void InteractionRaycast()
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
            else if (hit.collider.TryGetComponent<ILongInteractable>(out var longInteractable))
            {
                Debug.Log("Hit longInteractable");
                CurrentLongInteractible = longInteractable;
                return;
            }
        }
        CurrentInteractible = null;
        CurrentLongInteractible = null;
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


        MoveSpeedCurrent.Value = moveSpeedCurrent;
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

    bool isInteracting = false;
    private void InputLongInteract()
    {
        if (CurrentLongInteractible == null) return;
        isInteracting = true;
    }
    private void CancelLongInteract()
    {
        if (CurrentLongInteractible == null) return;
        Hud.LongInteract.value = 0;
        isInteracting = false;
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
    [Rpc(SendTo.Owner)]
    public void TeleportRpc(Vector3 position,LocationType locationType)
    {
        _player.Location = locationType;
        ChangePosition(position);
    }
    [Rpc(SendTo.Owner)]
    public void TeleportAndRotateRpc(Vector3 position, Quaternion rotation)
    {
        ChangePositionAndRotation(position, rotation);
    }

    public void TeleportToSpawn()
    {
        ChangePositionAndRotation(PlayerSpawner.getSpawnPointTransform());
    }
    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        TeleportToSpawn();
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