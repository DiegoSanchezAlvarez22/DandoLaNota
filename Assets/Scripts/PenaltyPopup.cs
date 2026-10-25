using UnityEngine;
using TMPro;
using System.Collections;

public class PenaltyPopup : MonoBehaviour
{
    public static PenaltyPopup Instance;

    [SerializeField] TMP_Text text;
    [SerializeField] float showTime = 1.5f;

    void Awake()
    {
        Instance = this;
        text.gameObject.SetActive(false);
    }

    public void Show(int points)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(points));
    }

    IEnumerator ShowRoutine(int points)
    {
        text.text = "-" + points;
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(showTime);
        text.gameObject.SetActive(false);
    }
}