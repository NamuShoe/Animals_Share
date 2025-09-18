using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WildBoarBossController : BossController
{
    [Header("2nd Pattern")]
    private bool isAiming = false;
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void Pattern1st()
    {
        base.Pattern1st();
        EnemyManager enemyManager = EnemyManager.instance;
        // enemyManager.SpawnEnemy(1, 10); //모기
        // enemyManager.SpawnEnemy(3, 10); //나방
    }

    protected override void Pattern2nd()
    {
        base.Pattern2nd();
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < 2; i++)
        {
            seq.AppendCallback(() => StartCoroutine(Stay(3f)))
                .AppendInterval(3f)
                .AppendCallback(() => isAiming = true)
                .AppendCallback(() => transform.DOMove(PlayerController.instance.transform.position, 1f))
                .AppendInterval(1f)
                .Append(transform.DOMove(new Vector3(0, 3f, 0), 1f));
        }
        seq.Append(transform.DORotate(new Vector3(0, 0, 0), 0.5f));
    }

    #region Pattern2nd
    private IEnumerator Stay(float time)
    {
        yield return new WaitUntil(() => AimPlayer(time));
        isAiming = false;
    }

    private bool AimPlayer(float time)
    {
        var target = PlayerController.instance.transform;
        Vector3 dirVector = target.transform.position - transform.position;
        float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return isAiming;
    }
    #endregion

    protected override void Pattern3th()
    {
        base.Pattern3th();
    }
}
