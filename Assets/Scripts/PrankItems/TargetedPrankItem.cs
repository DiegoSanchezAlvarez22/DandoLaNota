using UnityEngine;

public class TargetedPrankItem : PrankItem
{
    [Header("Ajustes de Objetivo")]
    [SerializeField] private float distanciaAplicacion = 2.5f; // Rango cuerpo a cuerpo
    [SerializeField] private GameObject modeloPegadoPrefab; // El chicle aplastado visual

    // Variables internas para recordar dónde y a quién hemos apuntado
    private PrankTarget objetivoFijado;
    private Vector3 puntoDeImpacto;
    private Quaternion rotacionDeImpacto;
    private Transform transformObjetivo;

    // Cambia el texto del HUD en tiempo real dependiendo de a qué miremos
    public override string GetInteractPrompt()
    {
        if (!isHeld) return base.GetInteractPrompt();

        Ray ray = new Ray(currentPlayer.GetPlayerCamera().position, currentPlayer.GetPlayerCamera().forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distanciaAplicacion))
        {
            PrankTarget target = hit.collider.GetComponentInParent<PrankTarget>();
            if (target != null)
            {
                return $"[E] Aplicar en {target.nombreObjetivo}\n[Q] Soltar";
            }
        }
        return "Acércate a un objetivo (Alumno, Profesor, Pared...)\n[Q] Soltar";
    }

    // Solo nos deja iniciar el QTE si estamos apuntando a un objetivo válido
    public override void OnInteract(PlayerInteraction player)
    {
        if (!isHeld) 
        {
            PickUp(player);
            return;
        }

        Ray ray = new Ray(player.GetPlayerCamera().position, player.GetPlayerCamera().forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distanciaAplicacion))
        {
            PrankTarget target = hit.collider.GetComponentInParent<PrankTarget>();
            if (target != null)
            {
                // Guardamos los datos del punto exacto donde lo hemos pegado
                objetivoFijado = target;
                puntoDeImpacto = hit.point;
                rotacionDeImpacto = Quaternion.LookRotation(hit.normal);
                transformObjetivo = hit.collider.transform;
                
                currentPlayer = player;
                
                if (QTEManager.Instance != null && prankData != null)
                {
                    currentPlayer.SetFrozen(true);
                    currentPlayer.SetCurrentPrank(prankData);
                    QTEManager.Instance.StartMinigame(prankData.minigameType, OnMinigameResult);
                }
            }
        }
    }

    protected override void AccionFinalExitosa()
    {
        if (AchievementManager.Instance != null && prankData != null)
            AchievementManager.Instance.UnlockPrank(prankData.id);

        // Multiplicadores de puntuación por riesgo
        int puntosFinales = pointsAwarded;
        if (objetivoFijado != null)
        {
            if (objetivoFijado.tipo == TargetType.Alumno) puntosFinales *= 2;      // El doble por alumno
            else if (objetivoFijado.tipo == TargetType.Profesor) puntosFinales *= 3; // El triple por profesor
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddLaughPoints(puntosFinales);

        // Instanciamos el chicle aplastado visualmente en la superficie que apuntamos
        if (modeloPegadoPrefab != null && transformObjetivo != null)
        {
            GameObject pegote = Instantiate(modeloPegadoPrefab, puntoDeImpacto, rotacionDeImpacto);
            pegote.transform.SetParent(transformObjetivo); // Lo emparentamos para que se mueva con el alumno si camina
        }

        currentPlayer.ClearHand();
        Destroy(gameObject);
    }
}