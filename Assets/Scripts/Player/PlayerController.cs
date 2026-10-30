using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public bool IsSeated => isSeated;
   
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.8f;

    [Header("Configuración de Cámara")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;

    [Header("Estado del Jugador")]
    [SerializeField] private bool isSeated = false;
    [SerializeField] private float standingHeight = 0.5f;
    [SerializeField] private float seatedHeight = 0.3f;

    [Header("Controles (New Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference standAction;

    [Header("Interacciones")]
    [SerializeField] private PlayerInteraction playerInteraction;

    private CharacterController characterController;
    private float xRotation = 0f;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        standAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        standAction.action.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

private void Update()
    {
        if (playerInteraction != null && playerInteraction.IsFrozen)
        {
            ApplyGravityOnly(); 
            return; 
        }

        HandleMouseLook();

        if (isSeated)
        {
            if (standAction.action.WasPressedThisFrame())
            {
                ChangeSeatedState(false);
            }
            return;
        }

        HandleMovement();
    }

    private void ApplyGravityOnly()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void ChangeSeatedState(bool state)
    {
        isSeated = state;

        if (isSeated)
        {
            cameraTransform.localPosition = new Vector3(0, seatedHeight, 0);
        }
        else
        {
            cameraTransform.localPosition = new Vector3(0, standingHeight, 0);
        }
    }
}