using UnityEngine;

public class GearGrinder : MonoBehaviour
{
    [SerializeField] private AudioSource grindSound;

    private void OnTriggerEnter(Collider other)
    {
        BreakableObject target = other.GetComponentInParent<BreakableObject>();
        if (!target || target.IsGrinding) return;

        target.Grind(transform, other.ClosestPoint(transform.position));
        
        
        // if (target.IsGrinding && grindSound)
        // {
        //     grindSound.pitch = Random.Range(0.85f, 1.2f);
        //     grindSound.Play();
        // }
    }
}
