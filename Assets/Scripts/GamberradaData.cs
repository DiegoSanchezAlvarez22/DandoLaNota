using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NuevaGamberrada", menuName = "DandoLaNota/NuevaGamberrada")]
public class GamberradaData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("ID único. Ej: bola_papel, avion_papel")] // sin espacios que da errores
    public string id;
    
    [Tooltip("Nombre visual que aparecerá en el anexo")]
    public string prankName;

    [Header("Contenido Visual")]
    [Tooltip("Icono representativo de la gamberrada para la UI")]
    public Sprite icon;
    
    [Tooltip("Clip de vídeo demostrativo que se reproducirá en el anexo")]
    public VideoClip demoVideo;
}
