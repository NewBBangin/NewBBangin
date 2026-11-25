using DG.Tweening;
using UnityEngine;

public class CarVisualTween : MonoBehaviour
{
    [SerializeField] private CarMovement movement;

    [Header("Tilt")]
    [SerializeField] private float maxRoll = 5f;
    [SerializeField] private float maxYaw = 20f;

    [Header("Spring")]
    [SerializeField, Range(0f, 1f)] private float leadAmount = 0.25f;
    [SerializeField] private float frequency = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float damping = 0.75f;
    [SerializeField] private float counterKick = 40f;

    private float roll, rollVel;
    private float yaw, yawVel;
    private float lastInputSign;
    private Quaternion baseRotation;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<CarMovement>();
        if (movement == null)
        {
            Rigidbody car = GetComponentInParent<Rigidbody>();
            if (car != null) movement = car.GetComponentInChildren<CarMovement>();
        }

        baseRotation = transform.localRotation;
    }

    private void Update()
    {
        if (movement == null) return;

        float dt = Time.deltaTime;
        float input = Mathf.Clamp(movement.InputX, -1f, 1f);
        float ratio = movement.SideRatio;

        float source = Mathf.Lerp(ratio, input, leadAmount);

        // 반대로 꺾을 때 원래 방향으로 한 번 더 쏠림
        float inputSign = Mathf.Abs(input) > 0.1f ? Mathf.Sign(input) : 0f;
        if (inputSign != 0f && lastInputSign != 0f && inputSign != lastInputSign && Mathf.Abs(ratio) > 0.3f)
        {
            rollVel += -Mathf.Sign(ratio) * counterKick;
        }
        if (inputSign != 0f) lastInputSign = inputSign;

        Spring(ref roll, ref rollVel, -source * maxRoll, dt);
        Spring(ref yaw, ref yawVel, source * maxYaw, dt);

        transform.localRotation = baseRotation * Quaternion.Euler(0f, yaw, roll);
    }

    private void Spring(ref float value, ref float velocity, float target, float dt)
    {
        float omega = 2f * Mathf.PI * frequency;
        float f = 1f + 2f * dt * damping * omega;
        float oo = omega * omega;
        float hoo = dt * oo;
        float hhoo = dt * hoo;
        float detInv = 1f / (f + hhoo);

        float detX = f * value + dt * velocity + hhoo * target;
        float detV = velocity + hoo * (target - value);

        value = detX * detInv;
        velocity = detV * detInv;
    }
    
    #region  doRotation
    /*[SerializeField] private CarMovement movement;

    [Header("Angle")]
    [SerializeField] private float yawAngle = 20f;
    [SerializeField] private float rollAngle = 5f;

    [Header("Tween")]
    [SerializeField] private float turnDuration = 0.25f;
    [SerializeField] private float returnDuration = 0.35f;
    [SerializeField] private float overshoot = 1.5f;
    [SerializeField] private float threshold = 0.2f;

    private int currentDir;
    private Vector3 baseEuler;
    private Tween rotateTween;

    private void Awake()
    {
        if (movement == null) movement = GetComponentInParent<CarMovement>();
        if (movement == null)
        {
            Rigidbody car = GetComponentInParent<Rigidbody>();
            if (car != null) movement = car.GetComponentInChildren<CarMovement>();
        }

        baseEuler = transform.localEulerAngles;
    }

    private void Update()
    {
        if (movement == null) return;

        int dir = GetDir();
        if (dir == currentDir) return;

        currentDir = dir;
        Rotate(dir);
    }

    private int GetDir()
    {
        float ratio = movement.SideRatio;
        if (ratio > threshold) return 1;
        if (ratio < -threshold) return -1;
        return 0;
    }

    private void Rotate(int dir)
    {
        rotateTween?.Kill();

        Vector3 target = baseEuler + new Vector3(0f, dir * yawAngle, -dir * rollAngle);
        float duration = dir == 0 ? returnDuration : turnDuration;

        rotateTween = transform.DOLocalRotate(target, duration).SetEase(Ease.OutBack, overshoot);
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
    }*/
    

    #endregion
    
}