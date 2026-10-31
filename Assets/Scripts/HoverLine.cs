using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverLine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image underlineImage;
    public Sprite[] lineSprites;
    public float animSpeed = 10f;
    private float targetFill = 0f;

    void Update()
    {
        if (underlineImage != null)
        {
            underlineImage.fillAmount = Mathf.Lerp(underlineImage.fillAmount, targetFill, Time.deltaTime * animSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cambia la imagen por una aleatoria de la lista
        if (lineSprites.Length > 0 && underlineImage != null)
        {
            int randomIndex = Random.Range(0, lineSprites.Length);
            underlineImage.sprite = lineSprites[randomIndex];
        }
        
        targetFill = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetFill = 0f;
    }
}