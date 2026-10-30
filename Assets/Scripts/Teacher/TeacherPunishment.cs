using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeacherAI))]
public class TeacherPunishment : MonoBehaviour
{
    [Header("Configuración de Expulsión")]
    // Límite de puntos para expulsar
    [SerializeField] private int expulsionThreshold = 100;
    // Tiempo extra de suspense si te expulsa
    [SerializeField] private float expulsionAnimationDelay = 3f;

    private NavMeshAgent agent;
    private TeacherAI teacherAI;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        teacherAI = GetComponent<TeacherAI>();
    }

    public void PunishStudent(StudentIdentity student, int pointsAdded)
    {
        // Sumamos los puntos al momento
        student.AddPunishmentPoints(pointsAdded);

        if (student.IsPlayer() && GameManager.Instance != null)
        {
            GameManager.Instance.AddPunishmentPoints(pointsAdded);
        }

        // Iniciamos la rutina para que el profesor camine hacia el culpable
        StartCoroutine(ScoldRoutine(student));
    }

    private IEnumerator ScoldRoutine(StudentIdentity student)
    {
        // Reactivamos el movimiento del agente y le decimos que vaya a la posición exacta del alumno
        agent.isStopped = false;
        agent.SetDestination(student.transform.position);

        // Esperamos hasta que el profesor llegue a tu lado físicamente
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // Obligamos al profesor a girarse para mirarte a la cara
        Vector3 lookPosition = new Vector3(student.transform.position.x, transform.position.y, student.transform.position.z);
        transform.LookAt(lookPosition);

        // Comprobamos si superas el límite de expulsión
        if (student.GetPunishmentPoints() >= expulsionThreshold)
        {
            // Drama extra si te expulsa[cite: 2]
            yield return new WaitForSeconds(expulsionAnimationDelay);
            if (student.IsPlayer())
            {
                // Game Over: activa el panel de fin de partida
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
            // Te mira fijamente exactamente 1.5 segundos y se va
            yield return new WaitForSeconds(1.5f);
            teacherAI.ResumePatrol();
        }
    }
}