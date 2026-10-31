using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform handSocket;
    
    [Header("Interfaz de Usuario")]
    [SerializeField] private TMP_Text interactionPromptUI;
    [SerializeField] private TMP_Text actionTimerUI; 

    [Header("Configuración del Raycast")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float dropForce = 4f;

    [Header("Bolsillo (Teléfono)")]
    [SerializeField] private GameObject phonePrefab; // El prefab del móvil que instanciará
    [SerializeField] private InputActionReference phoneAction; // Acción de la tecla V

    [Header("Controles")]
    [SerializeField] private InputActionReference interactAction; 
    [SerializeField] private InputActionReference dropAction;     

    private IInteractable currentTarget;
    private PrankItem heldItem;

    private float lastInteractTime = 0f;
    private float interactCooldown = 0.2f;

    public bool IsFrozen { get; private set; }

    // Memoria para saber qué gamberrada estamos ejecutando ahora mismo
    private GamberradaData currentPrankData;
    public GamberradaData CurrentPrankData => currentPrankData;

    private void OnEnable()
    {
        if (interactAction != null) { interactAction.action.Enable(); interactAction.action.performed += ContextInteract; }
        if (dropAction != null) { dropAction.action.Enable(); dropAction.action.performed += DropInteract; }
        if (phoneAction != null) { phoneAction.action.Enable(); phoneAction.action.performed += TogglePhone; }
    }

    private void OnDisable()
    {
        if (interactAction != null) { interactAction.action.performed -= ContextInteract; interactAction.action.Disable(); }
        if (dropAction != null) { dropAction.action.performed -= DropInteract; dropAction.action.Disable(); }
        if (phoneAction != null) { phoneAction.action.performed -= TogglePhone; phoneAction.action.Disable(); }
    }

    private void Update()
    {
        if (IsFrozen)
        {
            if (interactionPromptUI != null) interactionPromptUI.text = "Ejecutando...";
            return;
        }

        if (heldItem != null)
        {
            currentTarget = null;
            if (interactionPromptUI != null) interactionPromptUI.text = heldItem.GetInteractPrompt();
            return; 
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                if (interactionPromptUI != null) interactionPromptUI.text = currentTarget.GetInteractPrompt();
                return;
            }
        }

        currentTarget = null;
        if (interactionPromptUI != null) interactionPromptUI.text = "";
    }

    // Función para guardar o borrar la gamberrada actual
    public void SetCurrentPrank(GamberradaData data)
    {
        currentPrankData = data;
    }

    private void ContextInteract(InputAction.CallbackContext context)
    {
        if (IsFrozen || Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (heldItem != null) heldItem.OnInteract(this);
        else if (currentTarget != null) currentTarget.OnInteract(this);
    }

    private void DropInteract(InputAction.CallbackContext context)
    {
        if (IsFrozen) return; 

        if (heldItem != null)
        {
            heldItem.Drop(playerCamera.forward * dropForce);
        }
    }

    private void TogglePhone(InputAction.CallbackContext context)
    {
        if (IsFrozen || phonePrefab == null) return;

        // Si ya tenemos el móvil en la mano, lo guardamos (tecla V funciona como On/Off)
        if (heldItem != null && heldItem is PhonePrankItem)
        {
            heldItem.Drop(Vector3.zero);
            return;
        }

        // Si tenemos la mano vacía, instanciamos el móvil y lo equipamos
        if (heldItem == null)
        {
            GameObject phoneObj = Instantiate(phonePrefab);
            PhonePrankItem phoneItem = phoneObj.GetComponent<PhonePrankItem>();
            
            if (phoneItem != null)
            {
                phoneItem.ForceEquip(this);
            }
        }
    }

    public void SetFrozen(bool state) => IsFrozen = state;
    public void UpdateActionTimer(float timeLeft)
    {
        if (actionTimerUI != null) actionTimerUI.text = timeLeft > 0 ? timeLeft.ToString("F1") + "s" : "";
    }
    public Transform GetHandSocket() => handSocket;
    public void EquipItem(PrankItem item) => heldItem = item;
    public void ClearHand()
    {
        heldItem = null;
        SetCurrentPrank(null);
    } 
    public Transform GetPlayerCamera() => playerCamera; 
}