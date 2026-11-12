using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("ÃÑ¾Ë ¼³Á¤")]
    private Rigidbody _rb;
    private Vector3 BulletDir;
    [SerializeField] private float speed;

    [Header("ÃÑ¾Ë Å¸°Ù °¨Áö")]
    [SerializeField] private float dectRange;
    [SerializeField] private LayerMask whatisTarget;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    
    private void Update()
    {
            BulletDir = (DectTarget().transform.position - transform.position).normalized;
         
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = BulletDir * speed;
    }

    private Collider DectTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, dectRange, whatisTarget);

        float shortestdistance = int.MaxValue;

        Collider target = null;

        foreach(var nearestTarget in targets)
        {
            float distance = (nearestTarget.transform.position - transform.position).magnitude;
            if(shortestdistance > distance)
            {
                shortestdistance = distance;
                target = nearestTarget;
            }
        }
        return target;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, dectRange);
        Gizmos.color = Color.red;
    }
}
