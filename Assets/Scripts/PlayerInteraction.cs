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

    [Header("Configuración del Raycast")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float dropForce = 4f;

    [Header("Controles")]
    [SerializeField] private InputActionReference interactAction; 
    [SerializeField] private InputActionReference dropAction;     

    private IInteractable currentTarget;
    private PrankItem heldItem;

    // Escudo anti-doble pulsación
    private float lastInteractTime = 0f;
    private float interactCooldown = 0.2f;

    private void OnEnable()
    {
        if (interactAction != null) {
            interactAction.action.Enable();
            interactAction.action.performed += ContextInteract;
        }
        if (dropAction != null) {
            dropAction.action.Enable();
            dropAction.action.performed += DropInteract;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null) {
            interactAction.action.performed -= ContextInteract;
            interactAction.action.Disable();
        }
        if (dropAction != null) {
            dropAction.action.performed -= DropInteract;
            dropAction.action.Disable();
        }
    }

    private void Update()
    {
        // ESTADO 1: MANO OCUPADA
        if (heldItem != null)
        {
            currentTarget = null;
            if (interactionPromptUI != null) 
                interactionPromptUI.text = heldItem.GetInteractPrompt();
            return; 
        }

        // ESTADO 2: MANO VACÍA
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                currentTarget = interactable;
                if (interactionPromptUI != null) 
                    interactionPromptUI.text = currentTarget.GetInteractPrompt();
                return;
            }
        }

        currentTarget = null;
        if (interactionPromptUI != null) 
            interactionPromptUI.text = "";
    }

    private void ContextInteract(InputAction.CallbackContext context)
    {
        // Evita que el botón se pulse dos veces en menos de 0.2 segundos
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (heldItem != null)
        {
            heldItem.OnInteract(this);
            return; 
        }

        if (currentTarget != null)
        {
            currentTarget.OnInteract(this);
        }
    }

    private void DropInteract(InputAction.CallbackContext context)
    {
        if (heldItem != null)
        {
            heldItem.Drop(playerCamera.forward * dropForce);
            ClearHand();
        }
    }

    public Transform GetHandSocket() => handSocket;
    public void EquipItem(PrankItem item) => heldItem = item;
    public void ClearHand() => heldItem = null;
}