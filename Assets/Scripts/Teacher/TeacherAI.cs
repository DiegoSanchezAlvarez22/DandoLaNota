using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherAI : MonoBehaviour
{
    public enum TeacherState { Patrolling, Suspicious, Catching }

    [Header("Estado Actual")]
    [SerializeField] private TeacherState currentState = TeacherState.Patrolling;

    [Header("Animaciones")]
    // Referencia al componente Animator del profesor
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
    [SerializeField] private LayerMask obstacleMask; // Solo muros, asegúrate de que el jugador NO está en esta capa

    [Header("Indicadores Visuales")]
    [SerializeField] private GameObject questionMarkIcon;
    [SerializeField] private GameObject exclamationMarkIcon;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(false);

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
            if (animator != null) animator.SetBool("IsWalking", true);
            Debug.Log("[Profesor] Iniciando patrulla.");
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
                    Debug.Log("[Profesor] Falsa alarma, vuelvo a patrullar.");
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
        
        // Pasa a animación Idle (parado)
        if (animator != null) animator.SetBool("IsWalking", false); 
        Debug.Log($"[Profesor] Esperando {waitTimeAtWaypoint}s en el waypoint.");
        
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        // Vuelve a animación de andar
        if (animator != null) animator.SetBool("IsWalking", true); 
        isWaiting = false;
    }

    private void CheckVision()
    {
        // Detecta todo en su radio, sin importar la Layer
        Collider[] allColliders = Physics.OverlapSphere(eyesTransform.position, viewRadius); 

        for (int i = 0; i < allColliders.Length; i++)
        {
            Transform targetTransform = allColliders[i].transform;
            
            PlayerInteraction player = targetTransform.GetComponentInParent<PlayerInteraction>();
            if (player == null) player = targetTransform.GetComponentInChildren<PlayerInteraction>();

            if (player != null)
            {
                Vector3 directionToTarget = (targetTransform.position - eyesTransform.position).normalized;
                float angle = Vector3.Angle(eyesTransform.forward, directionToTarget);
                float distance = Vector3.Distance(eyesTransform.position, targetTransform.position);

                if (angle < viewAngle / 2f)
                {
                    if (!Physics.Raycast(eyesTransform.position, directionToTarget, distance, obstacleMask))
                    {
                        Debug.Log($"[Visión] Te veo directamente. IsFrozen: {player.IsFrozen}");
                        if (player.IsFrozen)
                        {
                            Debug.LogError("[¡PILLADO!] Gamberrada detectada a plena vista.");
                            player.ClearHand();
                            player.SetFrozen(false);
                            
                            StudentIdentity identity = player.GetComponentInParent<StudentIdentity>();
                            if (identity != null) CatchStudent(identity);
                            return; 
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Visión] Hay una pared/obstáculo entre nosotros.");
                    }
                }
                break; // Solo comprobamos al jugador una vez por frame
            }
        }
    }

    public void HearNoise(Vector3 noisePosition)
    {
        if (currentState == TeacherState.Catching) return;
        
        currentState = TeacherState.Suspicious;
        questionMarkIcon.SetActive(true);
        exclamationMarkIcon.SetActive(false);
        
        agent.SetDestination(noisePosition);
        if (animator != null) animator.SetBool("IsWalking", true);
        isWaiting = false;
    }

    public void CatchStudent(StudentIdentity guiltyStudent)
    {
        currentState = TeacherState.Catching;
        agent.isStopped = true;
        
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(true);

        // Llamamos al castigo y le pasamos el control al TeacherPunishment
        GetComponent<TeacherPunishment>().PunishStudent(guiltyStudent, 25);
    }

    public void ResumePatrol()
    {
        currentState = TeacherState.Patrolling;
        isWaiting = false;
        agent.isStopped = false;
        
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(false);
        
        if (animator != null)
        {
            animator.SetBool("IsScolding", false);
            animator.SetBool("IsWalking", true);
        }

        if (waypoints.Length > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}