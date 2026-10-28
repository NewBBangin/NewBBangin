using UnityEngine;

namespace Hyunwoo.Scripts.Skill
{
    // 10/28
    // SkillSO
    // SkillSO 밖에서 구현할거 : 장착, Run() 실행
    // SkillSO 안에서 구현할거 : Init() => Component 불러와야할것들, Run() => 구현부, 
    
    public abstract class SkillSO : ScriptableObject
    {
        public abstract void Init();
        public abstract void Run();
    }
}
