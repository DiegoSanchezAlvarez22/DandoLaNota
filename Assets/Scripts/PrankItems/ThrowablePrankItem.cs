using UnityEngine;

// Fíjate que hereda de PrankItem, no de MonoBehaviour. ¡Ya incluye toda la lógica base!
public class ThrowablePrankItem : PrankItem
{
    [Header("Ajustes de Lanzamiento")]
    [SerializeField] private float fuerzaLanzamiento = 15f;
    [Tooltip("Tiempo antes de que el objeto desaparezca del suelo tras tirarlo")]
    [SerializeField] private float tiempoDeVidaTrasLanzar = 4f; 

    protected override void AccionFinalExitosa()
    {
        // 1. Damos los puntos y guardamos el logro al momento de soltarlo
        if (AchievementManager.Instance != null && prankData != null)
        {
            AchievementManager.Instance.UnlockPrank(prankData.id);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLaughPoints(pointsAwarded);
        }

        // 2. Calculamos la dirección usando la cámara del jugador (hacia donde miras)
        Vector3 direccion = currentPlayer.GetPlayerCamera().forward * fuerzaLanzamiento;

        // 3. Limpiamos la mano virtualmente para que puedas coger otra cosa
        currentPlayer.ClearHand();

        // 4. Lanzamos el objeto físicamente reutilizando la función Drop de la clase padre
        Drop(direccion);

        // 5. Lo destruimos tras unos segundos para que la clase no se llene de basura acumulada
        Destroy(gameObject, tiempoDeVidaTrasLanzar);
    }
}