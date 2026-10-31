using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherAI : MonoBehaviour
{
    // AÑADIDO: Nuevo estado 'Tripped' para cuando se cae
    public enum TeacherState { Patrolling, Suspicious, Catching, Tripped }

    [Header("Estado Actual")]
    [SerializeField] private TeacherState currentState = TeacherState.Patrolling;

    [Header("Animaciones")]
    [SerializeField] private Animator animator; 

    [Header("Configuración de Patrulla")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTimeAtWaypoint = 2f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    [Header("Configuración de Visión")]
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private Transform eyesTransform;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Configuración de Caída (Zancadilla)")]
    [Tooltip("Tiempo total que duran las animaciones de caerse y levantarse")]
    [SerializeField] private float tiempoRecuperacionCaida = 4f;

    [Header("Indicadores Visuales")]
    //[SerializeField] private GameObject questionMarkIcon;
    //[SerializeField] private GameObject exclamationMarkIcon;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        //questionMarkIcon.SetActive(false);
        //exclamationMarkIcon.SetActive(false);

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
            if (animator != null) animator.SetBool("IsWalking", true);
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
                CheckVision();
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    ResumePatrol();
                }
                break;
            case TeacherState.Catching:
                break;
            case TeacherState.Tripped:
                // Mientras está en el suelo no hace absolutamente nada
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
        if (animator != null) animator.SetBool("IsWalking", false);
        transform.Rotate(0f, 180f, 0f);

        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        if (animator != null) animator.SetBool("IsWalking", true); 
        isWaiting = false;
    }

    private void CheckVision()
    {
        Collider[] allColliders = Physics.OverlapSphere(eyesTransform.position, viewRadius); 
        bool jugadorAnalizado = false;

        for (int i = 0; i < allColliders.Length; i++)
        {
            Transform targetTransform = allColliders[i].transform;
            
            PlayerInteraction playerDetectado = targetTransform.GetComponentInParent<PlayerInteraction>();
            if (playerDetectado == null) playerDetectado = targetTransform.GetComponentInChildren<PlayerInteraction>();

            if (playerDetectado != null && !jugadorAnalizado)
            {
                jugadorAnalizado = true; 

                Vector3 puntoMiraJugador = playerDetectado.transform.position + (Vector3.up * 1.2f); 
                Vector3 directionToTarget = (puntoMiraJugador - eyesTransform.position).normalized;
                
                float angle = Vector3.Angle(eyesTransform.forward, directionToTarget);
                float distance = Vector3.Distance(eyesTransform.position, puntoMiraJugador);

                // === SOLUCIÓN: Detección por cono visual O por contacto/proximidad ===
                bool estaEnCono = angle < viewAngle / 2f;
                bool estaMuyCerca = distance <= 1.5f; // A metro y medio te pilla aunque no te mire directamente

                if (estaEnCono || estaMuyCerca)
                {
                    if (!Physics.Raycast(eyesTransform.position, directionToTarget, distance, obstacleMask))
                    {
                        if (playerDetectado.IsFrozen)
                        {
                            Debug.LogError("[¡PILLADO!] Gamberrada detectada a plena vista o por proximidad.");
                            
                            PrankItem[] items = FindObjectsOfType<PrankItem>();
                            foreach (var item in items) item.StopAllCoroutines();
                            
                            EnvironmentPrank[] entornos = FindObjectsOfType<EnvironmentPrank>();
                            foreach (var entorno in entornos) entorno.StopAllCoroutines();

                            PrankItem objetoEnMano = playerDetectado.GetComponentInChildren<PrankItem>();
                            if (objetoEnMano != null) objetoEnMano.Drop(Vector3.zero);

                            StudentIdentity identity = playerDetectado.GetComponentInParent<StudentIdentity>();
                            if (identity != null) CatchStudent(identity, playerDetectado);
                            
                            playerDetectado.ClearHand();
                            playerDetectado.UpdateActionTimer(0);
                            
                            return; 
                        }
                    }
                }
            }
        }
    }

    public void HearNoise(Vector3 noisePosition)
    {
        // Si está regañando o cayéndose, ignora el ruido
        if (currentState == TeacherState.Catching || currentState == TeacherState.Tripped) return;
        
        currentState = TeacherState.Suspicious;
        //questionMarkIcon.SetActive(true);
        //exclamationMarkIcon.SetActive(false);
        
        agent.SetDestination(noisePosition);
        if (animator != null) animator.SetBool("IsWalking", true);
        isWaiting = false;
    }

    public void CatchStudent(StudentIdentity guiltyStudent, PlayerInteraction playerInvolucrado = null)
    {
        currentState = TeacherState.Catching;
        agent.isStopped = true;
        
        //questionMarkIcon.SetActive(false);
        //exclamationMarkIcon.SetActive(true);

        // Obtenemos los puntos directamente del ScriptableObject de la jugarreta actual, o 25 por defecto
        int puntosDeLaGamberrada = 25;
        if (playerInvolucrado != null && playerInvolucrado.CurrentPrankData != null)
        {
            puntosDeLaGamberrada = playerInvolucrado.CurrentPrankData.punishmentPoints;
        }

        GetComponent<TeacherPunishment>().PunishStudent(guiltyStudent, puntosDeLaGamberrada);
    }

    public void TripOver()
    {
        // Si ya está cayéndose o regañando, no se tropieza otra vez
        if (currentState == TeacherState.Catching || currentState == TeacherState.Tripped) return;

        Debug.Log("[Profesor] ¡Me han puesto la zancadilla!");
        
        currentState = TeacherState.Tripped;
        agent.isStopped = true; // Se detiene en seco para no resbalar por el suelo
        
        //questionMarkIcon.SetActive(false);
        //exclamationMarkIcon.SetActive(false);

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            // Usaremos un "Trigger" para disparar la caída al instante
            animator.SetTrigger("Trip"); 
        }

        StartCoroutine(RecoverFromTripRoutine());
    }

    private IEnumerator RecoverFromTripRoutine()
    {
        // Esperamos a que termine la animación de caer y levantarse
        yield return new WaitForSeconds(tiempoRecuperacionCaida);
        
        Debug.Log("[Profesor] Me he levantado. Sigo patrullando.");
        ResumePatrol();
    }

public void ResumePatrol()
    {
        currentState = TeacherState.Patrolling;
        isWaiting = false;
        agent.isStopped = false; 
        
        //questionMarkIcon.SetActive(false);
        //exclamationMarkIcon.SetActive(false);
        
        if (animator != null)
        {
            animator.SetBool("IsScolding", false);
            animator.SetBool("IsWalking", true);
        }

        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}