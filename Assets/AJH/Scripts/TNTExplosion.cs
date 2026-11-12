using System;
using Unity.VisualScripting;
using UnityEngine;

namespace AJH.Scripts
{
    public class TNTExplosion : MonoBehaviour
    {
        [SerializeField] private float _fibre;
        [SerializeField] private float _range;
        
        private void Init(float lv)
        {
            _fibre *= lv;
            _range = _fibre / 2;
            Explosion();
        }

        private void OnEnable()
        {
            Explosion();
        }

        private void Explosion()
        {
            Collider[] coll = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider c in coll)
            {
                if (c.TryGetComponent(out Rigidbody rb))
                {
                    c.GetComponent<Rigidbody>().AddExplosionForce(_fibre, transform.position, _range);
                }
            }
            Destroy(gameObject);
        }
    }
}