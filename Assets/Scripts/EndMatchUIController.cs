using UnityEngine;
using TMPro;

public class EndMatchUIController : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    // Referencia al Canvas/Panel unificado de resultados finales
    [SerializeField] private GameObject endMatchCanvas;

    [Header("Textos de Puntuación")]
    // Encabezado principal ("¡EXPULSADO!" o "¡TIEMPO AGOTADO!")
    [SerializeField] private TMP_Text statusText;
    // Texto para los Puntos de Risa acumulados
    [SerializeField] private TMP_Text laughPointsText;
    // Texto para los Puntos de Castigo recibidos
    [SerializeField] private TMP_Text punishmentPointsText;
    // Texto para el resultado final (Risa - Castigo)
    [SerializeField] private TMP_Text finalScoreText;

    private void OnEnable()
    {
        // Suscripción al evento de fin de partida
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMatchEnded += DisplayEndMatchScreen;
        }
    }

    private void OnDisable()
    {
        // Cancelación de la suscripción al apagar el script
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMatchEnded -= DisplayEndMatchScreen;
        }
    }

    private void Start()
    {
        // Ocultación de la pantalla de resultados al iniciar la escena
        if (endMatchCanvas != null)
        {
            endMatchCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Activa el panel de resultados y rellena la información con los datos de la partida.
    /// </summary>
    private void DisplayEndMatchScreen(bool expelled, int laughPoints, int punishmentPoints, int finalScore)
    {
        if (endMatchCanvas != null)
        {
            endMatchCanvas.SetActive(true);
        }

        // Configuración dinámica del título según la causa de fin de partida
        if (statusText != null)
        {
            statusText.text = expelled ? "¡EXPULSADO!" : "¡CLASE TERMINADA!";
        }

        // Muestra de desglose de puntos
        if (laughPointsText != null) laughPointsText.text = $"P. DE RISA: \n{laughPoints}";
        if (punishmentPointsText != null) punishmentPointsText.text = $"P. DE CASTIGO: \n{punishmentPoints}";
        if (finalScoreText != null) finalScoreText.text = $"{finalScore}";
    }

    /// <summary>
    /// Restaura la velocidad del tiempo y regresa al Menú Principal.
    /// Invocado por el botón "Volver al Menú" en la pantalla de resultados.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadMainMenuScene();
        }
        else
        {
            Debug.LogError("SceneTransitionManager no encontrado.");
        }
    }
}