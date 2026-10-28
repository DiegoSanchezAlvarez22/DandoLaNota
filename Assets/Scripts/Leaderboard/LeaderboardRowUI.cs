using UnityEngine;
using TMPro; // Requerido para TextMeshPro

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("Textos de la Fila")]
    // Texto que muestra la posición en el ranking (1º, 2º, 3º...)
    [SerializeField] private TMP_Text rankText;

    // Texto que muestra el nombre del jugador
    [SerializeField] private TMP_Text nameText;

    // Texto que muestra la puntuación obtenida
    [SerializeField] private TMP_Text scoreText;

    /// <summary>
    /// Configura los textos visuales de esta fila con los datos descargados.
    /// </summary>
    public void Setup(string rank, string playerName, string score)
    {
        if (rankText != null) rankText.text = rank;
        if (nameText != null) nameText.text = playerName;
        if (scoreText != null) scoreText.text = score;
    }
}