using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

public class QTERitmo : MonoBehaviour
{
    [SerializeField] private Slider barraDeslizante;
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float zonaVerdeMin = 0.4f;
    [SerializeField] private float zonaVerdeMax = 0.6f;

    private bool yendoDerecha = true;
    private bool jugando;
    private Action<bool> onResult;

    public void Iniciar(Action<bool> callback)
    {
        onResult = callback;
        if (barraDeslizante != null) barraDeslizante.value = 0;
        yendoDerecha = true;
        jugando = true;
    }

    private void Update()
    {
        if (!jugando) return;

        float paso = velocidad * Time.unscaledDeltaTime;
        
        if (yendoDerecha)
        {
            barraDeslizante.value += paso;
            if (barraDeslizante.value >= 1f) yendoDerecha = false;
        }
        else
        {
            barraDeslizante.value -= paso;
            if (barraDeslizante.value <= 0f) yendoDerecha = true;
        }

        // Lectura con el Nuevo Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jugando = false;
            bool acierto = barraDeslizante.value >= zonaVerdeMin && barraDeslizante.value <= zonaVerdeMax;
            onResult?.Invoke(acierto);
        }
    }
}