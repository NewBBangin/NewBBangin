using UnityEngine;

namespace sungwook
{
    public class CarController : MonoBehaviour
    {
        [Header("References")]
        [field : SerializeField]
        public CarInputReader CarInputReader { get; private set; }
        [field : SerializeField]
        public CarMovement    CarMovement    { get; private set; }

        private void Update()
        {
            SendValues();
        }

        private void SendValues()
        {
            CarMovement.SetMoveInput(CarInputReader.InputDir);
        }
    }
}