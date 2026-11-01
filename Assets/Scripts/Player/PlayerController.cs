using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public bool IsSeated => isSeated;
   
    [Header("Configuración de Movimiento")]
    // Velocidad de desplazamiento por el aula
    [SerializeField] private float moveSpeed = 5f;
    // Fuerza de la gravedad aplicada al jugador
    [SerializeField] private float gravity = -9.8f;

    [Header("Configuración de Cámara")]
    // Sensibilidad del ratón
    [SerializeField] private float mouseSensitivity = 20f;
    // Referencia a la cámara principal emparentada al jugador
    [SerializeField] private Transform cameraTransform;

    [Header("Estado del Jugador")]
    // Determina si el jugador se encuentra actualmente sentado en el pupitre
    [SerializeField] private bool isSeated = false;
    // Altura normal de la cámara cuando el jugador está de pie
    [SerializeField] private float standingHeight = 0.5f;
    // Altura de la cámara cuando el jugador está sentado
    [SerializeField] private float seatedHeight = 0.3f;

    [Header("Controles (New Input System)")]
    // Referencia a la acción de movimiento
    [SerializeField] private InputActionReference moveAction;
    // Referencia a la acción de mirar
    [SerializeField] private InputActionReference lookAction;
    // Referencia a la acción de levantarse
    [SerializeField] private InputActionReference standAction;

    // Componente interno para gestionar colisiones y movimiento
    private CharacterController characterController;
    // Acumulador de la rotación vertical para limitar la vista
    private float xRotation = 0f;
    // Vector de velocidad vertical para aplicar la gravedad
    private Vector3 velocity;

    [SerializeField] private PlayerInteraction playerInteraction; // Referencia al script de interacción del jugador

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        if (playerInteraction == null)
        {
            playerInteraction = GetComponent<PlayerInteraction>();
            
            if (playerInteraction == null)
            {
                playerInteraction = GetComponentInChildren<PlayerInteraction>();
            }
        }
    }

    /// <summary>
    /// Se llama cuando el script se habilita
    /// </summary>
    private void OnEnable()
    {
        // Activación de las acciones de control al encender el script
        moveAction.action.Enable();
        lookAction.action.Enable();
        standAction.action.Enable();
    }

    /// <summary>
    /// Se llama cuando el script se deshabilita
    /// </summary>
    private void OnDisable()
    {
        // Desactivación de las acciones de control al apagar el script
        moveAction.action.Disable();
        lookAction.action.Disable();
        standAction.action.Disable();
    }

    private void Start()
    {
        // Bloqueo y ocultación del cursor SOLO si el menú de inicio NO está abierto
        if (!MatchStartUIController.isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Si hay un menú abierto (como el de inicio), bloqueamos visión y movimiento, pero aplicamos gravedad
        if (MatchStartUIController.isMenuOpen)
        {
            ApplyGravityOnly();
            return;
        }

        // 1. Manejamos siempre la visión (si no hay menús abiertos) para poder mirar durante el castigo
        HandleMouseLook();

        // 2. Si estamos en medio del tiempo de castigo/tensión, bloqueamos el movimiento corporal, pero aplicamos gravedad
        if (playerInteraction != null && playerInteraction.IsFrozen)
        {
            ApplyGravityOnly(); 
            return; 
        }

        if (isSeated)
        {
            if (standAction.action.WasPressedThisFrame())
            {
                ChangeSeatedState(false);
            }
            return;
        }

        // 3. Manejamos el movimiento lateral solo si no estamos congelados ni sentados
        HandleMovement();
    }

    /// <summary>
    /// Aplica únicamente la fuerza de la gravedad sin permitir movimiento lateral.
    /// Útil cuando el jugador está congelado por un minijuego, menú o castigo.
    /// </summary>
    private void ApplyGravityOnly()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Maneja la mirada del jugador basada en la entrada del ratón
    /// </summary>
    private void HandleMouseLook()
    {
        // Obtiene el valor global si el SettingsManager existe en la escena
        if (SettingsManager.Instance != null)
        {
            // Nota: Cambiado a currentSensitivity suponiendo que exista en SettingsManager, 
            // de lo contrario habría que adaptarlo.
            // mouseSensitivity = SettingsManager.Instance.currentSensitivity;
        }

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Maneja el movimiento del jugador basado en la entrada del teclado
    /// </summary>
    private void HandleMovement()
    {
        // Lectura del vector 2D generado por las teclas WASD
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        // Creación del vector de dirección basado en la rotación local del jugador
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Aplicación del movimiento a través del CharacterController
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Aplicación básica de la gravedad
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Modifica el estado del jugador entre sentado y de pie.
    /// </summary>
    /// <param name="state">True para sentarse, False para levantarse</param>
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