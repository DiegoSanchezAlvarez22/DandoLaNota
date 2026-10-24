using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    [Header("Instancia Global")]
    public static AchievementManager Instance { get; private set; }

    [Header("Base de Datos de Gamberradas")]
    [SerializeField] private List<GamberradaData> allPranks = new List<GamberradaData>(); // añadir los scriptableObjects

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

    /// <summary>
    /// Marca una gamberrada como descubierta y la guarda localmente.
    /// Invocado por los objetos interactuables al terminar un minijuego.
    /// </summary>
    /// <param name="prankId">El ID único de la gamberrada</param>
    public void UnlockPrank(string prankId)
    {
        // Solo se guarda y notifica si no estaba desbloqueada antes
        if (!IsPrankUnlocked(prankId))
        {
            PlayerPrefs.SetInt("Prank_" + prankId, 1);
            PlayerPrefs.Save();
            
            Debug.Log("¡Nueva gamberrada descubierta y guardada!: " + prankId);
        }
    }

    /// <summary>
    /// Comprueba en el guardado local si el jugador ya ha descubierto esta gamberrada.
    /// </summary>
    /// <param name="prankId">El ID único de la gamberrada</param>
    public bool IsPrankUnlocked(string prankId)
    {
        // Si no existe la clave, devuelve 0 por defecto (falso)
        return PlayerPrefs.GetInt("Prank_" + prankId, 0) == 1;
    }

    /// <summary>
    /// Devuelve la lista completa de datos para que la UI genere la cuadrícula del Anexo.
    /// </summary>
    public List<GamberradaData> GetAllPranks()
    {
        return allPranks;
    }

    /// <summary>
    /// Herramienta de desarrollo: Borra todo el progreso de las gamberradas descubiertas.
    /// </summary>
    [ContextMenu("Borrar Progreso Local")]
    public void ResetAllPranks()
    {
        foreach (GamberradaData prank in allPranks)
        {
            PlayerPrefs.DeleteKey("Prank_" + prank.id);
        }
        PlayerPrefs.Save();
        Debug.Log("Progreso de gamberradas reseteado.");
    }
}