using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PigBossController : BossController {
    [Header("1st Pattern")] 
    [SerializeField] private Transform machineGun;
    [SerializeField] private Transform FirePos;
    private Coroutine coroutine1st;
    [Header("2nd Pattern")]
    [SerializeField] private Transform[] missilePos = new Transform[2];
    // [Header("3th Pattern")]
    [Header("4th Pattern")]
    private Sequence rushSeq;
    
    protected override void Init()
    {
        base.Init();
    }

    protected override void Pattern1st()
    {
        base.Pattern1st();
        coroutine1st = StartCoroutine(RotateMachineGun());
        StartCoroutine(Shot1st());
    }

    #region Pattern1st

    IEnumerator RotateMachineGun()
    {
        var WFFU = new WaitForFixedUpdate();
        var target = playerController.transform;
        
        while (true) {
            Vector3 dirVector = target.position - machineGun.transform.position;
            float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            var temp = Quaternion.Slerp(machineGun.transform.rotation, rotation, 0.05f);
            temp.z = Mathf.Clamp(temp.z, -0.25f, 0.25f); // 0.25 == 45도
            machineGun.transform.rotation = temp;
            yield return WFFU;
        }
    }

    IEnumerator Shot1st()
    {
        var WFSFire = new WaitForSeconds(0.2f);
        var WFSReload = new WaitForSeconds(1.0f);
        var MGPos = machineGun.localPosition;
        
        for (int i = 0; i < 3; i++) {
            yield return WFSReload;
            for (int j = 0; j < 3; j++) {
                yield return WFSFire;
                var projectile =
                    EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[0],
                        FirePos.position, machineGun.rotation);
                projectile.attackPower = 20 * multiple;
                machineGun.DOLocalJump(MGPos, 0.02f, 1, 0.19f);
            }
        }
        StopMachineGun();
    }
    
    private void StopMachineGun()
    {
        StopCoroutine(coroutine1st);
        machineGun.transform.DOLocalRotate(Vector3.zero, 0.2f);
    }
    
    #endregion
    
    protected override void Pattern2nd()
    {
        base.Pattern2nd();
        FireMissile();
    }
    
    #region Pattern2nd

    private void FireMissile()
    {
        Shot2nd();
    }
    
    private void Shot2nd()
    {
        var projectile =
            (TrackingEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[1],
                missilePos[0].transform.position, Quaternion.identity);
        projectile.attackPower = 25f * multiple;
        
        projectile =
            (TrackingEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[1],
                missilePos[1].transform.position, Quaternion.identity);
        projectile.attackPower = 25f * multiple;
    }
    
    #endregion

    protected override void Pattern3th()
    {
        base.Pattern3th();
        FireExplosiveMissile();
    }
    
    #region Pattern3th

    private void FireExplosiveMissile()
    {
        var projectile =
            (ExplosiveEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[2],
                missilePos[0].transform.position, Quaternion.identity);
        projectile.attackPower = 50f * multiple;
        projectile.SetTimer(1f);
        
        projectile =
            (ExplosiveEnemyProjectile)EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[2],
                missilePos[1].transform.position, Quaternion.identity);
        projectile.attackPower = 50f * multiple;
        projectile.SetTimer(1f);
    }
    
    #endregion
    
    protected override void Pattern4th()
    {
        base.Pattern4th();
        Rush();
    }
    
    #region Pattern4th

    private void Rush()
    {
        float PosEndY = Camera.main.ViewportToWorldPoint(new Vector3(0, -0.5f, 0)).y;
        rushSeq = DOTween.Sequence();
        rushSeq.Append(transform.DOMoveY(7f, 1f).SetEase(Ease.Linear));
        for (int i = 0; i < 4; i++) {
            rushSeq.AppendCallback(SetPosX).AppendCallback(() => StartCoroutine(Shot4th()))
                .Append(transform.DOMoveY(PosEndY, 1.5f).SetEase(Ease.Linear))
                .AppendInterval(0.5f);
        }
        
        rushSeq.AppendCallback(() => transform.position = new Vector3(0, 7f, 0))
            .Append(transform.DOMove(new Vector3(0, 3f, 0), 1f)).SetEase(Ease.Linear);
    }
    private void SetPosX()
    {
        Vector3 PosY = new Vector3(0, 1.2f, 0);
        var position = transform.position;
        position = Camera.main.ViewportToWorldPoint(PosY);
        position = new Vector3(PlayerController.instance.transform.position.x, position.y, 0f);
        transform.position = position;
    }
    
    IEnumerator Shot4th()
    {
        var WFSFire = new WaitForSeconds(0.2f);
        var MGPos = machineGun.localPosition;
        
        for (int i = 0; i < 3; i++) {
            yield return WFSFire;
            var projectile =
                EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileNames[0],
                    FirePos.position, machineGun.rotation);
            projectile.attackPower = 20 * multiple;
            machineGun.DOLocalJump(MGPos, 0.02f, 1, 0.19f);
        }
        StopMachineGun();
    }
    
    #endregion

    protected virtual void OnDisable()
    {
        base.OnDisable();

        rushSeq.Pause();
        rushSeq.Kill();
    }
}
