using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FloorDataSO", menuName = "SO/MapDataSO")]
public class FloorDataSO : ScriptableObject
{
    [Header("Chunk Voxel Size")]
    public int chunkSizeX = 10;
    public int chunkSizeY = 5;
    public int chunkSizeZ = 16;

    [Header("Block Config")]
    public List<Block> blocks;
    public string groundName;
    public GameObject groundPrefab;
}