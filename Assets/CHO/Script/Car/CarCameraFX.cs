using Unity.Cinemachine;
using UnityEngine;

public class CarCameraFX : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CarMovement movement;
    [SerializeField] private CinemachineImpulseSource impulse;

    [Header("FOV")]
    [SerializeField] private float baseFov = 56f;
    [SerializeField] private float maxFov = 72f;
    [SerializeField] private float fovSpeedRef = 50f;
    [SerializeField] private float fovSmooth = 4f;

    [Header("Dutch")]
    [SerializeField] private float dutchAngle = 1.5f;
    [SerializeField] private float dutchSmooth = 6f;

    [Header("Shake")]
    [SerializeField] private float shakePerSpeedLoss = 0.1f;
    [SerializeField] private float maxShake = 1.5f;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInChildren<CarMovement>();
        if (impulse == null) impulse = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        if (movement != null) movement.OnImpact += Shake;
    }

    private void OnDisable()
    {
        if (movement != null) movement.OnImpact -= Shake;
    }

    private void LateUpdate()
    {
        if (vcam == null || movement == null) return;

        float dt = Time.deltaTime;
        float speedT = Mathf.Clamp01(movement.CurrentSpeed / fovSpeedRef);

        LensSettings lens = vcam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, Mathf.Lerp(baseFov, maxFov, speedT), 1f - Mathf.Exp(-fovSmooth * dt));
        lens.Dutch = Mathf.Lerp(lens.Dutch, -movement.SideRatio * dutchAngle, 1f - Mathf.Exp(-dutchSmooth * dt));
        vcam.Lens = lens;
    }

    private void Shake(float speedLoss)
    {
        if (impulse == null) return;
        impulse.GenerateImpulse(Mathf.Min(maxShake, speedLoss * shakePerSpeedLoss));
    }
}