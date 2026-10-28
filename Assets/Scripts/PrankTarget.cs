using UnityEngine;
using System.Collections.Generic;

public enum TargetType { Superficie, Alumno, Profesor }

public class PrankTarget : MonoBehaviour
{
    public TargetType tipo;
    public string nombreObjetivo = "Pared"; 

    // Lista global para encontrar a los alumnos al instante sin ralentizar el juego
    public static List<PrankTarget> todosLosObjetivos = new List<PrankTarget>();

    private void OnEnable()
    {
        todosLosObjetivos.Add(this);
    }

    private void OnDisable()
    {
        todosLosObjetivos.Remove(this);
    }
}