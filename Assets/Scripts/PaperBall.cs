using UnityEngine;

public class PaperBall : MonoBehaviour
{
    [SerializeField] float lifeTime = 5f;
    bool hasHit;

    void Start() => Destroy(gameObject, lifeTime);

    void OnCollisionEnter(Collision c)
    {
        if (hasHit) return;
        hasHit = true;

        TeacherAI teacher = FindFirstObjectByType<TeacherAI>();
        if (teacher != null) teacher.HearNoise(transform.position);
    }
}