using UnityEngine;
using System;

public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject canvasQTE;

    [Header("Controladores de Minijuegos")]
    [SerializeField] private QTESpam qteSpam;
    [SerializeField] private QTERitmo qteRitmo;
    [SerializeField] private QTESecuencia qteSecuencia;

    private Action<bool> currentCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (canvasQTE != null) canvasQTE.SetActive(false);
    }

    /// <summary>
    /// Inicia un minijuego de QTE según el tipo especificado
    /// </summary>
    public void StartMinigame(MinigameType type, Action<bool> callback)
    {
        currentCallback = callback;

        if (canvasQTE != null) canvasQTE.SetActive(true);

        // Congelar el mundo 3D y liberar el cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ocultar todos los paneles por seguridad
        qteSpam.gameObject.SetActive(false);
        qteRitmo.gameObject.SetActive(false);
        qteSecuencia.gameObject.SetActive(false);

        // Activa e inicia solo el correspondiente
        switch (type)
        {
            case MinigameType.SpamBoton:
                qteSpam.gameObject.SetActive(true);
                qteSpam.Iniciar(TerminarMinijuego);
                break;
            case MinigameType.BarraRitmo:
                qteRitmo.gameObject.SetActive(true);
                qteRitmo.Iniciar(TerminarMinijuego);
                break;
            case MinigameType.SecuenciaTeclas:
                qteSecuencia.gameObject.SetActive(true);
                qteSecuencia.Iniciar(TerminarMinijuego);
                break;
        }
    }

    /// <summary>
    /// Finaliza el minijuego actual y devuelve el resultado al callback proporcionado.
    /// </summary>
    private void TerminarMinijuego(bool success)
    {
        qteSpam.gameObject.SetActive(false);
        qteRitmo.gameObject.SetActive(false);
        qteSecuencia.gameObject.SetActive(false);

        if (canvasQTE != null) canvasQTE.SetActive(false);

        // Reanudar el mundo 3D y bloquear el cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentCallback?.Invoke(success);
    }
}