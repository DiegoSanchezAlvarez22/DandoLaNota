using UnityEngine;
using UnityEngine.Video; // Necesario para el sistema de vídeo de Unity
using TMPro;

public class AnnexVideoPlayer : MonoBehaviour
{
    [Header("Elementos del Panel")]
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private VideoPlayer videoPlayer; // Componente nativo de Unity

    private void Start()
    {
        // Asegurarse de que el reproductor está cerrado al iniciar
        CloseVideo();
    }

    /// <summary>
    /// Abre el panel emergente y reproduce el vídeo asignado.
    /// </summary>
    public void PlayVideo(GamberradaData data)
    {
        if (videoPanel != null) videoPanel.SetActive(true);
        if (titleText != null) titleText.text = data.prankName;

        if (videoPlayer != null && data.demoVideo != null)
        {
            videoPlayer.clip = data.demoVideo;
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// Detiene el vídeo y cierra el panel emergente.
    /// Invocado por un botón "Cerrar" (X) en el panel de vídeo.
    /// </summary>
    public void CloseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        if (videoPanel != null) videoPanel.SetActive(false);
    }
}