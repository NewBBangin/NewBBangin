using System.Collections;
using UnityEngine;

namespace AJH.Scripts
{
    public class TNTExplosion : MonoBehaviour
    {
        [SerializeField] private float _fibre = 20f;
        [SerializeField] private float _range = 5f;
        [SerializeField] private float _fuseTime = 1f;
        
        private ParticleSystem _ps;
        
        private Transform _firePos;
        private Transform _truck;

        public void Init()
        {
            GameObject firePos = GameObject.Find("FirePos");

            if (firePos == null)
            {
                Debug.LogError("firePos is null");
                Destroy(gameObject);
                return;
            }

            _firePos = firePos.transform;
            _truck = _firePos.root;
            transform.position = _firePos.position;

            _ps = GetComponent<ParticleSystem>();

            Explosion();
        }

        private void Explosion()
        {
            _ps.transform.position = transform.position;

            Vector3 blastCenter = _truck.position;
            Collider[] coll = Physics.OverlapSphere(blastCenter, _range);
            Debug.Log("[TNT] 폭발");

            _ps.Play();

            foreach (Collider c in coll)
            {
                if (c.transform.root == _truck) continue;

                if (c.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddExplosionForce(_fibre, blastCenter, _range, 1f, ForceMode.Impulse);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 center = _truck != null ? _truck.position : transform.position;

            Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
            Gizmos.DrawSphere(center, _range);

            Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
            Gizmos.DrawWireSphere(center, _range);
        }

    }
}
