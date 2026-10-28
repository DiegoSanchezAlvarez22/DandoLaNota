using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class QTESecuencia : MonoBehaviour
{
    [SerializeField] private float tiempoLimite = 4f;
    [SerializeField] private TMP_Text textoSecuencia;

    private string[] teclasPosibles = { "W", "A", "S", "D" };
    private List<string> secuenciaActual = new List<string>();
    private int indiceActual;
    private float tiempoRestante;
    private bool jugando;
    private Action<bool> onResult;

    public void Iniciar(Action<bool> callback)
    {
        onResult = callback;
        secuenciaActual.Clear();
        indiceActual = 0;
        tiempoRestante = tiempoLimite;

        for (int i = 0; i < 4; i++)
        {
            secuenciaActual.Add(teclasPosibles[UnityEngine.Random.Range(0, teclasPosibles.Length)]);
        }

        ActualizarTexto();
        jugando = true;
    }

    private void ActualizarTexto()
    {
        if (textoSecuencia == null) return;
        string txt = "";
        
        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            if (i < indiceActual) txt += $"<color=#55FF55>{secuenciaActual[i]}</color> "; 
            else if (i == indiceActual) txt += $"<color=#FFFF55><b>{secuenciaActual[i]}</b></color> "; 
            else txt += $"{secuenciaActual[i]} "; 
        }
        textoSecuencia.text = txt;
    }

    private void Update()
    {
        if (!jugando || Keyboard.current == null) return;

        tiempoRestante -= Time.unscaledDeltaTime;
        if (tiempoRestante <= 0)
        {
            jugando = false;
            onResult?.Invoke(false);
            return;
        }

        bool w = Keyboard.current.wKey.wasPressedThisFrame;
        bool a = Keyboard.current.aKey.wasPressedThisFrame;
        bool s = Keyboard.current.sKey.wasPressedThisFrame;
        bool d = Keyboard.current.dKey.wasPressedThisFrame;

        if (w || a || s || d)
        {
            string teclaPulsada = "";
            if (w) teclaPulsada = "W";
            else if (a) teclaPulsada = "A";
            else if (s) teclaPulsada = "S";
            else if (d) teclaPulsada = "D";

            if (teclaPulsada == secuenciaActual[indiceActual])
            {
                indiceActual++;
                ActualizarTexto();

                if (indiceActual >= secuenciaActual.Count)
                {
                    jugando = false;
                    onResult?.Invoke(true);
                }
            }
            else
            {
                // Fallo al pulsar la tecla incorrecta
                jugando = false;
                onResult?.Invoke(false);
            }
        }
    }
}