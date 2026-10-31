using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador visual del menú de opciones.
/// </summary>
public class SettingsMenuUI : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private bool isUpdatingUI = false;

    private void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            isUpdatingUI = true; // Bloqueamos el envío de datos
            
            if (masterSlider != null) masterSlider.value = SettingsManager.Instance.currentMaster;
            if (musicSlider != null) musicSlider.value = SettingsManager.Instance.currentMusic;
            if (sfxSlider != null) sfxSlider.value = SettingsManager.Instance.currentSFX;
            if (fullscreenToggle != null) fullscreenToggle.isOn = SettingsManager.Instance.currentFullscreen;
            
            isUpdatingUI = false; // Desbloqueamos el envío de datos
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        // Solo aplica el cambio si lo hace el jugador con el ratón, no el propio código
        if (!isUpdatingUI && SettingsManager.Instance != null) 
            SettingsManager.Instance.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (!isUpdatingUI && SettingsManager.Instance != null) 
            SettingsManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (!isUpdatingUI && SettingsManager.Instance != null) 
            SettingsManager.Instance.SetSFXVolume(value);
    }

    public void OnFullscreenToggled(bool isFullscreen)
    {
        if (!isUpdatingUI && SettingsManager.Instance != null) 
            SettingsManager.Instance.SetFullscreen(isFullscreen);
    }
}