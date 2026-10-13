using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class TeacherAI : MonoBehaviour
{
    // Definición de los posibles estados mentales del profesor
    public enum TeacherState { Patrolling, Suspicious, Catching }

    [Header("Estado Actual")]
    // Estado en el que se encuentra el profesor en este momento
    [SerializeField] private TeacherState currentState = TeacherState.Patrolling;

    [Header("Configuración de Patrulla")]
    // Lista de puntos de ruta (Waypoints) por los que el profesor caminará[cite: 3]
    [SerializeField] private Transform[] waypoints;
    // Tiempo, en segundos, que el profesor espera en cada waypoint antes de continuar
    [SerializeField] private float waitTimeAtWaypoint = 2f; 
    // Índice del punto de ruta actual hacia el que se dirige
    private int currentWaypointIndex = 0;
    // Indicador interno para saber si está esperando en un punto de ruta
    private bool isWaiting = false;

    [Header("Configuración de Visión")]
    // Distancia máxima a la que el profesor puede ver
    [SerializeField] private float viewRadius = 10f;
    // Ángulo de apertura del cono de visión (en grados)
    [SerializeField] private float viewAngle = 90f;
    // Punto de origen desde donde salen los rayos visuales (normalmente la cabeza)
    [SerializeField] private Transform eyesTransform;
    // Capa que identifica a los alumnos
    [SerializeField] private LayerMask studentMask;
    // Capa que identifica las paredes o pupitres que bloquean la visión
    [SerializeField] private LayerMask obstacleMask;

    [Header("Indicadores Visuales")]
    // Objeto 3D o UI (interrogación) que aparece al escuchar un ruido extraño[cite: 15]
    [SerializeField] private GameObject questionMarkIcon;
    // Objeto 3D o UI (exclamación) que aparece al pillar a un alumno[cite: 15]
    [SerializeField] private GameObject exclamationMarkIcon;

    // Componente interno para la navegación automática
    private NavMeshAgent agent;

    private void Awake()
    {
        // Inicialización del componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Ocultación de los iconos de alerta al iniciar
        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(false);

        // Envío del profesor al primer punto de ruta si existen
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    private void Update()
    {
        // Máquina de estados: el comportamiento cambia según el estado actual
        switch (currentState)
        {
            case TeacherState.Patrolling:
                HandlePatrol();
                CheckVision();
                break;
            case TeacherState.Suspicious:
                CheckVision();
                // Si llega al origen del ruido y no ve nada, volverá a patrullar tras un tiempo (se implementará con una corrutina)
                break;
            case TeacherState.Catching:
                // El profesor ha pillado a alguien; detiene su movimiento para regañar
                break;
        }
    }

    /// <summary>
    /// Gestiona el movimiento entre los diferentes puntos de ruta.
    /// </summary>
    private void HandlePatrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        // Comprobación de si ha llegado a su destino actual (con un pequeño margen de error)
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(WaitAtWaypointRoutine());
        }
    }

    /// <summary>
    /// Rutina de espera al llegar a un punto de ruta usando el tiempo serializado.
    /// </summary>
    private IEnumerator WaitAtWaypointRoutine()
    {
        isWaiting = true;
        
        // Espera el tiempo configurado en waitTimeAtWaypoint
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        // Avance al siguiente punto de ruta (vuelve al 0 si llega al final)
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        isWaiting = false;
    }

    /// <summary>
    /// Escanea el área en busca de alumnos realizando gamberradas dentro del cono de visión.
    /// </summary>
    private void CheckVision()
    {
        // Detección de todos los alumnos dentro del radio máximo
        Collider[] studentsInRadius = Physics.OverlapSphere(eyesTransform.position, viewRadius, studentMask);

        for (int i = 0; i < studentsInRadius.Length; i++)
        {
            Transform studentTransform = studentsInRadius[i].transform;
            Vector3 directionToStudent = (studentTransform.position - eyesTransform.position).normalized;

            // Comprobación de si el alumno está dentro del ángulo de visión frontal
            if (Vector3.Angle(eyesTransform.forward, directionToStudent) < viewAngle / 2f)
            {
                float distanceToStudent = Vector3.Distance(eyesTransform.position, studentTransform.position);

                // Lanzamiento de un rayo (Raycast) para comprobar que no hay paredes entre los ojos y el alumno
                if (!Physics.Raycast(eyesTransform.position, directionToStudent, distanceToStudent, obstacleMask))
                {
                    // ¡Visión directa establecida! Aquí se comprobaría si el alumno está haciendo una gamberrada.
                    // Para la base, simularemos que lo detecta.
                    
                    // StudentIdentity detectedStudent = studentTransform.GetComponent<StudentIdentity>();
                    // if (detectedStudent != null && alumno_haciendo_gamberrada)
                    // {
                    //      CatchStudent(detectedStudent);
                    // }
                }
            }
        }
    }

    /// <summary>
    /// Método público invocado por el gestor de sonido cuando se detecta un ruido.
    /// Envía al profesor a investigar la posición de origen[cite: 11, 13, 15].
    /// </summary>
    /// <param name="noisePosition">Vector3 con las coordenadas del sonido</param>
    public void HearNoise(Vector3 noisePosition)
    {
        if (currentState == TeacherState.Catching) return; // Si ya está regañando, ignora el sonido

        currentState = TeacherState.Suspicious;
        
        // Activación de la exclamación/interrogación animada y mirada de búsqueda[cite: 15]
        questionMarkIcon.SetActive(true);
        exclamationMarkIcon.SetActive(false);

        // Envío del agente a la posición del ruido
        agent.SetDestination(noisePosition);
        isWaiting = false;

        // Opcional: Rotar la cabeza hacia el ruido[cite: 15]
    }

    /// <summary>
    /// Método para procesar cuando se pilla a un alumno con las manos en la masa.
    /// </summary>
    /// <param name="guiltyStudent">Componente StudentIdentity del culpable</param>
    public void CatchStudent(StudentIdentity guiltyStudent)
    {
        currentState = TeacherState.Catching;
        agent.isStopped = true; // Detención del movimiento

        questionMarkIcon.SetActive(false);
        exclamationMarkIcon.SetActive(true); // Se enfada al pillarle[cite: 15]
        
        Debug.Log("¡El profesor ha pillado al alumno " + guiltyStudent.GetDeskID() + "!");
        
        // Aquí se conectará con el script TeacherPunishment.cs para aplicar los puntos.
    }
}