using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherAI : MonoBehaviour
{
    public enum TeacherState { Patrolling, Suspicious, Catching }

    [Header("Estado Actual")]
    [SerializeField] private TeacherState currentState = TeacherState.Patrolling;

    [Header("Configuración de Patrulla")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTimeAtWaypoint = 2f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("Configuración de Visión")]
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private Transform eyesTransform;
    [SerializeField] private LayerMask obstacleMask; // Solo para PAREDES o PUERTAS. ¡Asegúrate de que el Jugador NO está en esta capa!

    [Header("Indicadores Visuales")]
    [SerializeField] private GameObject questionMarkIcon;
    [SerializeField] private GameObject exclamationMarkIcon;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(false);
        
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
            Debug.Log("[Profesor] Iniciando patrulla hacia el waypoint 0.");
        }
        else
        {
            Debug.LogWarning("[Profesor] ¡No tengo waypoints asignados! No me puedo mover.");
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case TeacherState.Patrolling:
                HandlePatrol();
                CheckVision();
                break;
            case TeacherState.Suspicious:
                // Si está sospechando, también debe mirar por si te ve
                CheckVision();
                
                // Si llegó al origen del ruido y no nos pilla, volver a patrullar
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Debug.Log("[Profesor] He llegado a la zona del ruido. No veo nada. Vuelvo a patrullar.");
                    ResumePatrol();
                }
                break;
            case TeacherState.Catching:
                break;
        }
    }

    private void HandlePatrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;
        
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(WaitAtWaypointRoutine());
        }
    }

    private IEnumerator WaitAtWaypointRoutine()
    {
        isWaiting = true;
        Debug.Log($"[Profesor] He llegado al waypoint {currentWaypointIndex}. Esperando {waitTimeAtWaypoint} segundos...");
        
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        Debug.Log($"[Profesor] Retomando patrulla hacia el waypoint {currentWaypointIndex}.");
        isWaiting = false;
    }

    private void CheckVision()
    {
        // Escaneamos un área alrededor del profesor
        Collider[] allColliders = Physics.OverlapSphere(eyesTransform.position, viewRadius);

        // Variable para no saturar la consola: solo avisará si encuentra al jugador
        bool jugadorEnRadio = false;

        for (int i = 0; i < allColliders.Length; i++)
        {
            Transform targetTransform = allColliders[i].transform;
            
            // Buscamos si este objeto es el jugador (o un hijo/padre del jugador)
            PlayerInteraction player = targetTransform.GetComponentInParent<PlayerInteraction>();
            if (player == null) player = targetTransform.GetComponentInChildren<PlayerInteraction>();

            if (player != null)
            {
                jugadorEnRadio = true;
                Vector3 directionToTarget = (targetTransform.position - eyesTransform.position).normalized;
                float angle = Vector3.Angle(eyesTransform.forward, directionToTarget);
                float distance = Vector3.Distance(eyesTransform.position, targetTransform.position);

                Debug.Log($"[Visión] JUGADOR DETECTADO. Distancia: {distance}m. Ángulo: {angle}° (Necesita ser menor de {viewAngle / 2f}°).");

                // 1. Comprobamos si está dentro del cono frontal
                if (angle < viewAngle / 2f)
                {
                    // 2. Trazamos un rayo para ver si hay una pared en medio
                    RaycastHit hit;
                    bool blocked = Physics.Raycast(eyesTransform.position, directionToTarget, out hit, distance, obstacleMask);

                    if (blocked)
                    {
                        Debug.LogWarning($"[Visión] NO TE VEO. Mi vista choca contra el obstáculo: {hit.collider.gameObject.name}. (¡Revisa que este objeto no sea el propio jugador o la cámara!)");
                    }
                    else
                    {
                        Debug.Log($"[Visión] TE ESTOY VIENDO DIRECTAMENTE. ¿Estás haciendo algo malo? IsFrozen = {player.IsFrozen}");

                        // 3. ¡Si te está viendo y estás congelado (haciendo gamberrada), te pilla!
                        if (player.IsFrozen)
                        {
                            Debug.LogError("[¡PILLADO!] El profesor te ha visto cometiendo una gamberrada.");
                            
                            player.ClearHand();
                            player.SetFrozen(false);
                            
                            StudentIdentity identity = player.GetComponentInParent<StudentIdentity>();
                            if (identity != null)
                            {
                                CatchStudent(identity);
                            }
                            else
                            {
                                // Castigo por defecto si el jugador no tiene StudentIdentity
                                ApplyDirectPunishment();
                            }
                            return; // Terminamos aquí porque ya te ha pillado
                        }
                    }
                }
                else
                {
                    Debug.Log("[Visión] El jugador está cerca, pero está a mi espalda o fuera de mi cono de visión.");
                }
                
                // Rompemos el bucle porque ya hemos analizado al jugador
                break; 
            }
        }
    }

    private void ApplyDirectPunishment()
    {
        currentState = TeacherState.Catching;
        agent.isStopped = true;
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(true);
        
        if (GameManager.Instance != null) GameManager.Instance.AddPunishmentPoints(25);
        if (PenaltyPopup.Instance != null) PenaltyPopup.Instance.Show(25);
        
        StartCoroutine(ResumeAfterDelayRoutine(1.5f));
    }

    private IEnumerator ResumeAfterDelayRoutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResumePatrol();
    }

    public void HearNoise(Vector3 noisePosition)
    {
        if (currentState == TeacherState.Catching) return;
        
        Debug.Log($"[Profesor] ¡He escuchado un ruido en la posición {noisePosition}! Voy a investigar.");
        currentState = TeacherState.Suspicious;
        
        questionMarkIcon.SetActive(true);
        exclamationMarkIcon.SetActive(false);
        
        agent.SetDestination(noisePosition);
        isWaiting = false;
    }

    public void CatchStudent(StudentIdentity guiltyStudent)
    {
        currentState = TeacherState.Catching;
        agent.isStopped = true;
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(true);

        Debug.Log($"[Profesor] Aplicando castigo al estudiante: {guiltyStudent.gameObject.name}");
        GetComponent<TeacherPunishment>().PunishStudent(guiltyStudent, 25);
    }

    public void ResumePatrol()
    {
        Debug.Log("[Profesor] Todo en orden. Vuelvo a patrullar tranquilamente.");
        currentState = TeacherState.Patrolling;
        isWaiting = false;
        agent.isStopped = false; 
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(false);
        
        // Retomar camino al waypoint actual
        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}