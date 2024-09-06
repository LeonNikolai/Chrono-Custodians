using TMPro;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float stamina;
    [SerializeField] private float moveSpeed, jumpForce, gravity, mouseSensitivity;
    [SerializeField] private float staminaUseAmount, staminaRegainAmount;
    [SerializeField] private Transform rotate;
    [SerializeField] private TMP_Text staminaText, speedText;
    private CharacterController characterController;
    private Vector3 velocity, moveDirection;
    private float xRotation, moveSpeedCurrent, staminaRegainTimer;
    private bool grounded;
    
    private MovementStateManager movementState = new();
    private MovementModifierManager movementModifier = new();
    
    //private NetworkVariable<int> _currentModifier = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    private void Start()
    {
        if(!IsOwner) return;
        characterController = GetComponent<CharacterController>();
        movementState.GetMovementData(MovementState.Walking);
        stamina = 100f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); 
        if(!IsOwner) return;
        Player.Input.Player.Jump.performed += ctx => InputJump();
        Player.Input.Player.Crouch.performed += ctx => InputCrouch();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if(!IsOwner) return;
        Player.Input.Player.Jump.performed -= ctx => InputJump();
        Player.Input.Player.Crouch.performed -= ctx => InputCrouch();
    }

    private void Update()
    {
        if(!IsOwner) {return;}
        CheckGround();
        //CheckMovementState();
        CheckSprint();
        PlayerStamina();
        PlayerMove();
        PlayerCamera();
        
    }
    
    private void CheckGround()
    {
        Vector3 collision = new Vector3(characterController.bounds.center.x, characterController.bounds.min.y, characterController.bounds.center.z) 
                              + Vector3.up * characterController.radius;
        
        if (Physics.CapsuleCast(collision, collision + Vector3.up * characterController.height, 
        characterController.radius, Vector3.down, out RaycastHit groundHit, 0.2f) && velocity.y < 0f)
        {
            grounded = true; 
            velocity.y = 0f;
        }
        else 
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
    
    /*private void CheckMovementState() 
    {

    }*/
    
    private void CheckSprint()
    {
        if (Player.Input.Player.Sprint.IsPressed())
        {
            movementModifier.ActivateModifier(MovementModifier.Sprinting);
            stamina -= staminaUseAmount * Time.deltaTime;
            staminaRegainTimer = 2f;
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
        }
        if(staminaText) staminaText.text = $"Stamina: {stamina:F0} / 100";
    }

    private void PlayerMove()
    {
        Vector2 input = Player.Input.Player.Move.ReadValue<Vector2>();
        
        if (input != Vector2.zero)
        {
            moveDirection = 
                transform.right * input.x * movementState.data.direction.X + 
                transform.up * input.y * movementState.data.direction.Y + 
                transform.forward * input.y * movementState.data.direction.Z;
        }
        
        float lerpFactor = 1 - Mathf.Exp(-7.5f * Time.deltaTime);
        
        if (input != Vector2.zero)
        {
            moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, moveSpeed * movementModifier.movementMultiplier, lerpFactor);
        }
        else
        {
            moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, 0f, lerpFactor * 2);
        }
        
        if(speedText) speedText.text = $"Speed: {moveSpeedCurrent:F2}";
        characterController.Move(moveSpeedCurrent * Time.deltaTime * moveDirection);
    }

    private void PlayerCamera()
    {
        float mouseX = Player.Input.Player.Look.ReadValue<Vector2>().x * mouseSensitivity;
        float mouseY = Player.Input.Player.Look.ReadValue<Vector2>().y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        rotate.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    


    private void InputJump()
    {
        if (grounded)
        {
            velocity.y = Mathf.Sqrt(-jumpForce * gravity);
            grounded = false;
        }
    }
    
    private void InputCrouch()
    {
        if (grounded)
        {
            //currentState = MovementState.Crouching;
        }
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
    
    public void ChangeState(MovementState state) 
    {
        movementState.GetMovementData(state);
    }
}
