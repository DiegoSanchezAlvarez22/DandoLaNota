using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnexItemCardUI : MonoBehaviour
{
    [Header("Elementos de la Interfaz")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockedOverlay; // Panel semitransparente oscuro o icono de candado
    [SerializeField] private Button cardButton;

    private GamberradaData myData;
    private AnnexUIController mainController;

    /// <summary>
    /// Configura la tarjeta con los datos de la gamberrada y su estado.
    /// </summary>
    public void Setup(GamberradaData data, bool isUnlocked, AnnexUIController controller)
    {
        myData = data;
        mainController = controller;

        // Nombre (Oculto si está bloqueado)
        if (nameText != null)
            nameText.text = isUnlocked ? data.prankName : "???";

        // Icono visual
        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        // Capa de bloqueo visual
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        // Configuración del botón
        if (cardButton != null)
        {
            // Si no está desbloqueado, el botón no se puede pulsar
            cardButton.interactable = isUnlocked; 
            
            // Limpiamos eventos anteriores (por si el prefab se reutiliza) y asignamos el clic
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    private void OnCardClicked()
    {
        mainController.OpenVideoPopup(myData); // Abrir video de la gamberrada
    }
}