using Hyunwoo.Scripts.Skill;
using UnityEngine;

namespace AJH.Scripts
{
    [CreateAssetMenu(fileName = "TNTSkill", menuName = "SO/AJH/TNTSkill")]
    public class TNTSkill : SkillSO
    {
        private float _timer;
        [SerializeField]private float _interval = 5f;
        [SerializeField] private GameObject _prefab;

        public override void Init()
        {
            _timer = 0;
        }

        public override void Run()
        {
            _timer += Time.deltaTime;
            if (_timer > _interval)
            {
                _timer -= _interval;
                Debug.Log("[TNT] 소환");
                GameObject _dynamite = Instantiate(_prefab);
                _dynamite.GetComponent<TNTExplosion>().Init();
            }
        }
    }
}