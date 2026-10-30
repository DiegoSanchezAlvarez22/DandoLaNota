using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeacherAI))]
public class TeacherPunishment : MonoBehaviour
{
    [Header("Configuración de Expulsión")]
    [SerializeField] private int expulsionThreshold = 100;
    [SerializeField] private float expulsionAnimationDelay = 3f;

    [Header("Configuración de Regañina")]
    [Tooltip("Distancia a la que se detiene el profesor respecto al alumno para que se vea bien la animación")]
    [SerializeField] private float distanciaDetencion = 1.8f;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private TeacherAI teacherAI;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        teacherAI = GetComponent<TeacherAI>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void PunishStudent(StudentIdentity student, int pointsAdded)
    {
        student.AddPunishmentPoints(pointsAdded);

        if (student.IsPlayer() && GameManager.Instance != null)
        {
            GameManager.Instance.AddPunishmentPoints(pointsAdded);
        }

        // Nos aseguramos de que el jugador se quede congelado al ser pillado
        if (student.IsPlayer())
        {
            PlayerInteraction playerInt = student.GetComponentInChildren<PlayerInteraction>();
            if (playerInt == null) playerInt = student.GetComponentInParent<PlayerInteraction>();
            if (playerInt != null) playerInt.SetFrozen(true);
        }

        StartCoroutine(ScoldRoutine(student));
    }

    private IEnumerator ScoldRoutine(StudentIdentity student)
    {
        agent.isStopped = false;
        
        // Configuramos la distancia de parada del agente para que frene antes de chocar contigo
        agent.stoppingDistance = distanciaDetencion;
        agent.SetDestination(student.transform.position);
        if (animator != null) animator.SetBool("IsWalking", true);

        yield return null;

        // Esperamos a que el profesor llegue a esa distancia de seguridad de 1.8 metros
        while (Vector3.Distance(transform.position, student.transform.position) > distanciaDetencion + 0.5f)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) break;
            yield return null;
        }

        // El profesor se detiene y te mira a la cara
        agent.isStopped = true;
        if (animator != null) animator.SetBool("IsWalking", false);
        
        Vector3 lookPosition = new Vector3(student.transform.position.x, transform.position.y, student.transform.position.z);
        transform.LookAt(lookPosition);

        // Comienza la animación de enfado en bucle
        if (animator != null) 
        {
            animator.SetBool("IsScolding", true);
        }

        if (student.GetPunishmentPoints() >= expulsionThreshold)
        {
            yield return new WaitForSeconds(expulsionAnimationDelay);
            
            if (student.IsPlayer() && GameManager.Instance != null)
                GameManager.Instance.EndMatch(true);
            else
            {
                student.gameObject.SetActive(false);
                teacherAI.ResumePatrol();
            }
        }
        else
        {
            // El profesor te echa la bronca durante exactamente 1.5 segundos
            yield return new WaitForSeconds(1.5f);
            
            // DEVOLVER EL MOVIMIENTO: Al acabar la regañina, liberamos al jugador
            if (student.IsPlayer())
            {
                PlayerInteraction playerInt = student.GetComponentInChildren<PlayerInteraction>();
                if (playerInt == null) playerInt = student.GetComponentInParent<PlayerInteraction>();
                if (playerInt != null) playerInt.SetFrozen(false);
            }

            // El profesor se va a seguir patrullando
            teacherAI.ResumePatrol();
        }
    }
}