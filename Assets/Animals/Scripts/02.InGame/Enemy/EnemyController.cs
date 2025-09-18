using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : EnemyBase
{
    [Header("Coin")]
    [SerializeField] private int coinAmount = 5;
    [Range(0f, 1f)]
    [SerializeField] private float coinChance = 0.5f;

    protected Rigidbody2D rigidbody;

    protected override void Awake()
    {
        base.Awake();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        speedPercent = 1f;
    }

    protected override void Init()
    {
        base.Init();
        // var num = Convert.ToInt32(idName.Remove(0, 1)); // e001에서 번호만 구하는 함수
        // hp = EnemyDataManager.instance.enemies[num].hp * GameManager.instance.enemyTimes;
        // currentHp = hp;
        // speed = EnemyDataManager.instance.enemies[num].moveSpeed;
        // currentSpeed = UnityEngine.Random.Range(speed * 0.9f, speed * 1.1f);

        SetSpawnPosition();
    }
    

    protected virtual void SetSpawnPosition() {}

    private void SetPositionZtoZero()
    {
        var vector3 = transform.position;
        vector3.z = 0f;
        transform.position = vector3;
    }

    private void FixedUpdate()
    {
        if (isDead == true) return;
        Movement();
    }
    
    protected virtual void Movement() {}

    protected void OnBecameInvisible() // 현재 SpriteRenderer가 기존 위치와 달라지면 문제 발생
    {
        if(isDead == false)
            ReturnPool();
    }

    protected override void Dead()
    {
        base.Dead();

        if (SceneManager.GetActiveScene().name == "Ranking") return;
        if (Random.Range(0f, 1f) < coinChance) {
            GameManager.instance.AddPoint(coinAmount);
        }

        //아이템 드롭
        var sprite = GameManager.instance.GetDropItem(DropType.Material, false);
        if (sprite == null) return;
        
        CollectItem collectItem = particlePoolManager.GetFromPool<CollectItem>("Item");
        collectItem.PopItem(transform.position, 1f, sprite);
    }

    public void BuffSpeed(float speedAmount)
    {
        speedPercent += speedAmount;
    }

    //오브젝트가 처음 생성될 때 호출되며, Non Lazy = true일 경우 호출X
    public override void OnCreatedInPool()
    {

    }

    //오브젝트 풀을 통해, 오브젝트를 활성화 시킬 경우 호출
    public override void OnGettingFromPool()
    {
        //Init();
    }
}
