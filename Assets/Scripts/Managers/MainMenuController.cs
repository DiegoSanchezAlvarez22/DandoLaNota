using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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

    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Configuración de Pantalla")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Configuración de Control")]
    [SerializeField] private Slider sensitivitySlider;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string FULLSCREEN_KEY = "IsFullscreen";
    private const string SENSITIVITY_KEY = "MouseSensitivity";

    private void Start()
    {
        // Cargar las configuraciones guardadas
        LoadSettings();

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

    // ========================================================================
    // NUEVO: FUNCIONES DE OPCIONES Y SENSIBILIDAD
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

    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
    }

    private void LoadSettings()
    {
        // Cargar Audio
        float masterVal = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        float musicVal = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfxVal = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

        if (masterSlider != null) masterSlider.value = masterVal;
        if (musicSlider != null) musicSlider.value = musicVal;
        if (sfxSlider != null) sfxSlider.value = sfxVal;

        SetMasterVolume(masterVal);
        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);

        // Cargar Pantalla Completa
        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        SetFullscreen(isFullscreen);

        // Cargar Sensibilidad del Ratón (Valor por defecto: 2.0f)
        float sensitivityVal = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 2.0f);
        if (sensitivitySlider != null) sensitivitySlider.value = sensitivityVal;
        SetSensitivity(sensitivityVal);
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}