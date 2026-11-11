using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BreakableObject : MonoBehaviour
{
    [SerializeField] private GameObject brokenPrefab;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private float launchSpeed = 9f;
    [SerializeField] private float grindTime = 0.18f;
    [SerializeField] private float pieceForce = 8f;
    [SerializeField] private float pieceLifetime = 2.5f;

    public bool IsGrinding { get; private set; }
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Grind(Transform gear, Vector3 hitPoint)
    {
        if (IsGrinding || !brokenPrefab) return;

        IsGrinding = true;

        Vector3 direction = _rb.worldCenterOfMass - gear.position;
        direction.y = 0f;
        direction = (direction.normalized + gear.root.forward * 0.35f
            + Vector3.up * Random.Range(0.5f, 0.9f) + Random.insideUnitSphere * 0.2f).normalized;

        _rb.AddForce(direction * launchSpeed * Random.Range(0.8f, 1.25f), ForceMode.VelocityChange);
        _rb.maxAngularVelocity = 20f;
        _rb.AddTorque(Random.onUnitSphere * Random.Range(8f, 16f), ForceMode.VelocityChange);

        if (impactEffect)
        {
            ParticleSystem effect = Instantiate(impactEffect, hitPoint, Quaternion.LookRotation(direction));
            effect.Play();
            Destroy(effect.gameObject, 1.2f);
        }

        StartCoroutine(GrindRoutine(direction));
    }

    private IEnumerator GrindRoutine(Vector3 direction)
    {
        yield return new WaitForSeconds(grindTime * Random.Range(0.7f, 1.4f));

        Vector3 velocity = _rb.linearVelocity;
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        GameObject pieces = Instantiate(brokenPrefab, transform.position, transform.rotation);
        pieces.transform.localScale = transform.lossyScale;

        foreach (Rigidbody piece in pieces.GetComponentsInChildren<Rigidbody>())
        {
            piece.transform.SetParent(null, true);
            piece.transform.localScale = Vector3.Scale(piece.transform.localScale,
                new Vector3(Random.Range(0.6f, 1f), Random.Range(0.6f, 1f), Random.Range(0.6f, 1f)));

            Vector3 scatter = (piece.position - transform.position).normalized
                + direction * 0.5f + Vector3.up * 0.25f + Random.insideUnitSphere * 0.7f;

            piece.linearVelocity = velocity * 0.7f;
            piece.AddForce(scatter.normalized * pieceForce * Random.Range(0.6f, 1.6f), ForceMode.VelocityChange);
            piece.maxAngularVelocity = 25f;
            piece.angularVelocity = _rb.angularVelocity * 0.5f + Random.onUnitSphere * Random.Range(6f, 18f);
            Destroy(piece.gameObject, pieceLifetime * Random.Range(0.65f, 1f));
        }

        Destroy(pieces);
        Destroy(gameObject);
    }
}
