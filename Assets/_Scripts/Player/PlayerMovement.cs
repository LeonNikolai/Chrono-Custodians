using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed, jumpForce, gravity, mouseSensitivity;
    [SerializeField] private Transform rotate;
    private CharacterController characterController;
    private InputSystem_Actions playerActions;
    private Vector3 velocity;
    private float xRotation;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        Movement();
        Look();
    }

    private void Movement()
    {
        Vector2 input = playerActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        characterController.Move(moveSpeed * Time.deltaTime * move);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
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

    private void InputJump()
    {
        Debug.Log("heee");
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-jumpForce * gravity);
        }
    }
}
