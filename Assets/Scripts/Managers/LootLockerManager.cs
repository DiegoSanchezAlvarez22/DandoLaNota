using UnityEngine;
using LootLocker.Requests;
using System;
using System.Collections; // Necesario para la pausa (Corrutinas)

public class LootLockerManager : MonoBehaviour
{
    [Header("Instancia Global")]
    public static LootLockerManager Instance { get; private set; }

    [Header("Configuración de Leaderboard")]
    [SerializeField] private string leaderboardKey = "top_gamberros"; // Asegúrate de que sea tu ID
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

    public void StartGuestSession(string identifier, Action<bool> onComplete)
    {
        LootLockerSDKManager.StartGuestSession(identifier, (response) =>
        {
            isConnected = response.success;
            onComplete?.Invoke(response.success);
        });
    }

    /// <summary>
    /// Registra la sesión para el jugador actual limpiando espacios y con una pausa de seguridad.
    /// </summary>
    public void RegisterPlayerForMatch(string playerName, Action<bool> onComplete = null)
    {
        // Limpiamos espacios al principio y al final por si el jugador pulsó la barra espaciadora sin querer
        string cleanName = playerName.Trim();

        LootLockerSDKManager.StartGuestSession(cleanName, (response) =>
        {
            if (response.success)
            {
                isConnected = true;
                // En lugar de cambiar el nombre al instante, esperamos medio segundo
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
    /// Corrutina que da margen al servidor antes de aplicar el nombre público.
    /// </summary>
    private IEnumerator ApplyNameWithDelay(string name, Action<bool> onComplete)
    {
        // LA CLAVE ESTÁ AQUÍ: Usamos WaitForSecondsRealtime en lugar de WaitForSeconds
        // para que funcione aunque Time.timeScale sea 0.
        yield return new WaitForSecondsRealtime(0.5f);

        LootLockerSDKManager.SetPlayerName(name, (nameResponse) =>
        {
            if (!nameResponse.success)
            {
                Debug.LogWarning("El servidor rechazó el nombre: " + nameResponse.errorData.message);
            }
            // Dejamos que el jugador juegue independientemente de si el servidor aceptó el nombre o no
            onComplete?.Invoke(true); 
        });
    }

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

    private void ExecuteFetch(Action<LootLockerLeaderboardMember[]> onScoresRetrieved)
    {
        LootLockerSDKManager.GetScoreList(leaderboardKey, 10, 0, (response) =>
        {
            if (response.success) onScoresRetrieved?.Invoke(response.items);
            else Debug.LogError("Error al descargar: " + response.errorData.message);
        });
    }
}