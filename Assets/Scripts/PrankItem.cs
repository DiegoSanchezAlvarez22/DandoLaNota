using UnityEngine;

public class PrankItem : MonoBehaviour, IInteractable
{
    [Header("Datos")]
    [SerializeField] private GamberradaData prankData;
    [SerializeField] private int pointsAwarded = 500;

    private bool isHeld = false;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
    }

    public string GetInteractPrompt()
    {
        if (!isHeld)
        {
            string itemName = prankData != null ? prankData.prankName : "Objeto";
            return "[E] Recoger " + itemName;
        }
        else
        {
            return "[E] Realizar Gamberrada\n[Q] Soltar";
        }
    }

    public void OnInteract(PlayerInteraction player)
    {
        if (!isHeld)
        {
            PickUp(player);
        }
        else
        {
            ExecutePrank(player);
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

    private void ExecutePrank(PlayerInteraction player)
    {
        Debug.Log("¡Gamberrada completada de forma segura!");

        // Guarda el logro en el anexo
        if (prankData != null && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UnlockPrank(prankData.id);
        }

        // Suma los puntos en la partida actual
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLaughPoints(pointsAwarded);
        }

        // Limpia la mano y borra el objeto
        player.ClearHand();
        Destroy(gameObject);
    }
}