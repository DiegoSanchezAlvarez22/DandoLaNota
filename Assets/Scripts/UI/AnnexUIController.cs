using UnityEngine;
using System.Collections.Generic;

public class AnnexUIController : MonoBehaviour
{
    [Header("Referencias de la Cuadrícula")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject itemCardPrefab;

    [Header("Referencia al Reproductor de Vídeo")]
    [SerializeField] private AnnexVideoPlayer videoPlayerController;

    private void OnEnable()
    {
        // Cada vez que el jugador abre la pestaña del Anexo, se recargan las tarjetas
        PopulateGrid();
    }

    /// <summary>
    /// Genera la cuadrícula dinámica basándose en la base de datos de AchievementManager.
    /// </summary>
    private void PopulateGrid()
    {
        // Comprobación de seguridad
        if (AchievementManager.Instance == null) return;
        if (gridParent == null || itemCardPrefab == null) return;

        // Limpiamos las tarjetas anteriores para no duplicarlas
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Pedimos todos los datos posibles
        List<GamberradaData> allPranks = AchievementManager.Instance.GetAllPranks();

        foreach (GamberradaData prank in allPranks)
        {
            // Creamos una nueva tarjeta en la cuadrícula
            GameObject cardGo = Instantiate(itemCardPrefab, gridParent);
            AnnexItemCardUI cardUI = cardGo.GetComponent<AnnexItemCardUI>();

            if (cardUI != null)
            {
                // Consultamos si el jugador la tiene desbloqueada
                bool isUnlocked = AchievementManager.Instance.IsPrankUnlocked(prank.id);
                cardUI.Setup(prank, isUnlocked, this);
            }
        }
    }

    /// <summary>
    /// Función puente llamada por una tarjeta para abrir el vídeo.
    /// </summary>
    public void OpenVideoPopup(GamberradaData data)
    {
        if (videoPlayerController != null)
        {
            videoPlayerController.PlayVideo(data);
        }
    }
}