using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeacherAI))]
public class TeacherPunishment : MonoBehaviour
{
    [Header("Configuración de Expulsión")]
    // Límite de puntos para que el profesor expulse al alumno
    [SerializeField] private int expulsionThreshold = 100;

    // Tiempo en segundos que el profesor se queda mirando/regañando al alumno frente a su pupitre
    [SerializeField] private float expulsionAnimationDelay = 3f;

    // Componentes internos
    private NavMeshAgent agent;
    private TeacherAI teacherAI;

    private IEnumerator ResumeAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        teacherAI.ResumePatrol();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        teacherAI = GetComponent<TeacherAI>();
    }

    /// <summary>
    /// Evalúa el castigo de un alumno y determina si procede expulsarlo.
    /// Invocado por TeacherAI cuando pilla a un alumno.
    /// </summary>
    /// <param name="student">La identidad del estudiante pillado</param>
    /// <param name="pointsAdded">La cantidad de puntos de castigo por la gamberrada</param>
    /// 

    public void PunishStudent(StudentIdentity student, int pointsAdded)
    {
        student.AddPunishmentPoints(pointsAdded);

        // Si es el jugador, se registran los puntos en el GameManager
        if (student.IsPlayer())
        {
            GameManager.Instance.AddPunishmentPoints(pointsAdded);
        }

        // Comprobación de si supera el límite de expulsión
        if (student.GetPunishmentPoints() >= expulsionThreshold)
        {
            StartCoroutine(ExpulsionRoutine(student));
        }
        else
        {
            StartCoroutine(ResumeAfterDelay(1.5f));
        }
    }

    /// <summary>
    /// Corrutina que gestiona el acercamiento y la pausa de mirada frente al alumno.
    /// </summary>
    private IEnumerator ExpulsionRoutine(StudentIdentity student)
    {
        // Desplazamiento del profesor hacia la posición exacta del pupitre del alumno
        agent.isStopped = false;
        agent.SetDestination(student.transform.position);

        // Espera hasta que el profesor llegue físicamente al pupitre
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // TODO: A FUTURO - Sustituir este tiempo de espera por la activación de la animación de expulsión //
        // donde el profesor agarra al alumno y lo saca físicamente del aula                               //
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        // Pausa serializable (el profesor se queda parado mirando al alumno)
        yield return new WaitForSeconds(expulsionAnimationDelay);

        // Resolución del evento
        if (student.IsPlayer())
        {
            // Delegación directa al GameManager para finalizar la partida de forma centralizada
            GameManager.Instance.EndMatch(true);
        }
        else
        {
            // Si es un NPC, se deshabilita del aula y el profesor vuelve a patrullar
            student.gameObject.SetActive(false);
            teacherAI.ResumePatrol();
        }
    }
}