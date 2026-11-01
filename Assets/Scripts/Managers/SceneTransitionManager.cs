using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Instancia Global")]
    // Variable estática para aplicar el patrón Singleton
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Configuración Visual")]
    // Grupo de Canvas que contiene la imagen negra a pantalla completa
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    // Duración en segundos de los fundidos a negro y transparente
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Nombres de Escenas")]
    // Nombre exacto de la escena del menú principal en los Build Settings
    [SerializeField] private string mainMenuSceneName;
    // Nombre exacto de la escena del juego en los Build Settings
    [SerializeField] private string gameSceneName;

    private void Awake()
    {
        // Patrón Singleton: asegura que el mánager sobreviva a las cargas de escena
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
        // Al arrancar el juego por primera vez, forzamos la pantalla a negro absoluto y hacemos el fundido
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 1f;
            transitionCanvasGroup.blocksRaycasts = true; // Bloquea los clics en la UI durante el fundido
            
            StartCoroutine(InitialFadeRoutine());
        }
    }

    /// <summary>
    /// Realiza el fundido inicial de negro a transparente al abrir la aplicación.
    /// </summary>
    private IEnumerator InitialFadeRoutine()
    {
        yield return StartCoroutine(FadeRoutine(0f));
        transitionCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Carga la escena del juego con una transición de fundido a negro y luego a transparente.
    /// </summary>
    public void LoadGameScene()
    {
        StartCoroutine(TransitionToSceneRoutine(gameSceneName, true));
    }

    /// <summary>
    /// Carga la escena del menú principal con una transición de fundido a negro y luego a transparente.
    /// </summary>
    public void LoadMainMenuScene()
    {
        StartCoroutine(TransitionToSceneRoutine(mainMenuSceneName, false));
    }

    /// <summary>
    /// Corrutina que maneja la transición asíncrona a una escena específica.
    /// </summary>
    private IEnumerator TransitionToSceneRoutine(string sceneName, bool isStartingMatch)
    {
        // Bloqueamos la interfaz para que el jugador no pulse botones mientras se oscurece
        transitionCanvasGroup.blocksRaycasts = true;
        
        // Fundido a negro completo antes de iniciar la carga
        yield return StartCoroutine(FadeRoutine(1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Esperamos a que el motor termine de cargar los recursos de la nueva escena
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Permitimos que la escena recién cargada arranque
        asyncLoad.allowSceneActivation = true;
        yield return null; 

        if (isStartingMatch)
        {
            GameManager.Instance.StartNewMatch();
        }

        // Fundido a transparente (ahora funcionará aunque el GameManager haya puesto el tiempo a 0)
        yield return StartCoroutine(FadeRoutine(0f));
        
        // Devolvemos el control de los menús al jugador
        transitionCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Realiza la interpolación lineal del CanvasGroup ignorando la escala de tiempo del juego.
    /// </summary>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (transitionCanvasGroup == null) yield break;

        float startAlpha = transitionCanvasGroup.alpha;
        float timeElapsed = 0f;

        // Suavizado progresivo hacia el valor alfa objetivo
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        // Garantiza que la opacidad finalice en el valor exacto solicitado
        transitionCanvasGroup.alpha = targetAlpha;
    }
}