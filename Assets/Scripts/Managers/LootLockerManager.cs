using UnityEngine;
using LootLocker.Requests;
using System;
using System.Collections;

public class LootLockerManager : MonoBehaviour
{
    [Header("Instancia Global")]
    // Instancia estática única para acceder al LootLockerManager desde cualquier otro script (Singleton)
    public static LootLockerManager Instance { get; private set; }

    [Header("Configuración de Leaderboard")]
    // Importante que el ID coincida
    [SerializeField] private string leaderboardKey = "top_gamberros";
    [SerializeField] private bool isConnected = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGuestSession("espectador_menu", null);
    }

    /// <summary>
    /// Inicia una sesión de invitado en LootLocker para poder interactuar con la tabla de clasificación
    /// </summary>
    public void StartGuestSession(string identifier, Action<bool> onComplete)
    {
        LootLockerSDKManager.StartGuestSession(identifier, (response) =>
        {
            isConnected = response.success;
            onComplete?.Invoke(response.success);
        });
    }

    /// <summary>
    /// Registra la sesión para el jugador actual limpiando espacios y con una pausa de seguridad
    /// </summary>
    public void RegisterPlayerForMatch(string playerName, Action<bool> onComplete = null)
    {
        // Limpia el añadido de espacios innecesarios
        string cleanName = playerName.Trim();

        LootLockerSDKManager.StartGuestSession(cleanName, (response) =>
        {
            if (response.success)
            {
                isConnected = true;
                StartCoroutine(ApplyNameWithDelay(cleanName, onComplete));
            }
            else
            {
                Debug.LogError("Error al iniciar sesión: " + response.errorData.message);
                isConnected = false;
                onComplete?.Invoke(false);
            }
        });
    }

    /// <summary>
    /// Corrutina que da margen al servidor antes de aplicar el nombre público
    /// </summary>
    private IEnumerator ApplyNameWithDelay(string name, Action<bool> onComplete)
    {
        // WaitForSecondsRealtime() en lugar de WaitForSeconds() para que funcione aunque Time.timeScale sea 0
        yield return new WaitForSecondsRealtime(0.5f);

        LootLockerSDKManager.SetPlayerName(name, (nameResponse) =>
        {
            if (!nameResponse.success)
            {
                Debug.LogWarning("El servidor rechazó el nombre: " + nameResponse.errorData.message);
            }
            onComplete?.Invoke(true);
        });
    }

    /// <summary>
    /// Sube la puntuación final del jugador a la tabla de clasificación
    /// </summary>
    public void SubmitScore(int finalScore)
    {
        if (!isConnected) return;

        string playerName = PlayerPrefs.GetString("PlayerName", "Anónimo").Trim();

        LootLockerSDKManager.SubmitScore("", finalScore, leaderboardKey, playerName, (response) =>
        {
            if (response.success) Debug.Log("Puntuación subida con éxito.");
            else Debug.LogError("Error al subir: " + response.errorData.message);
        });
    }

    /// <summary>
    /// Descarga las puntuaciones más altas de la tabla de clasificación y las devuelve mediante un callback
    /// </summary>
    public void FetchTopHighScores(Action<LootLockerLeaderboardMember[]> onScoresRetrieved)
    {
        if (!isConnected)
        {
            StartGuestSession("espectador_menu", (success) =>
            {
                if (success) ExecuteFetch(onScoresRetrieved);
            });
        }
        else
        {
            ExecuteFetch(onScoresRetrieved);
        }
    }

    /// <summary>
    /// Ejecuta la descarga de puntuaciones más altas y llama al callback con los resultados
    /// </summary>
    private void ExecuteFetch(Action<LootLockerLeaderboardMember[]> onScoresRetrieved)
    {
        LootLockerSDKManager.GetScoreList(leaderboardKey, 10, 0, (response) =>
        {
            if (response.success) onScoresRetrieved?.Invoke(response.items);
            else Debug.LogError("Error al descargar: " + response.errorData.message);
        });
    }
}