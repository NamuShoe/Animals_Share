using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroController : PlayerController
{
    private bool isDart = false;
    protected override void Start()
    {
        var user_Weapon = DataManager.instance.userData.equipmentSpecificList[DataManager.instance.userData.CurrentWeaponId].id;
        Weapon weapon = ItemManager.instance.weapons.Find(w => w.id == user_Weapon);

        if (weapon.weaponCode == 3) // 다트
            isDart = true;
        
        base.Start();
    }
    
    //다트 마스터
    protected override void BasicSkill() 
    { 
        if(!isDart) return; 
        weaponController.IncreaseAttackSpeed(10);
        weaponController.IncreaseCriticalPercent(5);
    }
    protected override void A1Skill() { if(isDart) weaponController.IncreaseProjectileSpeed(4); }
    protected override void B1Skill() { weaponController.IncreaseAttackSpeed(3); }
    protected override void A2Skill() { IncreaseDrainPercent(1.5f); }
    protected override void B2_1Skill() { IncreaseMoveSpeed(3); }
    protected override void B2_2Skill() { IncreaseAvoid(2); }
    protected override void A3Skill() { SkillManager.instance.AddProbability("멀티샷", 0.5f); }
    protected override void B3Skill() { weaponController.IncreaseCriticalPercent(2); }
    //다트의 달인
    protected override void LastSkill()
    {
        if (!isDart) return;
        weaponController.IncreaseAttackSpeed(10);
        weaponController.IncreaseCriticalPercent(15);
    }
}