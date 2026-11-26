using UnityEngine;
using UnityEngine.InputSystem;

namespace sungwook
{
    public class CarInputReader : MonoBehaviour, Controls.IPlayerActions
    {
        private Controls controls;
        
        public Vector2 InputDir { get; private set; }

        private void OnEnable()
        {
            if (controls == null)
            {
                controls = new Controls();
            }
            controls.Player.SetCallbacks(this);
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Player.SetCallbacks(null);
            if (controls != null)
            {
                controls.Disable();
            }
        }
        
    
        public void OnMove(InputAction.CallbackContext context)
        {
            InputDir = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
        
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
        
        }
    }   
}