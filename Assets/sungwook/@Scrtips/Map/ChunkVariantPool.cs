using System.Collections.Generic;
using UnityEngine;

internal sealed class ChunkVariantPool
{
    private readonly IReadOnlyList<GameObject> prefabs;
    private readonly Queue<Chunk>[] available;
    private readonly Dictionary<Chunk, int> owners = new();
    private readonly Transform poolRoot;
    private readonly Transform storage;

    public ChunkVariantPool(IReadOnlyList<GameObject> prefabs, Transform poolRoot)
    {
        this.prefabs = prefabs;
        this.poolRoot = poolRoot;
        available = new Queue<Chunk>[prefabs.Count];
        for (int i = 0; i < available.Length; i++) available[i] = new Queue<Chunk>();

        storage = new GameObject("Available").transform;
        storage.SetParent(poolRoot, false);
        storage.gameObject.SetActive(false);
    }

    public bool Prewarm(int total)
    {
        for (int i = 0; i < total; i++)
        {
            int type = i % prefabs.Count;
            Chunk chunk = Create(type);
            if (chunk == null) return false;
            available[type].Enqueue(chunk);
        }
        return true;
    }

    public Chunk Rent(int type)
    {
        Chunk chunk = available[type].Count > 0 ? available[type].Dequeue() : Create(type);
        if (chunk != null) chunk.transform.SetParent(poolRoot, false);
        return chunk;
    }

    public void Return(Chunk chunk)
    {
        chunk.Recycle();
        chunk.transform.SetParent(storage, false);
        available[owners[chunk]].Enqueue(chunk);
    }

    private Chunk Create(int type)
    {
        GameObject instance = Object.Instantiate(prefabs[type], storage);
        instance.SetActive(false);
        instance.transform.localPosition = Vector3.zero;
        if (!instance.TryGetComponent(out Chunk chunk)) chunk = instance.AddComponent<Chunk>();
        if (!chunk.InitializeChunk())
        {
            Object.Destroy(instance);
            return null;
        }
        owners.Add(chunk, type);
        return chunk;
    }
}
