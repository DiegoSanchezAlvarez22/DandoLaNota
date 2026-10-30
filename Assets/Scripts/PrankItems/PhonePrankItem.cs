using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PhonePrankItem : PrankItem
{
    public enum MobileAction { Ninguna, Jugar, Llamar, Proyector }

    [Header("Bases de Datos (GamberradaData)")]
    [SerializeField] private GamberradaData dataJugar;
    [SerializeField] private GamberradaData dataLlamar;
    [SerializeField] private GamberradaData dataProyector;

    [Header("Puntuaciones")]
    [SerializeField] private int puntosJugar = 300;
    [SerializeField] private int puntosLlamar = 400;
    [SerializeField] private int puntosProyector = 600;

    [Header("Referencias Externas")]
    [Tooltip("El sonido de móvil que se reproducirá donde esté el alumno")]
    [SerializeField] private AudioClip tonoLlamada;
    [Tooltip("El nombre exacto del objeto del proyector en la escena de Unity")]
    [SerializeField] private string nombreObjetoProyector = "Canvas_Proyector";

    private MobileAction accionActual = MobileAction.Ninguna;

    public override string GetInteractPrompt()
    {
        if (!isHeld) return "[V] Sacar Móvil";
        // Cambiamos las instrucciones para mostrar el menú de opciones
        return "[1] Jugar  [2] Llamar  [3] Proyector\n[Q] Guardar";
    }

    public override void OnInteract(PlayerInteraction player)
    {
        // El móvil ya no se usa con la 'E', bloqueamos este botón para usar 1, 2 o 3.
        if (!isHeld) ForceEquip(player); 
    }

    private void Update()
    {
        // Leemos las teclas 1, 2 y 3 para elegir la gamberrada si tenemos el móvil en la mano
        if (isHeld && currentPlayer != null && !currentPlayer.IsFrozen && Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) IniciarGamberradaMovil(MobileAction.Jugar, dataJugar);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) IniciarGamberradaMovil(MobileAction.Llamar, dataLlamar);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) IniciarGamberradaMovil(MobileAction.Proyector, dataProyector);
        }
    }

    private void IniciarGamberradaMovil(MobileAction tipo, GamberradaData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Falta asignar el GamberradaData para esta acción en el Inspector del móvil.");
            return;
        }

        accionActual = tipo;
        prankData = data; // Reemplazamos los datos base por los de la acción elegida
        
        if (accionActual == MobileAction.Jugar) pointsAwarded = puntosJugar;
        else if (accionActual == MobileAction.Llamar) pointsAwarded = puntosLlamar;
        else if (accionActual == MobileAction.Proyector) pointsAwarded = puntosProyector;

        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.StartMinigame(prankData.minigameType, OnMinigameResult);
        }
    }

    public override void Drop(Vector3 throwForce)
    {
        // El móvil se guarda en el bolsillo (se destruye)
        isHeld = false;
        if (currentPlayer != null) currentPlayer.ClearHand();
        Destroy(gameObject);
    }

    protected override void AccionFinalExitosa()
    {
        if (AchievementManager.Instance != null && prankData != null)
            AchievementManager.Instance.UnlockPrank(prankData.id);
        
        if (GameManager.Instance != null)
            GameManager.Instance.AddLaughPoints(pointsAwarded);

        // EJECUCIÓN DE LOS EFECTOS ESPECIALES
        if (accionActual == MobileAction.Llamar)
        {
            EjecutarLlamada();
        }
        else if (accionActual == MobileAction.Proyector)
        {
            EjecutarProyector();
        }
        else if (accionActual == MobileAction.Jugar)
        {
            Debug.Log("Has jugado 5 segundos con éxito.");
        }

        Drop(Vector3.zero); 
    }

    private void EjecutarLlamada()
    {
        // Filtramos la lista global para buscar solo a los alumnos
        List<PrankTarget> alumnos = new List<PrankTarget>();
        foreach (var target in PrankTarget.todosLosObjetivos)
        {
            if (target.tipo == TargetType.Alumno) alumnos.Add(target);
        }

        if (alumnos.Count > 0 && tonoLlamada != null)
        {
            // Elegimos un alumno al azar y hacemos sonar la música en su posición física 3D
            PrankTarget victima = alumnos[Random.Range(0, alumnos.Count)];
            AudioSource.PlayClipAtPoint(tonoLlamada, victima.transform.position);
            Debug.Log("Llamando a: " + victima.nombreObjetivo);
        }
    }

    private void EjecutarProyector()
    {
        // Buscamos el proyector en la escena (porque el móvil es un Prefab y no puede conectarse arrastrando la variable)
        GameObject proyector = GameObject.Find("Canvas_Proyector");
        if (proyector != null)
        {
            // Activamos el primer objeto hijo que tenga dentro (la imagen troll)
            proyector.transform.GetChild(0).gameObject.SetActive(true);
            Debug.Log("¡Imagen subida al proyector!");
        }
        else
        {
            Debug.LogWarning("No se ha encontrado el objeto " + nombreObjetoProyector + " en la jerarquía.");
        }
    }
}