using UnityEngine;
using System.Collections.Generic;

namespace sungwook
{
    [CreateAssetMenu(fileName = "StageDataSO", menuName = "Stage/StageDataSO", order = 0)]
    public class StageDataSO : ScriptableObject
    {
        public float spawnTime;
        public List<ObstacleDataSO> obstacles;
    }
}