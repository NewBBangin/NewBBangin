using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectPush : MonoBehaviour
{
    [SerializeField] private float force;
    private Rigidbody _rb;
    private BreakableObject _breakable;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _breakable = GetComponent<BreakableObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_breakable && _breakable.IsGrinding) return;
        
        if (!collision.gameObject.CompareTag("Car")) return;

        Vector3 direction = transform.position - collision.transform.position;

        direction.y += 0.5f;
        
        _rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
}
