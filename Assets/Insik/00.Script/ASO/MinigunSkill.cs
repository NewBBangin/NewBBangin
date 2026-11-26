using Hyunwoo.Scripts.Skill;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "SkillSO/MinigunSkill", fileName = "MinigunSkill")]
public class MinigunSkill : SkillSO
{
    bool isActived = false;

    public GameObject miniGun;

    public override void Init()
    {
        miniGun = GameObject.Find("MiniGun");
    }

    public override void Run()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame) isActived = !isActived;

        miniGun.SetActive(isActived);
    }
}
