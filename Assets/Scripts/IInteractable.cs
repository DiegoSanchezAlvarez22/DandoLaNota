using UnityEngine;

public interface IInteractable
{
    // Devuelve el texto que se mostrará en pantalla (Ej: "Recoger bola de papel")
    string GetInteractPrompt();
    
    // Ejecuta la acción principal al pulsar el botón
    void OnInteract(PlayerInteraction player);
}
