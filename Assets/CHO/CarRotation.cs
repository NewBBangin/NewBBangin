using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarRotation : MonoBehaviour
{
    [SerializeField] private CarMovement movement;

    [Header("Tilt")]
    [SerializeField] private float maxRoll = 20f;
    [SerializeField] private float maxYaw = 8f;

    [Header("Spring")]
    [SerializeField, Range(0f, 1f)] private float leadAmount = 0.35f;
    [SerializeField] private float frequency = 3f;
    [SerializeField, Range(0.1f, 1f)] private float damping = 0.6f;
    [SerializeField] private float counterKick = 120f;

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
    
    
    /*private Vector3 moveInput;
    [SerializeField] private Vector3 _rotationValueRight;
    [SerializeField] private Vector3 _rotationValueLeft;
    [SerializeField] private float _rotationSpeed;      
    [SerializeField] private GameObject  targetParent;
    
    private void Update()
    {
        if (moveInput.x > 0.1)
        {
            RotationPerant(_rotationValueRight);
        }
        else if (moveInput.x < -0.1)
        {
            RotationPerant(_rotationValueLeft);
        }
        else
        {
            RotationPerant(new Vector3(0,0,0));
        }
    }

    public void SetMove(Vector3 move)
    {
        moveInput = move;
    }

    private void RotationPerant(Vector3 rotationvalue)
    {
        Quaternion target = Quaternion.Euler(rotationvalue);
        targetParent.transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, target, _rotationSpeed * Time.deltaTime);
    }*/
}
