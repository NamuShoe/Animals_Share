using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseController : PlayerController
{
    private EnemyManager enemyManager;
    private float enhanceDamagePercent = 0f;
    
    protected override void Start()
    {
        enemyManager = EnemyManager.instance;
        
        base.Start();
    }

    private IEnumerator Eradication()
    {
        var killCountWaitWhile = new WaitWhile(() => enemyManager.TotalKillCount % 3 == 0);
        var killCountWaitUntil = new WaitUntil(() => enemyManager.TotalKillCount % 3 == 0);
        var fireCountWaitUntil0 = new WaitUntil(() => weaponController.FireCount % 2 == 0);
        var fireCountWaitUntil1 = new WaitUntil(() => weaponController.FireCount % 2 == 1);
        yield return new WaitWhile(() => enemyManager.TotalKillCount == 0);
        
        while (true)
        {
            yield return killCountWaitWhile;
            yield return killCountWaitUntil;
            
            weaponController.IncreaseAttackPower(enhanceDamagePercent);
            if (weaponController.FireCount % 2 == 0)
                yield return fireCountWaitUntil1;
            else
                yield return fireCountWaitUntil0;

            weaponController.IncreaseAttackPower(-enhanceDamagePercent);
        }
    }
    
    //퇴치 본능
    protected override void BasicSkill()
    {
        StartCoroutine(Eradication());
        enhanceDamagePercent = 50f;
    }
    protected override void A1Skill() { weaponController.IncreaseProjectileSpeed(3); }
    protected override void B1Skill() { weaponController.IncreaseAttackSpeed(3); }
    protected override void A2Skill() { IncreaseDrainPercent(1.5f); }
    protected override void B2_1Skill() { IncreaseMoveSpeed(3); }
    protected override void B2_2Skill() { IncreaseAvoid(2); }
    protected override void A3Skill() { weaponController.IncreaseCriticalPercent(4); }
    protected override void B3Skill() { weaponController.IncreaseCriticalPercent(2); }
    //퇴치의 달인
    protected override void LastSkill() { enhanceDamagePercent = 100f; }
}

