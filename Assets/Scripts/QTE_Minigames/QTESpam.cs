using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class QTESpam : MonoBehaviour
{
    [SerializeField] private int clicksNecesarios = 10;
    [SerializeField] private float tiempoLimite = 3f;
    [SerializeField] private Slider barraProgreso;
    [SerializeField] private TMP_Text textoTiempo;

    private int clicksActuales;
    private float tiempoRestante;
    private bool jugando;
    private Action<bool> onResult;

    public void Iniciar(Action<bool> callback)
    {
        onResult = callback;
        clicksActuales = 0;
        tiempoRestante = tiempoLimite;
        if (barraProgreso != null) barraProgreso.value = 0;
        jugando = true;
    }

    private void Update()
    {
        if (!jugando) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        if (textoTiempo != null) textoTiempo.text = tiempoRestante.ToString("F1") + "s";

        // Lectura de la tecla espacio con el Nuevo Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            clicksActuales++;
            if (barraProgreso != null) barraProgreso.value = (float)clicksActuales / clicksNecesarios;

            if (clicksActuales >= clicksNecesarios)
            {
                jugando = false;
                onResult?.Invoke(true);
            }
        }

        if (tiempoRestante <= 0 && jugando)
        {
            jugando = false;
            onResult?.Invoke(false);
        }
    }
}