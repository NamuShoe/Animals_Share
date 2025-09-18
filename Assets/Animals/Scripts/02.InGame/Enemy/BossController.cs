using System;
using System.Collections;
using System.Collections.Generic;
using Redcode.Pools;
using UnityEngine;
using DG.Tweening;

public class BossController : EnemyBase
{
    [Space(20f)]
    [SerializeField] protected string[] enemyProjectileNames;
    
    private Sequence seq;
    protected float interval = 10f;

    protected override void Init()
    {
        base.Init();
        point = 100;
        SoundManager.instance.PlayBGMFade(SoundManager.GAME_BGM.Boss);

        seq = DOTween.Sequence();
        seq.AppendCallback(Pattern1st)
            .AppendInterval(interval)
            .AppendCallback(Pattern2nd)
            .AppendInterval(interval)
            .AppendCallback(Pattern3th)
            .AppendInterval(interval)
            .AppendCallback(Pattern4th)
            .AppendInterval(interval)
            .SetLoops(-1)
            .SetUpdate(false).Pause();
        
        transform.localPosition = new Vector3(0, 9, 0);
        transform.DOMoveY(3.0f, 1f).OnComplete(() => seq.Play());
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        seq.Pause();
        seq.Kill();
    }

    protected virtual void Pattern1st()
    {
        //Debug.LogError("Pattern1st");
    }
    
    protected virtual void Pattern2nd()
    {
        //Debug.LogError("Pattern2nd");
    }
    
    protected virtual void Pattern3th()
    {
        //Debug.LogError("Pattern3th");
    }
    
    protected virtual void Pattern4th()
    {
        //Debug.LogError("Pattern4th");
    }

    public override bool TakeDamage(float damage)
    {
        return base.TakeDamage(damage * WeaponController.instance.bossAttackPowerPercent);
    }

    protected override void DamageEffect(Color color)
    {
        DOTween.To(() => 0.5f, x => material.SetFloat("_HitEffectBlend", x), 0f, 0.25f);

        if (color == Color.white)
        {
            SoundManager.instance.PlayOneShot(SoundManager.GAME_SFX.Damage);
        }
    }

    protected override void Dead()
    {
        base.Dead();
        
        CollectItem collectItem = particlePoolManager.GetFromPool<CollectItem>("Item");
        collectItem.PopItem(transform.position, 1f, GameManager.instance.GetDropItem(DropType.Equipment, true));

        for (int i = 0; i < UnityEngine.Random.Range(1, 3 + 1); i++) // 1, 2, 3
        {
            collectItem = particlePoolManager.GetFromPool<CollectItem>("Item");
            collectItem.PopItem(transform.position, 1f, GameManager.instance.GetDropItem(DropType.Material, true));
        }
        
        if (GameManager.instance is RankingGameManager)
            EnemyManager.instance.NextEnemyPattern();
        else
            GameManager.instance.isWin = true;

        MissionManager.instance.MissionClearCheck(15);
    }
    
    public override void OnCreatedInPool()
    {

    }

    //오브젝트 풀을 통해, 오브젝트를 활성화 시킬 경우 호출
    public override void OnGettingFromPool()
    {
        Init();
    }
}
