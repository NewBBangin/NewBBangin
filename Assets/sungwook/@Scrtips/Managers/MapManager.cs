using UnityEngine;
using System.Collections.Generic;

public enum FloorType
{
    Empty,
    Floor_01,
}

public enum BlockType
{
    Red,
    Green,
}

public class MapManager : Singleton<MapManager>
{
    [Header("References")]
    public Transform cameraTransform; 
    public GameObject chunkPrefab;

    [Header("Settings")]
    public int poolSize = 10;
    public int visibleChunks = 4;

    [SerializeField]
    private FloorDataSO floorData;

    public Dictionary<FloorType, Chunk> FloorDict { get; private set; }
    public bool IsInitialized { get; private set; } = false;
    
    private List<Chunk> chunkPool;
    private Queue<Chunk> activeChunks;
    private float nextSpawnZ = 0f;
    private float chunkLength = 10f;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (IsInitialized) return;
        
        if (floorData != null)
        {
            chunkLength = floorData.chunkSizeZ;
        }
        
        chunkPool = new List<Chunk>();
        activeChunks = new Queue<Chunk>();
        FloorDict = new Dictionary<FloorType, Chunk>();
        
        IsInitialized = true;
    }

    void Start()
    {
        for (int i = 0; i < poolSize; ++i)
        {
            GameObject chunkObj = Instantiate(chunkPrefab, transform);
            chunkObj.SetActive(false);
            
            Chunk chunkComponent = chunkObj.GetComponent<Chunk>();
            chunkPool.Add(chunkComponent);
        }
        
        for (int i = 0; i < visibleChunks; ++i)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        CheckChunks();
    }
    
    void SpawnChunk()
    {
        Chunk chunk = GetPooledChunk();
        
        if (chunk != null)
        {
            Vector3 spawnPos = new Vector3(0, 0, nextSpawnZ); 
            chunk.gameObject.SetActive(true);
            
            chunk.InitializeChunk(floorData, spawnPos);
            
            activeChunks.Enqueue(chunk);
            nextSpawnZ += chunkLength;   
        }
    }
    
    void CheckChunks()
    {
        if (activeChunks.Count == 0) return;
        
        Chunk oldestChunk = activeChunks.Peek();
        
        if (cameraTransform.position.z > oldestChunk.transform.position.z + chunkLength * 1.5f)
        {
            Chunk recycledChunk = activeChunks.Dequeue();
            recycledChunk.gameObject.SetActive(false);
            
            SpawnChunk();
        }
    }

    public void AddFloor(FloorType floorType, Chunk chunk)
    {
        if (!FloorDict.ContainsKey(floorType))
        {
            FloorDict.Add(floorType, chunk);
        }
    }

    public bool ContainsFloor(FloorType floorType)
    {
        return FloorDict.ContainsKey(floorType);
    }
    
    Chunk GetPooledChunk()
    {
        foreach (Chunk chunk in chunkPool)
        {
            if (!chunk.gameObject.activeInHierarchy)
            {
                return chunk;
            }
        }
        
        GameObject newChunkObj = Instantiate(chunkPrefab, transform);
        newChunkObj.name = chunkPrefab.name;
        newChunkObj.SetActive(false);
        
        Chunk newChunk = newChunkObj.GetComponent<Chunk>();
        chunkPool.Add(newChunk);
        return newChunk;
    }
}