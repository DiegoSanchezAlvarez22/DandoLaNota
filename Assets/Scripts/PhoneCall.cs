using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneCall : MonoBehaviour
{
    [SerializeField] InputActionAsset controls;
    [SerializeField] float callDuration = 3f;
    [SerializeField] AudioSource classAudio;
    [SerializeField] AudioClip ringClip;
    [SerializeField] Transform phone;
    [SerializeField] Vector3 hiddenPos = new Vector3(0.2f, -0.6f, 0.4f);
    [SerializeField] Vector3 shownPos = new Vector3(0.2f, -0.15f, 0.4f);
    [SerializeField] float phoneSpeed = 8f;


    bool calling;
    float timer;
    InputAction callAction;

    void OnEnable()
    {
        callAction = controls.FindAction("Gameplay/Call");
        callAction.Enable();
    }
    void OnDisable() => callAction.Disable();

    void Update()
    {
        Vector3 target = calling ? shownPos : hiddenPos;
        phone.localPosition = Vector3.Lerp(phone.localPosition, target, phoneSpeed * Time.deltaTime);
        if (!calling)
        {
            if (callAction.WasPressedThisFrame())
            {
                calling = true;
                timer = callDuration;
                timer = callDuration;
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            calling = false;
            classAudio.PlayOneShot(ringClip);
        }
    }
}