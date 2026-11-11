using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PoolDataListSO", menuName = "SO/PoolDataListSO", order = 0)]
public class PoolDataListSO : ScriptableObject
{
    public List<PoolItemDataSO> poolList;
}