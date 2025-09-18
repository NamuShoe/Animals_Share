using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;
using Random = UnityEngine.Random;

public class ExplosiveEnemyProjectile : EnemyProjectile {
    [SerializeField] private GameObject shrapnelPrefab;
    private Pool<ShrapnelProjectile> projectilePool;
    
    private void Awake()
    {
        var container = GameObject.Find("EnemyProjectileContainer");
        projectilePool = Pool.Create((shrapnelPrefab.GetComponent<ShrapnelProjectile>()), 0, container.transform);
    }

    public void SetTimer(float time)
    {
        DOVirtual.DelayedCall(time, Explosion).OnComplete(TakeEnemyProjectileToPool);
    }

    private void Explosion()
    {
        
        for (int i = 0; i < 25; i++) {
            var dirVector = Random.insideUnitSphere;
            float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90f;
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            var projectile = projectilePool.Get(transform.position, rotation);
            projectile.transform.localScale = Vector3.one * 0.1f;
            projectile.attackPower = 20f;
            projectile.parentProjectile = this;
        }

        isCollide = true;
        //projectilePool.Get(transform.position, rotation);
        // transform.DOMove(from + Random.insideUnitCircle * range, 0.25f).SetEase(Ease.OutCubic));
    }

    public void TakeShrapnelProjectile(ShrapnelProjectile clone)
    {
        projectilePool.Take(clone);
    }
}
