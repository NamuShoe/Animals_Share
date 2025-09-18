using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;
using UnityEngine.Animations;

public class NutriaBossController : BossController
{
    [Header("1st Pattern")]
    public bool isBlind = false;
    [SerializeField] private GameObject mudPrefeb;
    private Pool<MudBullet> pool;
    private int multiShot = 3;
    private int spreadShot = 3;
    IEnumerator fireEnumerator;
    private Tween blindTween;

    [SerializeField] private GameObject blind;
    private GameObject Highlight;
    private GameObject Background;
    
    //[Header("2nd Pattern")]
    private Sequence seq;
    
    protected override void Awake()
    {
        base.Awake();
        pool = Pool.Create<MudBullet>(mudPrefeb.GetComponent<MudBullet>(), 0).NonLazy();
        blind = GameObject.Find("Blind");
        
        Highlight = blind.transform.GetChild(0).gameObject;
        Highlight.SetActive(false);
        var player = GameObject.Find("Player");
        var source = new ConstraintSource();
        source.sourceTransform = player.transform;
        source.weight = 1;
        Highlight.GetComponent<PositionConstraint>().AddSource(source);
        
        Background = blind.transform.GetChild(1).gameObject;
        Background.SetActive(false);
    }

    protected override void Init()
    {
        hp = 1700 * GameManager.instance.enemyTimes;
        speed = 2;
        base.Init();
    }

    protected override void Pattern1st()
    {
        base.Pattern1st();
        fireEnumerator = Fire();
        StartCoroutine(fireEnumerator);
    }

    #region Pattern1st
    private IEnumerator Fire()
    {
        while (true)
        {
            for (int i = 0; i < multiShot; i++)
                Invoke("Shot", i / 10f);
            yield return new WaitForSeconds(3f);
        }
    }
    
    void Shot()
    {
        float angle = 45f/(spreadShot + 1);
        for (int i = 0; i < spreadShot; i++)
        {
            var rotation = transform.rotation;
            Quaternion spawnRotation = Quaternion.Euler(rotation.x, rotation.y, angle * (i + 1) - 22.5f);
            MudBullet mudBullet = pool.Get(transform.position, spawnRotation);
            mudBullet.SetNutriaBossController(this);
        }
    }

    public void TakeMudBullet(MudBullet _mudBullet)
    {
        pool.Take(_mudBullet);
    }

    public void Blind(bool active)
    {
        isBlind = active;
        Highlight.SetActive(active);
        Background.SetActive(active);
        if (active == true)
        {
            blindTween.Kill();
            blindTween = DOVirtual.DelayedCall(5f, () => Blind(false)).SetUpdate(false);
        }
    }
    #endregion
    
    protected override void Pattern2nd()
    {
        base.Pattern2nd();
        StopCoroutine(fireEnumerator);

        Rush();
    }
    
    #region Pattern2nd
    private void Rush()
    {
        float PosEndY = Camera.main.ViewportToWorldPoint(new Vector3(0, -0.5f, 0)).y;
        seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(7f, 1f).SetEase(Ease.Linear));
        for (int i = 0; i < 4; i++)
        {
            seq.AppendCallback(SetPosX)
                .Append(transform.DOMoveY(PosEndY, 1.5f).SetEase(Ease.Linear))
                .AppendInterval(0.5f);
        }
        
        seq.AppendCallback(() => transform.position = new Vector3(0, 7f, 0))
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
    #endregion

    protected override void Pattern3th()
    {
        base.Pattern3th();
    }
    
    protected virtual void OnDisable()
    {
        base.OnDisable();
        seq.Kill();
    }
}
