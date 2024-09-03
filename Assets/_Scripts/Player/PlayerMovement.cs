using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float stamina;
    [SerializeField] private float moveSpeedNormal, moveSpeedSprint, jumpForce, gravity, mouseSensitivity;
    [SerializeField] private float staminaUseAmount, staminaRegainAmount;
    [SerializeField] private Transform rotate;
    [SerializeField] private TMP_Text staminaText, speedText;
    private CharacterController characterController;
    private InputSystem_Actions playerActions;
    private Vector3 velocity, moveDirection;
    private float xRotation, moveSpeedCurrent, staminaRegainTimer;
    private bool grounded;
    
    

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        stamina = 100f;
    }

    private void OnEnable()
    {
        playerActions = new InputSystem_Actions();
        playerActions.Player.Jump.performed += ctx => InputJump();
        playerActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerActions.Player.Jump.performed -= ctx => InputJump();
        playerActions.Player.Disable();
    }

    private void Update()
    {
        CheckGround();
        Movement();
        Look();
        Stamina();
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
        

    private void Movement()
    {
        Vector2 input = playerActions.Player.Move.ReadValue<Vector2>();
        if (input != Vector2.zero) 
        {
            moveDirection = transform.right * input.x + transform.forward * input.y;
        }
        
    float lerpFactor = 1 - Mathf.Exp(-7.5f * Time.deltaTime);

    if (playerActions.Player.Sprint.IsPressed() && stamina > 0f)
    {
        moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, moveSpeedSprint, lerpFactor);
        stamina -= staminaUseAmount * Time.deltaTime;
        staminaRegainTimer = 2f;
    }
    else if (input != Vector2.zero)
    {
        moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, moveSpeedNormal, lerpFactor);
    }
    else
    {
        moveSpeedCurrent = Mathf.Lerp(moveSpeedCurrent, 0f, lerpFactor * 2);
    }
        
        speedText.text = $"Speed: {moveSpeedCurrent:F2}";
        characterController.Move(moveSpeedCurrent * Time.deltaTime * moveDirection);
    }

    private void Look()
    {
        float mouseX = playerActions.Player.Look.ReadValue<Vector2>().x * mouseSensitivity;
        float mouseY = playerActions.Player.Look.ReadValue<Vector2>().y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        rotate.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    
    private void Stamina() 
    {
        staminaRegainTimer -= Time.deltaTime;
        if (staminaRegainTimer <= 0f && stamina < 100f) 
        {
            staminaRegainTimer = 0f;
            stamina += staminaRegainAmount * Time.deltaTime;
        }
        staminaText.text = $"Stamina: {stamina:F0} / 100";
    }

    private void InputJump()
    {
        if (grounded)
        {
            velocity.y = Mathf.Sqrt(-jumpForce * gravity);
            grounded = false;
        }
    }
}
