using System;
using UnityEngine;

namespace sungwook
{
    public class CarRotation : MonoBehaviour
    {
        [SerializeField]
        private CarMovement movement;
        [SerializeField]
        private float minAngle;
        [SerializeField]
        private float maxAngle;
        [SerializeField, Range(0f, 1f)]
        private float rotateSpeed;

        private float angle;

        public float Angle
        {
            get => angle;
            set
            {
                angle = Mathf.Clamp(value, minAngle, maxAngle);
            }
        }

        private void LateUpdate()
        {
            if (movement.IsMoving)
            {
                if (movement.moveInput.x > 0)
                {
                    angle = Mathf.Lerp(angle, maxAngle, rotateSpeed);
                }
                else if (movement.moveInput.x < 0)
                {
                    angle = Mathf.Lerp(angle, minAngle, rotateSpeed);
                }
            }
            else
            {
                angle =  Mathf.Lerp(angle, 0, rotateSpeed);
            }
            
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}