using UnityEngine;
using UnityEngine.UI; // Necesario si se referencian componentes de UI directamente (opcional aquí)

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    // Referencia al panel del menú principal
    [SerializeField] private GameObject mainCanvas;

    // Referencia al panel del anexo
    [SerializeField] private GameObject annexCanvas;

    // Referencia al panel de opciones
    [SerializeField] private GameObject settingsCanvas;

    // Referencia al panel de creditos
    [SerializeField] private GameObject creditsCanvas;

    private void Start()
    {
        // Solo mostrar el panel principal al iniciar el juego
        ShowMainPanelOnly();
    }

    /// <summary>
    /// Inicia la partida cargando la escena del aula.
    /// Invocado por el botón "Jugar".
    /// </summary>
    public void PlayGame()
    {
        // Validación para evitar errores si falta el mánager de transiciones
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogError("No se ha encontrado el SceneTransitionManager en la escena.");
        }
    }

    /// <summary>
    /// Abre el panel del Anexo ocultando el menú principal.
    /// Invocado por el botón "Anexo de Gamberradas".
    /// </summary>
    public void OpenAnnex()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
    }

    /// <summary>
    /// Abre el panel de Ajustes ocultando el menú principal.
    /// Invocado por el botón "Opciones".
    /// </summary>
    public void OpenSettings()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }

    /// <summary>
    /// Abre el panel de Créditos ocultando el menú principal.
    /// Invocado por el botón "Créditos".
    /// </summary>
    public void OpenCredits()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
    }

    /// <summary>
    /// Cierra cualquier submenú y vuelve a mostrar el menú principal.
    /// Invocado por los botones "Volver" dentro de los paneles de Anexo y Ajustes.
    /// </summary>
    public void ShowMainPanelOnly()
    {
        mainCanvas.SetActive(true);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    /// <summary>
    /// Cierra la aplicación completamente.
    /// Invocado por el botón "Salir".
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

        // Instrucción que cierra el juego (solo funciona en la Build)
        Application.Quit();
    }
}