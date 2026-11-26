using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private BoxCollider floorCollider;

    public int ChunkIndex { get; private set; }
    public Bounds WorldBounds => TransformBounds(localBounds, transform.localToWorldMatrix);

    private Bounds localBounds;
    private bool isInitialized;
    private readonly List<GameObject> spawnedObjects = new();

    public bool InitializeChunk()
    {
        if (isInitialized) return true;
        if (!TryGetLocalFloorBounds(transform, floorCollider, out localBounds)) return false;

        isInitialized = HasFloorArea(WorldBounds);
        return isInitialized;
    }

    internal static bool TryMeasurePrefab(GameObject prefab, out Bounds bounds)
    {
        bounds = default;
        Chunk chunk = prefab.GetComponent<Chunk>();
        Transform root = prefab.transform;
        if (!TryGetLocalFloorBounds(root, chunk != null ? chunk.floorCollider : null, out Bounds local))
            return false;

        bounds = TransformBounds(local, Matrix4x4.TRS(Vector3.zero, root.localRotation, root.localScale));
        return HasFloorArea(bounds);
    }

    private static bool HasFloorArea(Bounds bounds)
    {
        return bounds.size.x > 0.001f && bounds.size.z > 0.001f;
    }

    private static bool TryGetLocalFloorBounds(Transform root, BoxCollider floor, out Bounds bounds)
    {
        if (floor == null) floor = root.GetComponent<BoxCollider>();
        if (floor != null)
        {
            bounds = TransformBounds(new Bounds(floor.center, floor.size),
                root.worldToLocalMatrix * floor.transform.localToWorldMatrix);
            return true;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = default;
        if (renderers.Length == 0) return false;
        bounds = TransformBounds(renderers[0].localBounds,
            root.worldToLocalMatrix * renderers[0].transform.localToWorldMatrix);
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(TransformBounds(renderers[i].localBounds,
                root.worldToLocalMatrix * renderers[i].transform.localToWorldMatrix));
        }
        return true;
    }

    public void Activate(int chunkIndex, Vector3 position)
    {
        ChunkIndex = chunkIndex;
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Recycle()
    {
        ClearSpawnedObjects();
        gameObject.SetActive(false);
    }

    public bool TryGetRandomSpawnPosition(float edgePadding, float heightOffset, out Vector3 position)
    {
        position = default;
        if (!isInitialized || !gameObject.activeInHierarchy) return false;

        Bounds bounds = WorldBounds;
        float paddingX = Mathf.Clamp(edgePadding, 0f, bounds.extents.x);
        float paddingZ = Mathf.Clamp(edgePadding, 0f, bounds.extents.z);
        position = new Vector3(
            Random.Range(bounds.min.x + paddingX, bounds.max.x - paddingX),
            bounds.max.y + heightOffset,
            Random.Range(bounds.min.z + paddingZ, bounds.max.z - paddingZ));
        return true;
    }

    public void RegisterSpawnedObject(GameObject spawnedObject)
    {
        if (spawnedObject == null) return;
        spawnedObject.transform.SetParent(transform, true);
        spawnedObjects.Add(spawnedObject);
    }

    private void ClearSpawnedObjects()
    {
        foreach (GameObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject == null) continue;
            spawnedObject.SetActive(false);
            Destroy(spawnedObject);
        }
        spawnedObjects.Clear();
    }

    private void OnDestroy()
    {
        ClearSpawnedObjects();
    }

    private static Bounds TransformBounds(Bounds bounds, Matrix4x4 matrix)
    {
        Bounds result = new Bounds(matrix.MultiplyPoint3x4(bounds.min), Vector3.zero);
        for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 2; z++)
                {
                    Vector3 corner = new Vector3(
                        x == 0 ? bounds.min.x : bounds.max.x,
                        y == 0 ? bounds.min.y : bounds.max.y,
                        z == 0 ? bounds.min.z : bounds.max.z);
                    result.Encapsulate(matrix.MultiplyPoint3x4(corner));
                }
        
        return result;
    }
}
