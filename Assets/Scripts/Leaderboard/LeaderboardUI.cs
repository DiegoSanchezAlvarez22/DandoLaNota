using UnityEngine;
using TMPro;
using System.Collections; // Necesario para las corrutinas

public class LeaderboardUI : MonoBehaviour
{
    [Header("Referencias de la Tabla")]
    [SerializeField] private Transform rowsParent;
    [SerializeField] private GameObject rowPrefab;

    private void OnEnable()
    {
        // Esperar a que el LootLockerManager esté listo antes de refrescar la tabla
        StartCoroutine(WaitAndRefresh());
    }

    /// <summary>
    /// Espera en segundo plano hasta que el LootLockerManager exista en la escena.
    /// </summary>
    private IEnumerator WaitAndRefresh()
    {
        // Pausa la ejecución aquí hasta que la instancia deje de ser nula
        while (LootLockerManager.Instance == null)
        {
            yield return null;
        }

        // Una vez asegurado que existe, pide los datos
        RefreshLeaderboard();
    }

    /// <summary>
    /// Refresca la tabla de clasificación descargando los datos más recientes y actualizando la UI
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (rowsParent == null) return;

        // Limpieza de filas anteriores
        foreach (Transform child in rowsParent)
        {
            Destroy(child.gameObject);
        }

        LootLockerManager.Instance.FetchTopHighScores((scores) =>
        {
            if (scores == null) return;

            for (int i = 0; i < scores.Length; i++)
            {
                GameObject rowGo = Instantiate(rowPrefab, rowsParent);
                LeaderboardRowUI rowUI = rowGo.GetComponent<LeaderboardRowUI>();
                if (rowUI == null) continue;

                string playerName = "Anónimo";

                // Buscamos el nombre del jugador en el orden de prioridad
                if (!string.IsNullOrEmpty(scores[i].metadata))
                {
                    playerName = scores[i].metadata;
                }
                else if (scores[i].player != null && !string.IsNullOrEmpty(scores[i].player.name))
                {
                    playerName = scores[i].player.name;
                }
                else if (!string.IsNullOrEmpty(scores[i].member_id))
                {
                    playerName = "Jugador " + scores[i].member_id;
                }

                // Asignamos la información al prefab
                rowUI.Setup(scores[i].rank.ToString(), playerName, scores[i].score.ToString());
            }
        });
    }
}