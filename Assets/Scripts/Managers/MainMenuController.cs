using UnityEngine;
using UnityEngine.UI; // Necesario si se referencian componentes de UI directamente (opcional aquí)
using UnityEngine.Audio;

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

    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Configuración de Pantalla")]
    [SerializeField] private Toggle fullscreenToggle;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string FULLSCREEN_KEY = "IsFullscreen";

    private void Start()
    {
        // Cargar las configuraciones guardadas de audio y pantalla
        LoadSettings();

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

    // ========================================================================
    // FUNCIONES DE OPCIONES Y GUARDADO DE DATOS
    // ========================================================================

    public void SetMasterVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("MasterVol", db);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetMusicVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("MusicVol", db);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("SFXVol", db);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
    }

    private void LoadSettings()
    {
        float masterVal = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        float musicVal = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfxVal = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

        if (masterSlider != null) masterSlider.value = masterVal;
        if (musicSlider != null) musicSlider.value = musicVal;
        if (sfxSlider != null) sfxSlider.value = sfxVal;

        SetMasterVolume(masterVal);
        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);

        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        SetFullscreen(isFullscreen);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}