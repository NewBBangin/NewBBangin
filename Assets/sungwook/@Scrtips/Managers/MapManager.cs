using System.Linq;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
    [Header("References")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public GameObject[] chunkPrefabs = System.Array.Empty<GameObject>();

    [Header("Settings")]
    [Min(0)] 
    public int poolSize = 10;
    [Min(1)] 
    public int visibleChunks = 4;
    [Min(0)] 
    public int chunksBehindPlayer = 1;
    public bool IsInitialized => ChunkManager.Instance != null && ChunkManager.Instance.IsInitialized;
    public Chunk FrontChunk => IsInitialized ? ChunkManager.Instance.FrontChunk : null;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (IsInitialized) return;

        if (playerTransform == null)
        {
            sungwook.CarMovement car = FindAnyObjectByType<sungwook.CarMovement>();
            if (car != null)
            {
                playerTransform = car.targetParent != null ? car.targetParent : car.transform;
            }
        }

        Transform target = playerTransform != null ? playerTransform : cameraTransform;
        
        GameObject[] validPrefabs = chunkPrefabs?.Where(prefab => prefab != null).Distinct().ToArray() 
                                    ?? System.Array.Empty<GameObject>();
        ChunkManager.Instance.Initialize(validPrefabs, target, poolSize, visibleChunks,
            chunksBehindPlayer, transform.position);
    }
}
