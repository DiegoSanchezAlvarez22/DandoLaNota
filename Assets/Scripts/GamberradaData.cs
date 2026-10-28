using UnityEngine;
using UnityEngine.Video;

public enum MinigameType
{
    SpamBoton,
    BarraRitmo,
    SecuenciaTeclas
}

[CreateAssetMenu(fileName = "NuevaGamberrada", menuName = "DandoLaNota/GamberradaData")]
public class GamberradaData : ScriptableObject
{
    [Header("Identificación")]
    public string id;
    public string prankName;

    [Header("Mecánica")]
    public MinigameType minigameType;

    [Header("Contenido Visual")]
    public Sprite icon;
    public VideoClip demoVideo;
}