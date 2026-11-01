using UnityEngine;
using TMPro;

public class Temporizador : MonoBehaviour
{
    [SerializeField] TMP_Text Tiempo;

    void Update()
    {
        if (GameManager.Instance == null || Tiempo == null) return;

        float t = Mathf.Max(0f, GameManager.Instance.currentMatchTime);
        int min = Mathf.FloorToInt(t / 60f);
        int seg = Mathf.FloorToInt(t % 60f);
        Tiempo.text = $"{min:00}:{seg:00}";
    }
}