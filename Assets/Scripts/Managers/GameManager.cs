using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Instancia Global")]
    // Instancia estática única para acceder al GameManager desde cualquier otro script (Singleton)
    public static GameManager Instance { get; private set; }

    [Header("Configuración Global (Persistente)")]
    // Nivel de dificultad actual seleccionado en el menú (1: Fácil, 2: Medio, 3: Difícil, 4: Extremo)
    [SerializeField] private int currentDifficulty = 1;

    [Header("Configuración de Partida")]
    // Duración total de la partida en segundos
    [SerializeField] private float matchDurationSeconds = 480f;
    // Límite máximo de puntos de enfado antes de ser expulsado
    [SerializeField] private int maxPunishmentPoints = 100;

    [Header("Estado de la Partida Actual")]
    // Tiempo restante de la partida en curso
    [SerializeField] private float currentMatchTime;
    // Puntos de risa acumulados
    [SerializeField] private int currentLaughPoints = 0;
    // Puntos de castigo acumulados
    [SerializeField] private int currentPunishmentPoints = 0;
    // Determina si el temporizador y el juego están activos
    [SerializeField] private bool isMatchActive = false;

    // [Header("Eventos de Interfaz")]
    // Evento cada segundo para actualizar el reloj visual del aula
    public event Action<float> OnTimeUpdated;
    // Evento al ganar o perder puntos para actualizar los marcadores
    public event Action<int, int> OnPointsUpdated;
    // Evento al finalizar la partida: (bool expelled, int laughPoints, int punishmentPoints, int finalScore)
    public event Action<bool, int, int, int> OnMatchEnded;

    private void Awake()
    {
        // Configuración del patrón Singleton para garantizar una única existencia del GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya existe un GameManager al recargar el menú, se destruye la copia duplicada
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Gestión del temporizador si la partida está en curso
        if (isMatchActive)
        {
            currentMatchTime -= Time.deltaTime;

            // Notificación a la interfaz gráfica sobre el tiempo restante
            OnTimeUpdated?.Invoke(currentMatchTime);

            // Comprobación de fin de tiempo
            if (currentMatchTime <= 0f)
            {
                currentMatchTime = 0f;
                EndMatch(false);
            }
        }
    }

    /// <summary>
    /// Reinicia los valores de la partida.
    /// Invocado al cargar la escena del aula, pero NO inicia el temporizador todavía.
    /// </summary>
    public void StartNewMatch()
    {
        currentMatchTime = matchDurationSeconds;
        currentLaughPoints = 0;
        currentPunishmentPoints = 0;

        // La partida no está activa aún
        isMatchActive = false;
        Time.timeScale = 0f;

        // Desbloquea y muestra el cursor para poder escribir en la UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPointsUpdated?.Invoke(currentLaughPoints, currentPunishmentPoints);
        Debug.Log("Nueva partida preparada. Esperando nombre del jugador...");
    }

    /// <summary>
    /// Inicia el temporizador de la partida y reanuda el tiempo real.
    /// Invocado desde la interfaz cuando el jugador confirma su nombre.
    /// </summary>
    public void BeginMatchTimer()
    {
        isMatchActive = true;
        Time.timeScale = 1f;

        // Bloquea de nuevo el cursor para controlar la cámara en primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("¡El jugador ha confirmado su nombre. Comienza el tiempo!");
    }

    /// <summary>
    /// Añade puntos de risa al marcador global de la partida.
    /// Invocado por los objetos interactuables tras realizar una gamberrada con éxito
    /// </summary>
    /// <param name="points">Cantidad de puntos a sumar</param>
    public void AddLaughPoints(int points)
    {
        if (!isMatchActive) return;

        currentLaughPoints += points;
        OnPointsUpdated?.Invoke(currentLaughPoints, currentPunishmentPoints);
    }

    /// <summary>
    /// Registra un aumento en los puntos de castigo globales del jugador.
    /// Invocado cuando el TeacherAI pilla al jugador.
    /// </summary>
    /// <param name="points">Cantidad de castigo a sumar</param>
    public void AddPunishmentPoints(int points)
    {
        if (!isMatchActive) return;

        currentPunishmentPoints += points;
        OnPointsUpdated?.Invoke(currentLaughPoints, currentPunishmentPoints);

        // Comprobación de límite de expulsión
        if (currentPunishmentPoints >= maxPunishmentPoints)
        {
            EndMatch(true);
        }
    }

    /// <summary>
    /// Finaliza la partida, congela el tiempo y notifica a la interfaz unificada de puntuación.
    /// </summary>
    /// <param name="expelled">Verdadero si termina por expulsión, falso si es por tiempo</param>
    public void EndMatch(bool expelled)
    {
        if (!isMatchActive) return;
        isMatchActive = false;
        Time.timeScale = 0f;

        // Desbloqueo del cursor para la interfaz final
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cálculo de la puntuación final
        int finalScore = GetFinalScore();

        // Notificación al controlador de UI unificado
        OnMatchEnded?.Invoke(expelled, currentLaughPoints, currentPunishmentPoints, finalScore);

        // Llama a LootLockerManager para subir 'finalScore'
        LootLockerManager.Instance.SubmitScore(finalScore);
    }

    /// <summary>
    /// Devuelve el cálculo de la puntuación final.
    /// </summary>
    public int GetFinalScore()
    {
        return currentLaughPoints - currentPunishmentPoints;
    }
}