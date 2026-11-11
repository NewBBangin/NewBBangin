using UnityEngine;

namespace Hyunwoo.MakeSkill
{
    /// <summary>Rotates only the blade and hub; mounting hardware stays fixed.</summary>
    [DisallowMultipleComponent]
    public sealed class GearRotor : MonoBehaviour
    {
        [Tooltip("Rotation around the gear's local Y axis, in degrees per second.")]
        public float degreesPerSecond = 1250f;

        private void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
