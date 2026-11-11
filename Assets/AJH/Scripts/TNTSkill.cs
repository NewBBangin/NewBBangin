using System;
using Hyunwoo.Scripts.Skill;
using UnityEngine;

namespace AJH.Scripts
{
    public class TNTSkill : SkillSO
    {
        [SerializeField] private float _value;
        
        //UpgrateManger _upgrateManger;
        
        //폭발 범위
        //폭발 강도
        public override void Init(/*UpgrateManager upgrateManager, */)
        {
            //_upgrateManger = upgrateManager
            
        }

        public override void Run()
        {
            
        }
    }
}