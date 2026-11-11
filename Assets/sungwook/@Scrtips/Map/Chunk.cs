using UnityEngine;

public class Chunk : MonoBehaviour
{
    public FloorType[,,] VoxelGrid { get; private set; }
    
    private FloorDataSO  floorDataSO;
    private int          sizeX;
    private int          sizeY;
    private int          sizeZ;
    
    public void InitializeChunk(FloorDataSO dataSO, Vector3 spawnPosition)
    {
        floorDataSO = dataSO;
        sizeX       = dataSO.chunkSizeX;
        sizeY       = dataSO.chunkSizeY;
        sizeZ       = dataSO.chunkSizeZ;

        transform.position = spawnPosition;
        VoxelGrid          = new FloorType[sizeX, sizeY, sizeZ];

        GenerateVoxelData();
        BuildVoxelBlocks();
    }
    
    private void GenerateVoxelData()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                VoxelGrid[x, 0, z] = FloorType.Floor_01;
                
                for (int y = 1; y < sizeY; y++)
                {
                    VoxelGrid[x, y, z] = FloorType.Empty;
                }
            }
        }
    }
    
    private void BuildVoxelBlocks()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GameObject prefabToSpawn = GetBlockPrefab(BlockType.Red);
        if (prefabToSpawn == null) return;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    if (VoxelGrid[x, y, z] != FloorType.Empty)
                    {
                        Vector3 localPos = new Vector3(x, y, z);
                        GameObject block = Instantiate(prefabToSpawn, transform);
                        block.transform.localPosition = localPos;
                    }
                }
            }
        }
    }

    private GameObject GetBlockPrefab(BlockType type)
    {
        if (floorDataSO == null || floorDataSO.blocks == null) return null;
        
        foreach (var block in floorDataSO.blocks)
        {
            if (block.blockType == type) return block.blockPrefab;
        }
        return null;
    }
    
    public void SetVoxelData(int x, int y, int z, FloorType type)
    {
        if (IsValidIndex(x, y, z))
        {
            VoxelGrid[x, y, z] = type;
        }
    }
    
    public FloorType PopAndZero(int x, int y, int z)
    {
        if (!IsValidIndex(x, y, z)) return FloorType.Empty;

        FloorType originalType = VoxelGrid[x, y, z];
        VoxelGrid[x, y, z] = FloorType.Empty;
        
        return originalType;
    }

    private bool IsValidIndex(int x, int y, int z)
    {
        return x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0 && z < sizeZ;
    }
}
