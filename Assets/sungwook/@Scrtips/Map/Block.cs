using UnityEngine;

[System.Serializable]
public struct Block
{
    public BlockType blockType;
    public GameObject blockPrefab;
    public BlockDataSO blockDataSO;
    
    public int xCount;
    public int yCount;
    public int zCount;
}