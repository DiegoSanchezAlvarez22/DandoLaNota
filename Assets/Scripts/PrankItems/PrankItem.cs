using UnityEngine;
using System.Collections;

public class PrankItem : MonoBehaviour, IInteractable
{
    [Header("Datos")]
    [SerializeField] protected GamberradaData prankData;
    [SerializeField] protected int pointsAwarded = 500;

    protected bool isHeld = false;
    protected Collider itemCollider;
    protected Rigidbody itemRigidbody;
    protected AudioSource audioSource;
    protected PlayerInteraction currentPlayer;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // AÑADIDO 'virtual' para que el chicle pueda leer a dónde miramos
    public virtual string GetInteractPrompt()
    {
        if (!isHeld) return "[E] Recoger " + (prankData != null ? prankData.prankName : "Objeto");
        return "[E] Realizar Gamberrada\n[Q] Soltar";
    }

    // AÑADIDO 'virtual' para que el chicle compruebe si el objetivo es válido antes del QTE
    public virtual void OnInteract(PlayerInteraction player)
    {
        if (!isHeld) PickUp(player);
        else 
        {
            currentPlayer = player;
            if (QTEManager.Instance != null && prankData != null)
            {
                currentPlayer.SetFrozen(true);
                QTEManager.Instance.StartMinigame(prankData.minigameType, OnMinigameResult);
            }
        }
    }

    // AÑADIDO 'protected' para que el chicle pueda usarlo
    protected void PickUp(PlayerInteraction player)
    {
        isHeld = true;
        player.EquipItem(this);

        if (itemCollider != null) itemCollider.enabled = false;
        if (itemRigidbody != null) itemRigidbody.isKinematic = true;

        transform.SetParent(player.GetHandSocket());
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void ForceEquip(PlayerInteraction player)
    {
        isHeld = true;
        currentPlayer = player;
        player.EquipItem(this);

        if (itemCollider != null) itemCollider.enabled = false;
        if (itemRigidbody != null) itemRigidbody.isKinematic = true;

        transform.SetParent(player.GetHandSocket());
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public virtual void Drop(Vector3 throwForce)
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

    protected void OnMinigameResult(bool success)
    {
        StartCoroutine(EjecutarGamberradaRutina(success));
    }

    private IEnumerator EjecutarGamberradaRutina(bool qteExito)
    {
        if (currentPlayer == null || prankData == null) yield break;

        currentPlayer.SetFrozen(true);
        float tiempoTotal = prankData.tiempoEjecucionBase;

        if (!qteExito)
        {
            if (prankData.penalizacionPorFallo == TipoPenalizacion.AumentarTiempo)
                tiempoTotal += prankData.tiempoPenalizacion;
            else if (prankData.penalizacionPorFallo == TipoPenalizacion.HacerRuido)
                if (prankData.sonidoPenalizacion != null) audioSource.PlayOneShot(prankData.sonidoPenalizacion);
        }

        while (tiempoTotal > 0)
        {
            currentPlayer.UpdateActionTimer(tiempoTotal);
            tiempoTotal -= Time.deltaTime;
            yield return null; 
        }

        currentPlayer.UpdateActionTimer(0);
        currentPlayer.SetFrozen(false);
        AccionFinalExitosa();
    }

    protected virtual void AccionFinalExitosa()
    {
        currentPlayer.ClearHand();
        Destroy(gameObject);
    }
}