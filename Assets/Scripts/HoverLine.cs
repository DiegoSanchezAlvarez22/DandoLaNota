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
            underlineImage.fillAmount = Mathf.Lerp(underlineImage.fillAmount, targetFill, Time.unscaledDeltaTime * animSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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

    // Este método se ejecuta automáticamente cuando el objeto (o su Canvas padre) se apaga
    void OnDisable()
    {
        targetFill = 0f; // Reinicia el objetivo de la animación
        
        if (underlineImage != null)
        {
            underlineImage.fillAmount = 0f;
        }
    }
}