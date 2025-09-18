using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BatBossController : BossController
{
    [Header("1st Pattern")]
    private float buffAmount = 0.2f;

    private float buffTime = 10f;
    private bool isBuffSpeed = false;
    
    [Header("2nd Pattern")]
    [SerializeField] private GameObject soundWave;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        hp = 1100 * GameManager.instance.enemyTimes;
        speed = 2;
        soundWave.SetActive(false);
        base.Init();
    }

    protected override void Pattern1st()
    {
        base.Pattern1st();
        BuffSpeed(buffAmount);
        isBuffSpeed = true;
        DOVirtual.DelayedCall(buffTime, () => BuffSpeed(-buffAmount)).OnComplete(() => isBuffSpeed = false).SetUpdate(false);
    }

    private void BuffSpeed(float speedAmount)
    {
        EnemyController[] enemies = GameObject.FindObjectsOfType<EnemyController>(true);

        foreach (var enemy in enemies)
        {
            enemy.BuffSpeed(speedAmount);
        }
    }

    protected override void Pattern2nd()
    {
        base.Pattern2nd();
        soundWave.SetActive(true);
        DOVirtual.DelayedCall(6, () => soundWave.SetActive(false)).SetUpdate(false);
    }

    protected override void Pattern3th()
    {
        base.Pattern3th();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (isBuffSpeed == true)
            BuffSpeed(-buffAmount);
    }
}
