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
        return "[1] Jugar  [2] Llamar  [3] Proyector\n[Q] Guardar";
    }

    public override void OnInteract(PlayerInteraction player)
    {
        currentPlayer.SetFrozen(true);
        if (!isHeld) ForceEquip(player); 
    }

    private void Update()
    {
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
        prankData = data; 
        
        if (accionActual == MobileAction.Jugar) pointsAwarded = puntosJugar;
        else if (accionActual == MobileAction.Llamar) pointsAwarded = puntosLlamar;
        else if (accionActual == MobileAction.Proyector) pointsAwarded = puntosProyector;

        if (QTEManager.Instance != null)
        {
            currentPlayer.SetFrozen(true); 
            currentPlayer.SetCurrentPrank(prankData);
            QTEManager.Instance.StartMinigame(prankData.minigameType, OnMinigameResult);
        }
    }

    public override void Drop(Vector3 throwForce)
    {
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
        StudentIdentity[] todosLosAlumnos = FindObjectsOfType<StudentIdentity>();
        List<StudentIdentity> alumnosValidos = new List<StudentIdentity>();

        // Filtramos para quedarnos solo con los NPCs
        foreach (StudentIdentity alumno in todosLosAlumnos)
        {
            if (!alumno.IsPlayer() && alumno.gameObject.activeInHierarchy)
            {
                alumnosValidos.Add(alumno);
            }
        }

        if (alumnosValidos.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, alumnosValidos.Count);
            StudentIdentity victima = alumnosValidos[indiceAleatorio];

            Debug.Log($"[Gamberrada Móvil] ¡Llamando a: {victima.gameObject.name} (Pupitre {victima.GetDeskID()})!");

            if (tonoLlamada != null)
            {
                AudioSource.PlayClipAtPoint(tonoLlamada, victima.transform.position);
            }

            // Enviamos al profesor a echarle la bronca al NPC
            TeacherAI profesor = FindObjectOfType<TeacherAI>();
            if (profesor != null)
            {
                profesor.CatchStudent(victima, currentPlayer);
            }
        }
        else
        {
            Debug.LogError("[Gamberrada Móvil] ERROR: La lista de víctimas está vacía. ¡Asegúrate de que los NPCs tengan el script 'StudentIdentity' añadido y 'Is Player' desmarcado!");
        }
    }

    private void EjecutarProyector()
    {
        GameObject proyector = GameObject.Find("Canvas_Proyector");
        if (proyector != null)
        {
            proyector.transform.GetChild(0).gameObject.SetActive(true);
            Debug.Log("¡Imagen subida al proyector!");
        }
        else
        {
            Debug.LogWarning("No se ha encontrado el objeto " + nombreObjetoProyector + " en la jerarquía.");
        }
    }
}