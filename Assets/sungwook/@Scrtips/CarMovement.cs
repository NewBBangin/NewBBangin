using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace sungwook
{
    public class CarMovement : MonoBehaviour
    {
        public bool IsMoving { get; private set; }
        [SerializeField] private float speed;
        [SerializeField] private float sideSpeed;   
        private Rigidbody _rigid;
        public Transform targetParent;
        public Vector2 moveInput { get; private set; }

        private void Awake()
        {
            _rigid = GetComponentInParent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 changeTrm = new Vector3(0, 0, 1) * (Time.deltaTime * speed);
            Vector3 sideMove = new Vector3(moveInput.x, 0, 0) * (Time.deltaTime * sideSpeed); 

            if (targetParent != null)
            {
                targetParent.position += changeTrm + sideMove;
            }
        }

        private void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
            if (moveInput.x != 0)
            {
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }
        }
    }   
}