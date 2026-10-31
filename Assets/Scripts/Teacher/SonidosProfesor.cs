using UnityEngine;

public class SonidosProfesor : MonoBehaviour
{
    [SerializeField] AudioClip[] sonido;
    [SerializeField] float min=15f;
    [SerializeField] float max=35f;
    [SerializeField, Range(0f, 1f)] float volumen = 0.8f;

    AudioSource audio;
    float siguiente;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        if (audio == null) audio=gameObject.AddComponent<AudioSource>();
        audio.spatialBlend=1f;
        audio.playOnAwake=false;
        Siguiente();
    }

    void Update()
    {
        if (Time.time <siguiente)
        {
            return;
        }

        if (sonido.Length > 0 && !audio.isPlaying)
        {
            audio.PlayOneShot(sonido[Random.Range(0, sonido.Length)], volumen);
        }
        Siguiente();
    }

    void Siguiente() => siguiente = Time.time + Random.Range(min, max);
}
