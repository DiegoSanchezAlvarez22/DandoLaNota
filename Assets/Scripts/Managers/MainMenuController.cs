using UnityEngine;

/// <summary>
/// Controla exclusivamente la navegación de paneles en la escena del menú principal.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Paneles de Interfaz")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject annexCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject creditsCanvas;

    private void Start()
    {
        // Solo mostrar el panel principal al iniciar el juego
        ShowMainPanelOnly();
    }

    public void PlayGame()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogError("No se ha encontrado el SceneTransitionManager en la escena.");
        }
    }

    public void OpenAnnex()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void OpenSettings()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
        creditsCanvas.SetActive(false);
    }

    public void OpenCredits()
    {
        mainCanvas.SetActive(false);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
    }

    public void ShowMainPanelOnly()
    {
        mainCanvas.SetActive(true);
        annexCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}