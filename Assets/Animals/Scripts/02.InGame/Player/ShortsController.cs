using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShortsController : PlayerController
{
    private bool isLuckyPerson = false;
    
    protected override void Start()
    {
        base.Start();
    }

    protected override void Avoid()
    {
        base.Avoid();
        if (!isLuckyPerson) return;
        
        weaponController.IncreaseAttackPower(25);
        IncreaseMoveSpeed(25);
        DOVirtual.DelayedCall(5f, () =>
        {
            weaponController.IncreaseAttackPower(-25);
            IncreaseMoveSpeed(-25);
        });
    }
    
    //의도치 않은 행운
    protected override void BasicSkill() { IncreaseAvoid(3); }
    protected override void A1Skill() { IncreaseAvoid(4); }
    protected override void B1Skill() { weaponController.IncreaseAttackSpeed(3); }
    protected override void A2Skill() { /*아이템 드롭 확률 3% 증가*/ }
    protected override void B2_1Skill() { IncreaseMoveSpeed(3); }
    protected override void B2_2Skill() { IncreaseAvoid(2);}
    protected override void A3Skill() { /*스테이지 클리어시 합계 획득 골드 5% 증가*/ }
    protected override void B3Skill() { weaponController.IncreaseCriticalPercent(2); }
    //행운아
    protected override void LastSkill() { isLuckyPerson = true; }
}
