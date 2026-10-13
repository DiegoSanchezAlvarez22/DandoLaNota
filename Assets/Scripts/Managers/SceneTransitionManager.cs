using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Instancia Global")]
    // Instancia única del gestor de escenas (Patrón Singleton)
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Configuración Visual")]
    // Componente CanvasGroup utilizado para controlar la opacidad de la pantalla de carga (cortinilla negra)
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    // Duración en segundos de la transición de fundido (fade in / fade out)
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Nombres de Escenas")]
    // Nombres exactos de las escenas configurados en el Build Settings
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "Classroom";

    private void Awake()
    {
        // Configuración del Singleton persistente
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
        // Se asegura de que la pantalla de carga esté invisible al iniciar el juego por primera vez
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Inicia la secuencia de transición hacia la escena del Aula.
    /// Puede ser invocado desde el botón "Jugar" del Menú Principal.
    /// </summary>
    public void LoadGameScene()
    {
        StartCoroutine(TransitionToSceneRoutine(gameSceneName, true));
    }

    /// <summary>
    /// Inicia la secuencia de transición hacia el Menú Principal.
    /// </summary>
    public void LoadMainMenuScene()
    {
        StartCoroutine(TransitionToSceneRoutine(menuSceneName, false));
    }

    /// <summary>
    /// Corrutina principal que gestiona el fundido a negro, la carga asíncrona y la inicialización del juego.
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar</param>
    /// <param name="isStartingMatch">Indica si se debe avisar al GameManager para empezar la partida</param>
    private IEnumerator TransitionToSceneRoutine(string sceneName, bool isStartingMatch)
    {
        // 1. Fade a negro (Bloquea interacciones durante la transición)
        transitionCanvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(FadeRoutine(1f));

        // 2. Carga de la escena en segundo plano (Asíncrona)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // Evita que la escena se muestre antes de que termine de cargar al 100%
        asyncLoad.allowSceneActivation = false;

        // Espera hasta que el motor informe de que la carga base ha terminado (el progreso llega al 0.9)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Activación de la escena cargada
        asyncLoad.allowSceneActivation = true;

        // Espera un fotograma para asegurar que la escena se ha activado en Unity
        yield return null; 

        // 3. Lógica post-carga
        if (isStartingMatch)
        {
            // Notificación al GameManager de que la clase ya está cargada para que inicie el reloj y los puntos[cite: 2]
            GameManager.Instance.StartNewMatch();
        }

        // 4. Fade a transparente (Revela la nueva escena)
        yield return StartCoroutine(FadeRoutine(0f));
        transitionCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Realiza la interpolación lineal de la opacidad del CanvasGroup a lo largo del tiempo configurado.
    /// </summary>
    /// <param name="targetAlpha">Valor objetivo de opacidad (0 = invisible, 1 = opaco)</param>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (transitionCanvasGroup == null) yield break;

        float startAlpha = transitionCanvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            // Cálculo del porcentaje de transición actual
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        transitionCanvasGroup.alpha = targetAlpha;
    }
}