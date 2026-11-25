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

    internal Dictionary<FloorType, GameObject> CreateBlockPrefabLookup()
    {
        var prefabs = new Dictionary<FloorType, GameObject>();
        if (blocks == null) return prefabs;

        foreach (Block block in blocks)
        {
            if (block.floorType != FloorType.Empty && !prefabs.ContainsKey(block.floorType))
            {
                prefabs.Add(block.floorType, block.blockPrefab);
            }
        }

        if (prefabs.ContainsKey(FloorType.Floor_01)) return prefabs;

        // Older assets used the first Red entry for the base floor.
        foreach (Block block in blocks)
        {
            if (block.floorType == FloorType.Empty && block.blockType == BlockType.Red)
            {
                prefabs.Add(FloorType.Floor_01, block.blockPrefab);
                break;
            }
        }

        return prefabs;
    }
}
