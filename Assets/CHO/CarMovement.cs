using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float speed;
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
    }

    
}