using UnityEngine;

namespace KSM._00.Scripts
{
    public class UpgradeStatTester : MonoBehaviour
    {
        public float baseSpeed = 20f;
        public float baseMaxHP = 100f;
        public float finalSpeed,finalMaxHP ;

        public bool sawBladeUnlocked;

        private NodeManager _manager;

        private void Start()
        {
            _manager = NodeManager.Instance;

            if (_manager == null)
            {
                Debug.LogError("NodeManager가 없습니다.", this);
                return;
            }

            _manager.OnChanged += RefreshStats;
            RefreshStats();
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnChanged -= RefreshStats;
            }
        }
        

        public void RefreshStats()
        {
            if (_manager == null) return;
            finalSpeed = _manager.CalculateStat(UpgradeTarget.MaxSpeed, baseSpeed);
            finalMaxHP = _manager.CalculateStat(UpgradeTarget.MaxHP, baseMaxHP);
                _manager.IsUpgradeUnlocked(UpgradeTarget.SawBlade);
                
                
                
           _manager.IsUpgradeUnlocked(UpgradeTarget.SawBlade);
        }
    }
}