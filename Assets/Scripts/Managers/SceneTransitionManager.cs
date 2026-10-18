using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Instancia Global")]
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Configuración Visual")]
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Nombres de Escenas")]
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string gameSceneName;

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
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }

    public void LoadGameScene()
    {
        StartCoroutine(TransitionToSceneRoutine(gameSceneName, true));
    }

    public void LoadMainMenuScene()
    {
        StartCoroutine(TransitionToSceneRoutine(mainMenuSceneName, false));
    }

    private IEnumerator TransitionToSceneRoutine(string sceneName, bool isStartingMatch)
    {
        transitionCanvasGroup.blocksRaycasts = true;
        
        // Fundido a negro
        yield return StartCoroutine(FadeRoutine(1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        yield return null; 

        if (isStartingMatch)
        {
            GameManager.Instance.StartNewMatch();
        }

        // Fundido a transparente (ahora funcionará aunque el GameManager haya puesto el tiempo a 0)
        yield return StartCoroutine(FadeRoutine(0f));
        transitionCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Realiza la interpolación lineal ignorando la escala de tiempo del juego.
    /// </summary>
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (transitionCanvasGroup == null) yield break;

        float startAlpha = transitionCanvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            yield return null;
        }

        transitionCanvasGroup.alpha = targetAlpha;
    }
}