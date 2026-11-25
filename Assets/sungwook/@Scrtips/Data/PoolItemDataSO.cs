using UnityEngine;

[CreateAssetMenu(fileName = "PoolItemDataSO", menuName = "SO/PoolItemDataSO", order = 0)]
public class PoolItemDataSO : ScriptableObject
{
    public string      itemName;
    public GameObject  prefab;
    public int         count;

    private void OnValidate()
    {
        if(prefab == null) return;
        
        itemName = prefab.name;
    }
}
