using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MASTER_KEY = "MasterVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";
    private const string FULLSCREEN_KEY = "IsFullscreen";

    public float currentMaster { get; private set; }
    public float currentMusic { get; private set; }
    public float currentSFX { get; private set; }
    public bool currentFullscreen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Cargamos los datos guardados, con un valor por defecto de 0.75f (75%)
            currentMaster = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
            currentMusic = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
            currentSFX = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
            currentFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Aplicamos los valores al AudioMixer una vez que el juego ha arrancado
        SetMasterVolume(currentMaster);
        SetMusicVolume(currentMusic);
        SetSFXVolume(currentSFX);
        SetFullscreen(currentFullscreen);
    }

    public void SetMasterVolume(float value)
    {
        currentMaster = value;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("MasterVol", db);
        
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save(); // GUARDADO INMEDIATO
    }

    public void SetMusicVolume(float value)
    {
        currentMusic = value;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("MusicVol", db);
        
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save(); // GUARDADO INMEDIATO
    }

    public void SetSFXVolume(float value)
    {
        currentSFX = value;
        float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        if (audioMixer != null) audioMixer.SetFloat("SFXVol", db);
        
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save(); // GUARDADO INMEDIATO
    }

    public void SetFullscreen(bool isFullscreen)
    {
        currentFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
        
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
        PlayerPrefs.Save(); // GUARDADO INMEDIATO
    }
}