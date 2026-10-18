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
        // En lugar de llamar directamente, iniciamos una corrutina que espera de forma segura
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
                
                // 1º: Buscamos el nombre incrustado (Partidas nuevas)
                if (!string.IsNullOrEmpty(scores[i].metadata))
                {
                    playerName = scores[i].metadata;
                }
                // 2º: Buscamos el nombre de la cuenta general
                else if (scores[i].player != null && !string.IsNullOrEmpty(scores[i].player.name))
                {
                    playerName = scores[i].player.name;
                }
                // 3º: Respaldo para las partidas viejas que hiciste antes de este sistema
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