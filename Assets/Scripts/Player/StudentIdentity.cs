using UnityEngine;
using System;

public class StudentIdentity : MonoBehaviour
{
    [Header("ID Alumno")]
    // Determina si este componente pertenece al jugador humano o a un NPC controlado por la IA
    [SerializeField] private bool isPlayer = false;
    
    // Número identificador del pupitre asignado a este alumno
    [SerializeField] private int deskID = 0;

    [Header("Registro de Disciplina")]
    // Acumulador de los puntos de castigo recibidos tras ser pillado por el profesor[cite: 2]
    [SerializeField] private int punishmentPoints = 0;

    // Evento que se dispara cada vez que cambian los puntos de castigo, útil para actualizar la UI del jugador
    public event Action<int> OnPunishmentPointsChanged;

    /// <summary>
    /// Añade puntos de castigo al expediente del alumno.
    /// Invocado por el script del profesor (TeacherPunishment) cuando detecta una gamberrada.
    /// </summary>
    /// <param name="points">Cantidad de puntos de enfado/castigo a sumar</param>
    public void AddPunishmentPoints(int points)
    {
        punishmentPoints += points;
        
        // Notificación a los componentes suscritos (como el GameHUDController) sobre el nuevo valor
        OnPunishmentPointsChanged?.Invoke(punishmentPoints);

        // Registro en consola para facilitar la depuración en el editor
        Debug.Log($"Alumno en pupitre {deskID} (Jugador: {isPlayer}) recibe {points} puntos de castigo. Total: {punishmentPoints}");
    }

    /// <summary>
    /// Devuelve la cantidad actual de puntos de castigo.
    /// Útil para que el GameManager compruebe si se ha superado el límite de expulsión.
    /// </summary>
    /// <returns>Valor entero con los puntos actuales</returns>
    public int GetPunishmentPoints()
    {
        return punishmentPoints;
    }

    /// <summary>
    /// Indica si este estudiante representa al jugador principal.
    /// </summary>
    /// <returns>Verdadero si es el jugador, falso si es un NPC</returns>
    public bool IsPlayer()
    {
        return isPlayer;
    }

    /// <summary>
    /// Devuelve el identificador numérico del pupitre asignado.
    /// Útil para mecánicas como la selección aleatoria de asiento al inicio del nivel[cite: 5].
    /// </summary>
    /// <returns>Número del pupitre</returns>
    public int GetDeskID()
    {
        return deskID;
    }
    
    /// <summary>
    /// Asigna un nuevo número de pupitre dinámicamente al inicio del nivel.
    /// </summary>
    /// <param name="newDeskID">El identificador numérico del nuevo asiento</param>
    public void SetDeskID(int newDeskID)
    {
        deskID = newDeskID;
    }
}