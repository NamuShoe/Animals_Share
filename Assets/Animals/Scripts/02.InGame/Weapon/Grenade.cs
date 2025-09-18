using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private PlayerController playerController;
    
    private float damage = 0f;
    private float range = 3f;
    private float drainAmount = 0f;
    private Tweener moveTweener;
    private Tweener rotateTweener;
    
    private PoolManager particlePoolManager;

    private void Awake()
    {
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        playerController = PlayerController.instance;
    }

    private void OnEnable()
    {
        Movement();
    }

    private void OnDisable()
    {
        moveTweener.Pause();
        moveTweener.Kill();
        rotateTweener.Pause();
        rotateTweener.Kill();
    }

    private void Movement()
    {
        moveTweener = transform.DOMove(transform.up * 3f, 1.5f).SetEase(Ease.OutCirc).SetRelative(true)
            .OnComplete(TakeBombDamage);
        rotateTweener = transform.GetChild(0).DORotate(new Vector3(0, 0, 360f * 5), 1.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }

    private void TakeBombDamage()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range / 2f);
        foreach (var enemy in cols)
        {
            if (!enemy.gameObject.CompareTag("Enemy")) continue;
            
            var enemyDead = enemy.gameObject.GetComponent<EnemyBase>().TakeDamage(damage);
            if(enemyDead) playerController.HealHp(drainAmount * 100f);
        }
        
        GameObject particle = particlePoolManager.GetFromPool<ParticlePool>("GrenadeEffect", transform.position, Quaternion.identity).gameObject;
        particle.transform.localScale = Vector3.one * (range * 1.5f);
        gameObject.SetActive(false);
    }

    public void SetDamageAndRange(float damage, float range)
    {
        this.damage = damage;
        this.range = range;
    }
    
    public void IncreaseDrainAmount(float percent) { drainAmount += percent / 100f; }
}
