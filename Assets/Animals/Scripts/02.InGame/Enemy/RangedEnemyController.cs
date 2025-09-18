using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;

public class RangedEnemyController : EnemyController {
    [Space(20f)]
    [SerializeField] private string enemyProjectileName;
    
    public int positionNum = -1;
    private Coroutine fireCoroutine;

    protected override void Init()
    {
        base.Init();
        fireCoroutine = StartCoroutine(Fire());
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        EnemyManager.instance.isRangedSpawnPositionUsing[positionNum] = false;
        StopCoroutine(fireCoroutine);
    }

    protected override void SetSpawnPosition()
    {
        //스폰 위치
        // List<RectTransform> spawnPositions = EnemyManager.instance.rangedSpawnPosition;
        // List<bool> isRangedSpawnPositionUsing = EnemyManager.instance.isRangedSpawnPositionUsing;
        //
        // Debug.LogError("controller " + positionNum);
        // RectTransform spawnPosition = spawnPositions[positionNum];
        // isRangedSpawnPositionUsing[positionNum] = true;
        // transform.position = spawnPosition.position + Vector3.up;
        
        //방향
        GameObject target = playerController.gameObject;
        Vector3 dirVector;
        if (target == null)
            dirVector = new Vector3(0, -6, 0) - transform.position;
        else
            dirVector = target.transform.position - transform.position - new Vector3(0, 3, 0);
        float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        transform.DOMove(Vector3.down, 0.5f).SetRelative();
    }

    private IEnumerator Fire() // Speed에 따른 행동패턴
    {
        float currentAttackSpeed = Speed; // 임시로
        var stayWaitSec = new WaitForSeconds(1 + 1/currentAttackSpeed);
        var attackWaitSec = new WaitForSeconds(1.0f);
        while (true)
        {
            if (Math.Abs(currentAttackSpeed - Speed) > 0.001f) // currentSpeed != speed
            {
                currentAttackSpeed = Speed;
                stayWaitSec = new WaitForSeconds(1 + 1/currentAttackSpeed);
            }
            yield return stayWaitSec;
            
            animator.Play("Attack", -1, 0);
            yield return attackWaitSec;
            var projectile =
                EnemyManager.instance.GetEnemyProjectileFromPool(enemyProjectileName, transform.position, transform.rotation);
            //var projectile = EnemyManager.instance.projectilePool.Get(transform.position, transform.rotation);
            projectile.attackPower = currentAttackPower;
        }
    }

    protected override void Movement()
    {
        //이동
        Vector3 dirVector = playerController.gameObject.transform.position - transform.position;
        float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.05f);
    }
}
