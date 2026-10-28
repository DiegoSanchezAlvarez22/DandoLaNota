using UnityEngine;
using TMPro;

public class MatchStartUIController : MonoBehaviour
{
    [Header("Interfaz de Inicio")]
    // Referencia al panel oscuro que bloquea la pantalla al inicio
    [SerializeField] private GameObject startPanel;
    
    // Campo donde el jugador escribe su nombre
    [SerializeField] private TMP_InputField nameInputField;

    private void Start()
    {
        // Se asegura de que el panel se muestre nada más cargar la escena
        if (startPanel != null) startPanel.SetActive(true);

        // Si el jugador ya guardó su nombre en una partida anterior, se carga por comodidad
        if (PlayerPrefs.HasKey("PlayerName") && nameInputField != null)
        {
            nameInputField.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    /// <summary>
    /// Guarda el nombre, lo envía al servidor y da comienzo a la partida
    /// Invocado por el botón "Empezar Clase" en la interfaz
    /// </summary>
    public void ConfirmNameAndStart()
    {
        if (nameInputField == null || string.IsNullOrEmpty(nameInputField.text)) return;

        string playerName = nameInputField.text;

        // Llamamos a la nueva función que crea la cuenta distinta para cada nombre
        LootLockerManager.Instance.RegisterPlayerForMatch(playerName, (success) => 
        {
            // Guardado local
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            // Ocultar panel
            if (startPanel != null) startPanel.SetActive(false);
            if (startPanel != null) startPanel.SetActive(false);
            if (startPanel != null) startPanel.SetActive(false);

            // Reanudar tiempo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BeginMatchTimer();
            }
        });
    }
}