using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hyunwoo.Scripts.Skill
{
    public class SkillManager : MonoBehaviour
    {
        // 싱글톤 사용
        
        public static SkillManager Instance { get; private set; }

        [SerializeField] private List<SkillSO> skillList;

        public void AddSkill(SkillSO skill)
        {
            skillList.Add(skill);
        }
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            skillList.ForEach(skill => skill.Init());
        }

        private void Update()
        {
            skillList.ForEach(skill => skill.Run());
        }
    }
}