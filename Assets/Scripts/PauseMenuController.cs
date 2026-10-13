using UnityEngine;
using UnityEngine.InputSystem; // Añadido para el nuevo Input System

public class PauseMenuController : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    // Referencia al panel principal de pausa
    [SerializeField] private GameObject pausePanel;
    // Referencia al panel de opciones/ajustes dentro del juego
    [SerializeField] private GameObject settingsPanel;

    [Header("Controles (New Input System)")]
    // Referencia a la acción de pausar (ej. tecla Escape)
    [SerializeField] private InputActionReference pauseAction;

    [Header("Estado interno")]
    // Indica si el juego se encuentra pausado en este momento
    [SerializeField] private bool isPaused = false;

    private void OnEnable()
    {
        // Activación de la acción al encender el script
        if (pauseAction != null) pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        // Desactivación de la acción al apagar el script
        if (pauseAction != null) pauseAction.action.Disable();
    }

    private void Start()
    {
        // Se asegura de que la interfaz de pausa esté oculta al iniciar la clase
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Detección de la acción de pausa en el frame actual[cite: 2]
        if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Congela el tiempo del juego, muestra el menú y desbloquea el cursor.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        
        // Detiene el tiempo (afecta al temporizador de la partida y a las físicas)[cite: 2]
        Time.timeScale = 0f;

        // Desbloqueo del cursor para poder hacer clic en los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Oculta el menú, reanuda el tiempo y vuelve a bloquear el cursor para jugar.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Restauración del flujo normal del tiempo
        Time.timeScale = 1f;

        // Bloqueo del cursor de nuevo para la cámara en primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Abre el panel de ajustes ocultando los botones principales de pausa.
    /// </summary>
    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Cierra el panel de ajustes y vuelve a mostrar los botones de pausa.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    /// <summary>
    /// Restaura el tiempo a la normalidad y ordena la carga de la escena del menú principal.
    /// Invocado por el botón "Volver al Menú".
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Es vital devolver el tiempo a 1 antes de cambiar de escena, o el menú principal aparecerá congelado
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