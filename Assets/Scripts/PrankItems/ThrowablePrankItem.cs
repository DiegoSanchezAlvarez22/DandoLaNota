using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThrowablePrankItem : PrankItem
{
    [Header("Lanzamiento con carga")]
    [SerializeField] private float tiempoCargaMax = 1.2f;   // segundos hasta carga completa
    [SerializeField] private float velocidadMinima = 4f;    // m/s con carga mínima
    [SerializeField] private float velocidadMaxima = 14f;   // m/s con carga completa
    [SerializeField] private float tiempoDeVidaTrasLanzar = 4f;

    [Header("Barra de carga (opcional)")]
    [SerializeField] private string nombreBarraCarga = "Img_CargaLanzamiento";

    private bool cargando;
    private float carga;
    private Image barra;

    private bool lanzado;           // NUEVO
    private bool golpeoProfesor;    // NUEVO

    public bool Cargando => cargando;   // NUEVO

    public override string GetInteractPrompt()
    {
        if (!isHeld) return base.GetInteractPrompt();
        return "[Clic] Mantén para cargar, suelta para lanzar\n[Q] Soltar";
    }

    public override void OnInteract(PlayerInteraction player)
    {
        // Solo recoger; ya no hay minijuego
        if (!isHeld)
        {
            base.OnInteract(player);
            currentPlayer = player;
        }
    }

    private void Update()
    {
        if (!isHeld || currentPlayer == null || currentPlayer.IsFrozen
            || Time.timeScale == 0f || Mouse.current == null)
        {
            if (cargando) CancelarCarga();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            cargando = true;
            carga = 0f;
        }

        if (!cargando) return;

        carga = Mathf.Clamp01(carga + Time.deltaTime / tiempoCargaMax);
        ActualizarBarra(carga, true);

        if (Mouse.current.leftButton.wasReleasedThisFrame) Lanzar();
    }

    private void Lanzar()
    {
        float velocidad = Mathf.Lerp(velocidadMinima, velocidadMaxima, carga);
        CancelarCarga();

        if (AchievementManager.Instance != null && prankData != null)
            AchievementManager.Instance.UnlockPrank(prankData.id);
        if (GameManager.Instance != null)
            GameManager.Instance.AddLaughPoints(pointsAwarded);

        float masa = itemRigidbody != null ? itemRigidbody.mass : 1f;
        Vector3 impulso = currentPlayer.GetPlayerCamera().forward * velocidad * masa;

        currentPlayer.ClearHand();
        lanzado = true;             // NUEVO
        Drop(impulso);
        Destroy(gameObject, tiempoDeVidaTrasLanzar);
    }

    // NUEVO: si la bola lanzada toca al profesor, reacciona
    private void OnCollisionEnter(Collision c)
    {
        Debug.Log("Bola choca con: " + c.collider.name + " | lanzado: " + lanzado);

        if (!lanzado || golpeoProfesor) return;

        TeacherAI profe = c.collider.GetComponentInParent<TeacherAI>();
        if (profe == null) return;

        golpeoProfesor = true;
        profe.GetHitByObject();
    }

    private void CancelarCarga()
    {
        cargando = false;
        carga = 0f;
        ActualizarBarra(0f, false);
    }

    private void ActualizarBarra(float valor, bool visible)
    {
        if (barra == null)
        {
            GameObject go = GameObject.Find(nombreBarraCarga);
            if (go != null) barra = go.GetComponent<Image>();
        }
        if (barra == null) return;
        barra.fillAmount = valor;
        barra.enabled = visible;
    }
}