using System.Collections.Generic;
using UnityEngine;

namespace KSM._00.Scripts
{
    public class UpgradeStatTester : MonoBehaviour
    {
        public float baseSpeed = 20f;
        public float baseMaxHP = 100f;
        public float finalSpeed,finalMaxHP ;

        public bool sawBladeUnlocked;
        public Dictionary<string, float> ReadTypeValues = new();

        private NodeManager _manager;
        private UpgradeTarget[] unlockTargets =
        {
            UpgradeTarget.SawBlade,
        };

        private void Start()
        {
            _manager = NodeManager.Instance;

            if (_manager == null)
            {
                Debug.LogError("NodeManager가 없습니다.");
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
            
            foreach (UpgradeTarget target in unlockTargets)
            {
                bool unlocked = _manager.IsUpgradeUnlocked(target);

                if (unlocked)
                {
                    Debug.Log($"{target}: 해금 완료");
                }
                else
                {
                    Debug.Log($"{target}: 미해금");
                }
            }
            finalSpeed = _manager.CalculateStat(UpgradeTarget.MaxSpeed, baseSpeed);
            finalMaxHP = _manager.CalculateStat(UpgradeTarget.MaxHP, baseMaxHP);
            
        }
    }
}