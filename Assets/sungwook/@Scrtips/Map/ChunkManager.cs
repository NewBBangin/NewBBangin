using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ChunkManager : Singleton<ChunkManager>
{
    [SerializeField]
    private Transform playerTransform;

    public bool IsInitialized { get; private set; }
    public float ChunkLength => chunkLength;
    public IReadOnlyList<Chunk> ActiveChunks => activeChunks;
    public Chunk FrontChunk
    {
        get
        {
            TryGetFrontChunk(out Chunk chunk);
            return chunk;
        }
    }

    private readonly List<Chunk> activeChunks = new();
    private ChunkVariantPool     pool;
    private ChunkShuffleBag      order;
    private string               lastError;
    private int                  visibleCount;
    private int                  behindCount;
    private int                  firstChunkIndex = int.MinValue;
    private float                chunkLength;
    private float                chunkStartOffset;
    private Vector3              origin;
    private Transform            poolRoot;

    public bool Initialize(IReadOnlyList<GameObject> prefabs, Transform player, 
        int poolSize, int visibleChunks, int chunksBehindPlayer, Vector3 mapOrigin)
    {
        if (IsInitialized) return true;
        if (player != null) playerTransform = player;

        var types = new List<GameObject>(prefabs?.Count ?? 0);
        Bounds reference = default;
        
        foreach (GameObject prefab in prefabs ?? System.Array.Empty<GameObject>())
        {
            if (prefab == null || types.Contains(prefab)) continue;
            if (!Chunk.TryMeasurePrefab(prefab, out Bounds bounds))
            {
                return ReportError($"'{prefab.name}' needs a floor BoxCollider or Renderer with non-zero width and length.");
            }
            if (types.Count == 0) reference = bounds;
            else if (!FitsSameRoad(reference, bounds))
            {
                return ReportError($"'{prefab.name}' must match the floor edges and surface height of '{types[0].name}' relative to the prefab pivot.");
            }
            types.Add(prefab);
        }
        
        if (types.Count == 0) return ReportError("Assign at least one chunk prefab.");
        if (playerTransform == null) return ReportError("Assign a player or preview camera.");

        visibleCount = Mathf.Max(1, visibleChunks);
        behindCount = Mathf.Clamp(chunksBehindPlayer, 0, visibleCount - 1);
        origin = mapOrigin;
        chunkLength = reference.size.z;
        chunkStartOffset = reference.min.z;
        poolRoot = new GameObject("Chunk Pool").transform;
        poolRoot.gameObject.SetActive(isActiveAndEnabled);
        pool = new ChunkVariantPool(types, poolRoot);
        
        if (!pool.Prewarm(Mathf.Max(0, poolSize)))
        {
            return StopPool("Failed to initialize a floor while prewarming the chunk pool.");
        }

        order = new ChunkShuffleBag(types.Count);
        lastError = null;
        IsInitialized = true;
        RefreshChunks();
        return IsInitialized;
    }

    private static bool FitsSameRoad(Bounds first, Bounds next)
    {
        const float tolerance = 0.01f;
        return Mathf.Abs(first.min.z - next.min.z) <= tolerance &&
               Mathf.Abs(first.max.z - next.max.z) <= tolerance &&
               Mathf.Abs(first.min.x - next.min.x) <= tolerance &&
               Mathf.Abs(first.max.x - next.max.x) <= tolerance &&
               Mathf.Abs(first.max.y - next.max.y) <= tolerance;
    }

    private bool ReportError(string message)
    {
        if (lastError != message) Debug.LogError($"ChunkManager: {message}", this);
        lastError = message;
        return false;
    }

    private bool StopPool(string message)
    {
        IsInitialized = false;
        activeChunks.Clear();
        firstChunkIndex = int.MinValue;
        poolRoot.gameObject.SetActive(false);
        Destroy(poolRoot.gameObject);
        poolRoot = null;
        pool = null;
        order = null;
        return ReportError(message);
    }

    private void Update()
    {
        RefreshChunks();
    }

    public void RefreshChunks()
    {
        if (!IsInitialized || playerTransform == null) return;

        int playerChunkIndex = Mathf.FloorToInt((playerTransform.position.z - origin.z - chunkStartOffset) / chunkLength);
        int desiredFirst = playerChunkIndex - behindCount;
        if (firstChunkIndex == desiredFirst) return;
        int desiredLast = desiredFirst + visibleCount - 1;

        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            Chunk chunk = activeChunks[i];
            if (chunk.ChunkIndex >= desiredFirst && chunk.ChunkIndex <= desiredLast) continue;

            pool.Return(chunk);
            activeChunks.RemoveAt(i);
        }

        for (int index = desiredFirst; index <= desiredLast; index++)
        {
            bool alreadyActive = false;
            foreach (Chunk chunk in activeChunks)
            {
                if (chunk.ChunkIndex != index) continue;
                alreadyActive = true;
                break;
            }
            if (alreadyActive) continue;

            Chunk next = pool.Rent(order.Next());
            if (next == null)
            {
                StopPool("Failed to initialize a floor while expanding the chunk pool.");
                return;
            }
            next.Activate(index, origin + Vector3.forward * (index * chunkLength));
            activeChunks.Add(next);
        }

        activeChunks.Sort((left, right) => left.ChunkIndex.CompareTo(right.ChunkIndex));
        firstChunkIndex = desiredFirst;
    }

    public bool TryGetFrontChunk(out Chunk chunk)
    {
        chunk = null;
        if (!IsInitialized || !isActiveAndEnabled) return false;

        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (!activeChunks[i].gameObject.activeInHierarchy) continue;
            chunk = activeChunks[i];
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        if (poolRoot != null) poolRoot.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (poolRoot != null) poolRoot.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (poolRoot != null) Destroy(poolRoot.gameObject);
    }
}
