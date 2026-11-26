using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rigid;

    [Header("Forward")]
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float acceleration = 21f;
    [SerializeField, Range(0f, 1f)] private float impactResist = 0f;
    [SerializeField] private float minSpeed = 18f;

    [Header("Side")]
    [SerializeField] private float sideSpeed = 11f;
    [SerializeField] private float pressTime = 0.3f;
    [SerializeField] private float turnTime = 0.34f;
    [SerializeField] private float releaseTime = 0.45f;
    [SerializeField] private float inputDeadZone = 0.1f;

    [Header("Road")]
    [SerializeField] private bool useRoadLimit = false;
    [SerializeField] private float roadCenterX = 0f;
    [SerializeField] private float roadHalfWidth = 8f;

    private float currentSpeed;
    private float speedMultiplier = 1f;
    private float currentSide;
    private float inputX;
    private float forwardAccel;

    public event System.Action<float> OnImpact;

    public float CurrentSpeed => currentSpeed;
    public float ForwardAccel => forwardAccel;
    public float Acceleration => acceleration;
    public float MaxSpeed => maxSpeed;
    public float InputX => inputX;
    public float SideRatio => sideSpeed > 0f ? currentSide / sideSpeed : 0f;

    private void Awake()
    {
        if (rigid == null) rigid = GetComponentInParent<Rigidbody>();

        rigid.isKinematic = true;
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        currentSpeed = maxSpeed;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        UpdateForward(dt);
        UpdateSide(dt);

        Vector3 next = rigid.position + new Vector3(currentSide, 0f, currentSpeed) * dt;

        if (useRoadLimit)
        {
            float min = roadCenterX - roadHalfWidth;
            float max = roadCenterX + roadHalfWidth;
            if (next.x < min || next.x > max)
            {
                next.x = Mathf.Clamp(next.x, min, max);
                currentSide = 0f;
            }
        }

        rigid.MovePosition(next);
    }

    private void UpdateForward(float dt)
    {
        float before = currentSpeed;
        float target = maxSpeed * speedMultiplier;
        currentSpeed = Mathf.MoveTowards(currentSpeed, target, acceleration * dt);
        forwardAccel = (currentSpeed - before) / dt;
    }

    private void UpdateSide(float dt)
    {
        float input = Mathf.Abs(inputX) < inputDeadZone ? 0f : inputX;
        float target = input * sideSpeed;

        float time;
        if (input == 0f)
            time = releaseTime;
        else if (currentSide != 0f && Mathf.Sign(target) != Mathf.Sign(currentSide))
            time = turnTime;
        else
            time = pressTime;

        float rate = sideSpeed / Mathf.Max(time, 0.0001f);
        currentSide = Mathf.MoveTowards(currentSide, target, rate * dt);
    }

    public void SetMove(Vector3 move)
    {
        inputX = move.x;
    }

    private void OnMove(InputValue value)
    {
        inputX = value.Get<Vector2>().x;
    }

    public void DownSpeed(float value)
    {
        float before = currentSpeed;
        currentSpeed = Mathf.Max(minSpeed, currentSpeed - value * (1f - impactResist));
        OnImpact?.Invoke(before - currentSpeed);
    }

    public void UpSpeed(float value)
    {
        currentSpeed += value;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void AddMaxSpeed(float value)
    {
        maxSpeed += value;
    }

    public void SetImpactResist(float value)
    {
        impactResist = Mathf.Clamp01(value);
    }
}