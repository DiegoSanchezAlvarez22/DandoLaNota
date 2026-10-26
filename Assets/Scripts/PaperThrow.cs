using UnityEngine;
using UnityEngine.InputSystem;

public class PaperThrow : MonoBehaviour
{
    [SerializeField] InputActionAsset controls;
    [SerializeField] GameObject paperBallPrefab;
    [SerializeField] Transform cameraTransform;
    [SerializeField] float throwForce = 0.4f;
    [SerializeField] float cooldown = 0.5f;

    InputAction throwAction;
    float nextThrow;

    void OnEnable()
    {
        throwAction = controls.FindAction("Gameplay/Throw");
        throwAction.Enable();
    }
    void OnDisable() => throwAction.Disable();

    void Update()
    {
        if (Time.time < nextThrow) return;
        if (!throwAction.WasPressedThisFrame()) return;

        nextThrow = Time.time + cooldown;

        Vector3 pos = cameraTransform.position + cameraTransform.forward * 0.5f;
        GameObject ball = Instantiate(paperBallPrefab, pos, Quaternion.identity);
        ball.GetComponent<Rigidbody>().AddForce(cameraTransform.forward * throwForce, ForceMode.Impulse);
    }
}