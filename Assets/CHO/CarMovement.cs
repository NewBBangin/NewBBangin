using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float sideSpeed;   // 추가: 좌우 속도 분리
    private Rigidbody _rigid;
    public Transform targetParent;
    private Vector2 moveInput;

    private void Awake()
    {
        _rigid = GetComponentInParent<Rigidbody>();
        _rigid.isKinematic = true;   // 추가: 인스펙터에서 체크했으면 없어도 됨
    }

    private void Update()
    {
        Vector3 changeTrm = new Vector3(0, 0, 1) * Time.deltaTime * speed;
        Vector3 sideMove = new Vector3(moveInput.x, 0, 0) * Time.deltaTime * sideSpeed;   // 변경: speed → sideSpeed

        if (targetParent != null)
        {
            targetParent.position += changeTrm + sideMove;
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}