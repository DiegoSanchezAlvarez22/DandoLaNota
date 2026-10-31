using UnityEngine;
using UnityEngine.AI;

public class PasosProfesor : MonoBehaviour
{
    [SerializeField] AudioClip[] pasos;
    [SerializeField] float intervalo = 0.5f;
    [SerializeField, Range(0f, 1f)] float volumen = 0.5f;
    [SerializeField] Animator animator;

    AudioSource fuente;
    NavMeshAgent agente;
    float siguiente;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // AudioSource propio en un hijo, para no interferir con SonidosProfesor
        GameObject go = new GameObject("AudioPasos");
        go.transform.SetParent(transform, false);
        fuente = go.AddComponent<AudioSource>();
        fuente.spatialBlend = 1f;
        fuente.playOnAwake = false;
        fuente.minDistance = 1f;
        fuente.maxDistance = 12f;
    }

    void Update()
    {
        bool andando = animator != null
            && animator.GetBool("IsWalking")
            && !agente.isStopped
            && agente.velocity.sqrMagnitude > 0.1f;

        if (!andando || Time.time < siguiente) return;
        siguiente = Time.time + intervalo;

        if (pasos == null || pasos.Length == 0) return;
        AudioClip clip = pasos[Random.Range(0, pasos.Length)];
        if (clip == null) return;

        fuente.pitch = Random.Range(0.9f, 1.1f);
        fuente.PlayOneShot(clip, volumen);
    }
}