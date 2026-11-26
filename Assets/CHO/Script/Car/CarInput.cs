using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour
{
    public Vector3 MoveDir { get; private set; }

    private void OnMove(InputValue value)
   {
       MoveDir = value.Get<Vector2>();
   }
}
