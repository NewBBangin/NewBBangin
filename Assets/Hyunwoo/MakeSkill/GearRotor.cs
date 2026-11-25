using UnityEngine;

namespace Hyunwoo.MakeSkill
{
    public class GearRotor : MonoBehaviour
    {
        public float degreesPerSecond = 1250f;

        private void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
