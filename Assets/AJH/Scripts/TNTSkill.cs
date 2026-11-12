using System;
using Hyunwoo.Scripts.Skill;
using UnityEngine;

namespace AJH.Scripts
{
    [CreateAssetMenu(fileName = "TNTSkill", menuName = "SO/AJH/TNTSkill")]
    public class TNTSkill : SkillSO
    {
        private Transform _car;
        private float _timer;
        [SerializeField]private float _interval;
        [SerializeField] private GameObject _prefab;
        
        public override void Init()
        {
            _timer = 0;
            _car = GameObject.Find("Car").transform;
        }

        public override void Run()
        {
            _timer += Time.deltaTime;
            if (_timer > _interval)
            {
                _timer = 0;
                //터지는거
                Instantiate(_prefab/*차 앞쪽 위치*/);
            }
        }
    }
}