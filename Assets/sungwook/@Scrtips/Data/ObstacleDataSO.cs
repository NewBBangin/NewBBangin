using UnityEngine;

namespace sungwook
{
    [CreateAssetMenu(fileName = "ObstacleDataSO", menuName = "Obstacle/ObstacleDataSO", order = 0)]
    public class ObstacleDataSO : ScriptableObject
    {
        public GameObject obstaclePrefab;
    }
}