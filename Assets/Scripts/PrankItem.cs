using UnityEngine;

public class PrankItem : MonoBehaviour, IInteractable
{
    [Header("Datos")]
    [SerializeField] protected GamberradaData prankData;
    [SerializeField] protected int pointsAwarded = 500;
    
    [Header("Penalización")]
    [SerializeField] private AudioClip failNoise;

    private bool isHeld = false;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;
    private AudioSource audioSource;
    private PlayerInteraction currentPlayer;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
        
        // Nos aseguramos de tener un emisor de sonido para el ruido de caída
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public string GetInteractPrompt()
    {
        if (!isHeld) return "[E] Recoger " + (prankData != null ? prankData.prankName : "Objeto");
        return "[E] Realizar Gamberrada\n[Q] Soltar";
    }

    public void OnInteract(PlayerInteraction player)
    {
        if (!isHeld)
        {
            PickUp(player);
        }
        else
        {
            currentPlayer = player;
            // Lanzamos el minijuego asociado y le pasamos nuestra función de respuesta
            if (QTEManager.Instance != null && prankData != null)
            {
                QTEManager.Instance.StartMinigame(prankData.minigameType, OnMinigameResult);
            }
        }
    }

    private void PickUp(PlayerInteraction player)
    {
        isHeld = true;
        player.EquipItem(this);

        if (itemCollider != null) itemCollider.enabled = false;
        if (itemRigidbody != null) itemRigidbody.isKinematic = true;

        transform.SetParent(player.GetHandSocket());
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop(Vector3 throwForce)
    {
        isHeld = false;
        transform.SetParent(null); 
        
        if (itemCollider != null) itemCollider.enabled = true;
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.AddForce(throwForce, ForceMode.Impulse);
        }
    }

    // --- LÓGICA DE QTE ---

    private void OnMinigameResult(bool success)
    {
        if (success) ExecuteSuccessAction();
        else ExecuteFailAction();
    }

    /// <summary>
    /// Ejecución base en caso de superar el minijuego. (Virtual para personalizar en el futuro)
    /// </summary>
    protected virtual void ExecuteSuccessAction()
    {
        Debug.Log("Minijuego superado. Acción ejecutada con éxito.");

        if (prankData != null && AchievementManager.Instance != null)
            AchievementManager.Instance.UnlockPrank(prankData.id);

        if (GameManager.Instance != null)
            GameManager.Instance.AddLaughPoints(pointsAwarded);

        // TODO: Aquí añadiremos la animación de lanzar o usar el objeto
        currentPlayer.ClearHand();
        Destroy(gameObject);
    }

    /// <summary>
    /// Ejecución en caso de fallar el minijuego.
    /// </summary>
    protected virtual void ExecuteFailAction()
    {
        Debug.Log("Minijuego fallado. El objeto se ha caído haciendo ruido.");
        
        currentPlayer.ClearHand();
        
        // Cae directamente al suelo desde la mano
        Drop(Vector3.down * 2f); 

        // Reproduce el ruido que llamará la atención del profesor
        if (failNoise != null)
        {
            audioSource.PlayOneShot(failNoise);
            // TODO: Notificar al profesor usando su radio de audición
        }
    }
}