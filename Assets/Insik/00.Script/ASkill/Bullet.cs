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

    private void OnEnable()
    {
        BulletDir = Camera.main.transform.forward;
    }

    private void Update()
    {
        if (DectTarget() != null)
        {
            Debug.Log("asuirgweyu");
            BulletDir = DectTarget().transform.position - transform.position;
        }  
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = BulletDir * speed;
    }

    private Collider DectTarget()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, dectRange, whatisTarget);

        float shortestdistance = int.MaxValue;

        foreach(var nearestTarget in targets)
        {
            float distance = (nearestTarget.transform.position - transform.position).magnitude;
            if(shortestdistance > distance)
            {
                shortestdistance = distance;
                return nearestTarget;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, dectRange);
        Gizmos.color = Color.red;
    }
}
