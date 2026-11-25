using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarRotation : MonoBehaviour
{
    private Vector3 moveInput;
    [SerializeField] private Vector3 _rotationValueRight;
    [SerializeField] private Vector3 _rotationValueLeft;
    [SerializeField] private float _rotationSpeed;      

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
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, target, _rotationSpeed * Time.deltaTime);
    }
}
