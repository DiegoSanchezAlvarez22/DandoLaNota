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

        StartCoroutine(ScoldRoutine(student));
    }

    private IEnumerator ScoldRoutine(StudentIdentity student)
    {
        // El profesor camina hacia ti
        agent.isStopped = false;
        agent.SetDestination(student.transform.position);
        if (animator != null) animator.SetBool("IsWalking", true);

        // Espera a llegar físicamente
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Se detiene frente a ti y activa la animación de regañar
        agent.isStopped = true;
        if (animator != null) 
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsScolding", true);
        }

        Vector3 lookPosition = new Vector3(student.transform.position.x, transform.position.y, student.transform.position.z);
        transform.LookAt(lookPosition);

        if (student.GetPunishmentPoints() >= expulsionThreshold)
        {
            yield return new WaitForSeconds(expulsionAnimationDelay);
            
            if (student.IsPlayer())
            {
                if (GameManager.Instance != null) GameManager.Instance.EndMatch(true);
            }
            else
            {
                student.gameObject.SetActive(false);
                teacherAI.ResumePatrol();
            }
        }
        else
        {
            // Ejecuta la animación de enfado exactamente 1.5 segundos
            yield return new WaitForSeconds(1.5f);
            
            // La función ResumePatrol desactivará el estado de bronca internamente
            teacherAI.ResumePatrol();
        }
    }
}