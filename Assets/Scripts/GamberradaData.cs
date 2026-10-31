using UnityEngine;
using UnityEngine.Video;

public enum MinigameType { SpamBoton, BarraRitmo, SecuenciaTeclas }
public enum Dificultad { Facil, Medio, Dificil }
public enum TipoPenalizacion { AumentarTiempo, HacerRuido }

[CreateAssetMenu(fileName = "NuevaGamberrada", menuName = "DandoLaNota/GamberradaData")]
public class GamberradaData : ScriptableObject
{
    [Header("Identificación")]
    public string id;
    public string prankName;

    [Header("Mecánica del Minijuego")]
    public MinigameType minigameType;
    public Dificultad dificultad;

    [Header("Tiempos y Penalización")]
    [Tooltip("Tiempo en segundos que el jugador debe estar quieto para completar la acción")]
    public float tiempoEjecucionBase = 2f;
    [Tooltip("Puntos de castigo que se suman si el profesor te pilla haciendo esta gamberrada")]
public int punishmentPoints = 25; // Valor por defecto
    public TipoPenalizacion penalizacionPorFallo;
    [Tooltip("Segundos extra que tardarás si fallas (Solo para AumentarTiempo)")]
    public float tiempoPenalizacion = 2f;
    [Tooltip("Ruido que se reproducirá al fallar (Solo para HacerRuido)")]
    public AudioClip sonidoPenalizacion;

    [Header("Contenido Visual")]
    public Sprite icon;
    public VideoClip demoVideo;
}