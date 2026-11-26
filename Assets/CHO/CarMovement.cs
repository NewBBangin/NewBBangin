using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
     [SerializeField] private Rigidbody rigid;

    [Header("Forward")]
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField, Range(0f, 1f)] private float impactResist = 0f;
    [SerializeField] private float minSpeed = 0f;

    [Header("Side")]
    [SerializeField] private float sideSpeed = 12f;
    [SerializeField] private float pressTime = 0.15f;
    [SerializeField] private float turnTime = 0.1f;
    [SerializeField] private float releaseTime = 0.3f;
    [SerializeField] private float inputDeadZone = 0.1f;

    [Header("Road")]
    [SerializeField] private bool useRoadLimit = false;
    [SerializeField] private float roadCenterX = 0f;
    [SerializeField] private float roadHalfWidth = 8f;

    private float currentSpeed;
    private float speedMultiplier = 1f;
    private float currentSide;
    private float inputX;

    public float CurrentSpeed => currentSpeed;
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
        float target = maxSpeed * speedMultiplier;
        currentSpeed = Mathf.MoveTowards(currentSpeed, target, acceleration * dt);
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
        currentSpeed = Mathf.Max(minSpeed, currentSpeed - value * (1f - impactResist));
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
    
    /*[SerializeField] private float speed;
    [SerializeField] private float sideSpeed;   // 추가: 좌우 속도 분리
    [SerializeField] private float sideAccel;   // 누를 때 붙는 속도
    [SerializeField] private float sideDecel;   // 뗄 때 멈추는 속도
    [SerializeField] private float sideTurn;    // 반대 방향 눌렀을 때 꺾이는 속도
    private float currentSide;              
    private Rigidbody _rigid;
    public Transform targetParent;
    private Vector3 moveDir;

    private void Awake()
    {
        _rigid = GetComponentInParent<Rigidbody>();
        _rigid.isKinematic = true;   // 인스펙터에서 체크했으면 없어도 됨
    }

    private void Update()
    {
        Vector3 changeTrm = new Vector3(0, 0, 1) * Time.deltaTime * speed;
        
        float targetSide = moveDir.x * sideSpeed;

        float rate = sideAccel;
        if (moveDir.x == 0) rate = sideDecel/2;
        else if (currentSide != 0 && Mathf.Sign(targetSide) != Mathf.Sign(currentSide)) rate = sideTurn;
        
        currentSide = Mathf.MoveTowards(currentSide, targetSide, rate * Time.deltaTime);
        Vector3 sideMove = new Vector3(currentSide, 0, 0) * Time.deltaTime;
        
        if (targetParent != null)
        {
            targetParent.position += changeTrm + sideMove;
        }
    }

    public void SetMove(Vector3 move)
    {
        moveDir = move;
    }

    public void DownSpeed(float value)
    {
        speed -= value;
    }

    public void UpSpeed(float value)
    {
        speed += value;
    }*/

    
}