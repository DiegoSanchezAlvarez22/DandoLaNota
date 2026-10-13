using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Instancia Global")]
    // Instancia estática única para acceder al GameManager desde cualquier otro script (Patrón Singleton)
    public static GameManager Instance { get; private set; }

    [Header("Configuración Global (Persistente)")]
    // Nivel de dificultad actual seleccionado en el menú (1: Fácil, 2: Medio, 3: Difícil, 4: Extremo)[cite: 2, 3]
    [SerializeField] private int currentDifficulty = 1;

    [Header("Configuración de Partida")]
    // Duración total de la partida en segundos (ej. 480 segundos = 8 minutos)[cite: 2]
    [SerializeField] private float matchDurationSeconds = 480f;
    // Límite máximo de puntos de enfado antes de ser expulsado del aula[cite: 2]
    [SerializeField] private int maxPunishmentPoints = 100;

    [Header("Estado de la Partida Actual")]
    // Tiempo restante de la partida en curso
    [SerializeField] private float currentMatchTime;
    // Puntos de risa acumulados mediante gamberradas durante la partida actual[cite: 2]
    [SerializeField] private int currentLaughPoints = 0;
    // Puntos de castigo acumulados por ser pillado por el profesor[cite: 2]
    [SerializeField] private int currentPunishmentPoints = 0;
    // Indicador booleano que determina si el temporizador y el juego están activos
    [SerializeField] private bool isMatchActive = false;

    //[Header("Eventos de Interfaz")]
    // Evento disparado cada segundo para actualizar el reloj visual del aula
    public event Action<float> OnTimeUpdated;
    // Evento disparado al ganar o perder puntos para actualizar los marcadores
    public event Action<int, int> OnPointsUpdated;

    private void Awake()
    {
        // Configuración del patrón Singleton para garantizar una única existencia del GameManager
        if (Instance == null)
        {
            Instance = this;
            // Evita que este objeto se destruya al cambiar de la escena del menú a la del juego
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
    /// Reinicia los valores e inicia una nueva partida.
    /// Se invoca al cargar la escena del aula desde el menú.
    /// </summary>
    public void StartNewMatch()
    {
        currentMatchTime = matchDurationSeconds;
        currentLaughPoints = 0;
        currentPunishmentPoints = 0;
        isMatchActive = true;

        OnPointsUpdated?.Invoke(currentLaughPoints, currentPunishmentPoints);
        Debug.Log("Nueva partida iniciada. Nivel de dificultad: " + currentDifficulty);
    }

    /// <summary>
    /// Añade puntos de risa al marcador global de la partida.
    /// Invocado por los objetos interactuables tras realizar una gamberrada con éxito[cite: 2].
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

        // Comprobación de límite de expulsión[cite: 2]
        if (currentPunishmentPoints >= maxPunishmentPoints)
        {
            EndMatch(true);
        }
    }

    /// <summary>
    /// Finaliza la partida, detiene el tiempo y calcula la puntuación final.
    /// </summary>
    /// <param name="expelled">Verdadero si la partida acaba por expulsión, falso si es por tiempo</param>
    private void EndMatch(bool expelled)
    {
        isMatchActive = false;

        // Cálculo de la puntuación final: diferencia entre risa y enfado[cite: 2]
        int finalScore = currentLaughPoints - currentPunishmentPoints;

        if (expelled)
        {
            Debug.Log("Partida terminada: Expulsado del aula. Puntuación final: " + finalScore);
            // Aquí se llamaría a la UI de derrota
        }
        else
        {
            Debug.Log("Partida terminada: Tiempo agotado. Puntuación final: " + finalScore);
            // Aquí se llamaría a la UI de victoria/resumen
        }

        // Aquí se conectará en el futuro con LootLockerManager para subir el 'finalScore' al ranking[cite: 2, 9]
    }
}