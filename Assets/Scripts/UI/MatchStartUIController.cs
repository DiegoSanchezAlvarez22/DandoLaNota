using UnityEngine;
using TMPro;

public class MatchStartUIController : MonoBehaviour
{
    // === SEMÁFORO GLOBAL PARA EL RATÓN ===
    // Avisa a otros scripts (como PlayerController) si hay un menú activo
    public static bool isMenuOpen = false;

    [Header("Interfaz de Inicio")]
    // Referencia al panel oscuro que bloquea la pantalla al inicio
    [SerializeField] private GameObject startPanel;
    
    // Campo donde el jugador escribe su nombre
    [SerializeField] private TMP_InputField nameInputField;

    private void Start()
    {
        // Activamos el semáforo para que el jugador sepa que no debe bloquear el ratón
        isMenuOpen = true;

        // Se asegura de que el panel se muestre nada más cargar la escena
        if (startPanel != null) startPanel.SetActive(true);

        // Si el jugador ya guardó su nombre en una partida anterior, se carga por comodidad
        if (PlayerPrefs.HasKey("PlayerName") && nameInputField != null)
        {
            nameInputField.text = PlayerPrefs.GetString("PlayerName");
        }

        // Muestra y libera el cursor para poder usar los menús con libertad
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Guarda el nombre, lo envía al servidor y da comienzo a la partida.
    /// Invocado por el botón "Empezar Clase" en la interfaz.
    /// </summary>
    public void ConfirmNameAndStart()
    {
        // Validación para evitar que el jugador intente entrar con el nombre vacío
        if (nameInputField == null || string.IsNullOrEmpty(nameInputField.text)) return;

        string playerName = nameInputField.text;

        // Llamamos a la nueva función que crea la cuenta distinta para cada nombre
        LootLockerManager.Instance.RegisterPlayerForMatch(playerName, (success) => 
        {
            // Guardado local
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            // Ocultar panel de inicio
            if (startPanel != null) startPanel.SetActive(false);

            // Reanudar tiempo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BeginMatchTimer();
            }

            // Apagamos el semáforo para avisar de que ya no hay menús estorbando
            isMenuOpen = false;

            // Devolvemos el control de la cámara al jugador y ocultamos el ratón
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        });
    }
}