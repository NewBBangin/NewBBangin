using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;
    
    private void Update()
    {
        if (Keyboard.current.wKey.isPressed)
            transform.position += new Vector3(0, 0, 1) * (speed * Time.deltaTime); 
    }
}
