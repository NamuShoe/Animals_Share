using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PigeonBossController : BossController
{
    // [Header("1st Pattern")]
    // [Header("2nd Pattern")]
    // [Header("3th Pattern")]
    // [Header("4th Pattern")]
    
    protected override void Init()
    {
        base.Init();
    }

    protected override void Pattern1st()
    {
        base.Pattern1st();
        BacteriaAwl();
    }

    #region Pattern1st

    private void BacteriaAwl()
    {
        animator.Play("Pigeon_1st", -1, 0);
        DOVirtual.DelayedCall(1.0f, Shot1st);
        DOVirtual.DelayedCall(1.5f, Shot1st);
    }
    
    private void Shot1st()
    {
        float angle = 45f/(3 + 1);
        for (int i = 0; i < 3; i++)
        {
            var rotation = transform.rotation;
            Quaternion spawnRotation = Quaternion.Euler(rotation.x, rotation.y, angle * (i + 1) - 22.5f);
            var projectile =
                EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[0], transform.position, spawnRotation);
            projectile.attackPower = 25 * multiple;
        }
    }
    
    #endregion
    
    protected override void Pattern2nd()
    {
        base.Pattern2nd();
        BacteriaAwl();
    }
    
    #region Pattern2nd

    
    #endregion

    protected override void Pattern3th()
    {
        base.Pattern3th();
        animator.Play("Pigeon_2nd", -1, 0);
        DOVirtual.DelayedCall(1.0f, BacteriaWhirl);
    }
    
    #region Pattern3th
    
    private void BacteriaWhirl()
    {
        Shot3th();
    }

    private void Shot3th()
    {
        var projectile =
            (BounceEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[1],
                transform.position, Quaternion.identity);
        projectile.attackPower = 20f * multiple;
        projectile.AddForce(new Vector2(-1, -1f), ForceMode2D.Impulse);
        
        projectile =
            (BounceEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[1],
                transform.position, Quaternion.identity);
        projectile.attackPower = 20f * multiple;
        projectile.AddForce(new Vector2(1, -1f), ForceMode2D.Impulse);
    }
    
    #endregion
    
    protected override void Pattern4th()
    {
        base.Pattern4th();
    }
    
    #region Pattern4th
    
    #endregion

    protected virtual void OnDisable()
    {
        base.OnDisable();
    }
}
