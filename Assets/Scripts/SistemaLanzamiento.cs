using UnityEngine;
using UnityEngine.InputSystem;

public class SistemaLanzamiento : MonoBehaviour
{
    public GameObject bolaDePapelPrefab;
    public Transform puntoDeLaMano;
    public float fuerzaLanzamiento = 15f;

    public InputActionReference accionGenerarBola;
    public InputActionReference accionApuntarBola;
    public InputActionReference accionLanzarBola;

    private GameObject bolaEnMano;

    private void OnEnable()
    {
        if (accionGenerarBola != null) accionGenerarBola.action.Enable();
        if (accionApuntarBola != null) accionApuntarBola.action.Enable();
        if (accionLanzarBola != null) accionLanzarBola.action.Enable();
        Debug.Log("1. Controles activados correctamente.");
    }

    void Update()
    {
        if (accionGenerarBola.action.WasPressedThisFrame())
        {
            Debug.Log("2. ¡Has pulsado la E (GenerarBola)!");

            if (bolaDePapelPrefab == null || puntoDeLaMano == null)
            {
                Debug.LogError("ERROR: Faltan el Prefab o el PuntoMano en el Inspector.");
                return;
            }

            if (bolaEnMano == null)
            {
                GenerarBolaEnMano();
                Debug.Log("3. ¡Bola generada y emparentada!");
            }
        }

        bool estaApuntando = accionApuntarBola.action.IsPressed();

        if (accionLanzarBola.action.WasPressedThisFrame() && estaApuntando && bolaEnMano != null)
        {
            Debug.Log("4. ¡Bola lanzada!");
            bolaEnMano.transform.SetParent(null);
            bolaEnMano.GetComponent<Rigidbody>().isKinematic = false;
            bolaEnMano.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * fuerzaLanzamiento, ForceMode.Impulse);
            bolaEnMano = null;
        }
    }

    void GenerarBolaEnMano()
    {
        bolaEnMano = Instantiate(bolaDePapelPrefab, puntoDeLaMano.position, puntoDeLaMano.rotation, puntoDeLaMano);
        bolaEnMano.GetComponent<Rigidbody>().isKinematic = true;
    }
}