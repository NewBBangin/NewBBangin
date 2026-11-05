using Hyunwoo.Scripts.Skill;
using UnityEngine;

namespace Hyunwoo.MakeSkill
{
    [CreateAssetMenu(fileName = "GearSkill", menuName = "SO/Hyunwoo/GearSkill")]
    public class GearSkill : SkillSO
    {
        public bool skillEnable;
        
        private GameObject _gear;
        
        public override void Init()
        {
            _gear = GameObject.Find("CarGearPair");

            if (_gear == null)
            {
                Debug.LogError("gear is null");
            }
        }

        public override void Run()
        {
            if (!_gear) return;
            
            _gear.SetActive(skillEnable);
        }
    }
}
