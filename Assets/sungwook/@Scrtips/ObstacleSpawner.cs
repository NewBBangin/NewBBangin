using sungwook;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ObstacleSpawner : MonoBehaviour
{
    public List<StageDataSO> stages;
    public StageDataSO CurrentStage { get; private set; }

    [HideInInspector]
    public List<GameObject> obstacles = new();

    [Header("Front Chunk Spawning")]
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField, Min(0f)] private float edgePadding = 0.5f;
    [SerializeField] private float spawnHeigh = 1f;
    [SerializeField] private bool enableDebugStageKey = true;

    private int currentIndex = -1;
    private float timer;

    private void Awake()
    {
        obstacles ??= new List<GameObject>();
        ChangeNextStage(null);
    }

    private void LateUpdate()
    {
        obstacles.RemoveAll(obstacle => obstacle == null);

        if (enableDebugStageKey && Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            ChangeNextStage(CurrentStage);
        }

        if (CurrentStage == null) return;

        timer += Time.deltaTime;

        if (timer >= Mathf.Max(0.1f, CurrentStage.spawnTime))
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    public void ChangeNextStage(StageDataSO currentStage)
    {
        if (stages == null) return;

        for (int i = currentIndex + 1; i < stages.Count; i++)
        {
            if (stages[i] == null) continue;

            currentIndex = i;
            CurrentStage = stages[i];
            timer = 0f;
            return;
        }
    }

    public void SpawnObstacle()
    {
        if (CurrentStage == null || CurrentStage.obstacles == null) return;

        if (chunkManager == null)
        {
            chunkManager = FindAnyObjectByType<ChunkManager>();
        }

        if (chunkManager == null || !chunkManager.TryGetFrontChunk(out Chunk frontChunk)) return;

        GameObject prefab = GetRandomObstaclePrefab();
        if (prefab == null || !frontChunk.TryGetRandomSpawnPosition(edgePadding, spawnHeigh, out Vector3 position)) return;

        GameObject obstacle = Instantiate(prefab, position, Quaternion.identity);
        obstacles.Add(obstacle);
        frontChunk.RegisterSpawnedObject(obstacle);
    }

    private GameObject GetRandomObstaclePrefab()
    {
        GameObject selected = null;
        int validCount = 0;

        foreach (ObstacleDataSO data in CurrentStage.obstacles)
        {
            if (data == null || data.obstaclePrefab == null) continue;

            validCount++;
            if (Random.Range(0, validCount) == 0)
            {
                selected = data.obstaclePrefab;
            }
        }

        return selected;
    }
}
