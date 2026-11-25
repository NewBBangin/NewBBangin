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
            transform.position = _firePos.position;

            _ps = GetComponent<ParticleSystem>();

            Explosion();
        }

        private void Explosion()
        {
            _ps.transform.position = _firePos.position;
            
            Collider[] coll = Physics.OverlapSphere(transform.position, _range);
            Debug.Log("[TNT] 폭발");
            
            _ps.Play();
            
            foreach (Collider c in coll)
            {
                if (c.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddExplosionForce(_fibre, transform.position, _range, 1f, ForceMode.Impulse);
                }
            }
        }

    }
}
