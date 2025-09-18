using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour, IPoolObject
{
    public string idName;

    protected PlayerController playerController;
    protected Animator animator;
    protected Material material;
    
    [SerializeField] protected float attackPower;
    [SerializeField] protected float currentAttackPower;
    public float CurrentAttackPower => currentAttackPower;
    [SerializeField] protected float hp;
    [SerializeField] protected float currentHp;
    [SerializeField] protected float speed;
    [SerializeField] protected float currentSpeed;
    [SerializeField] protected float speedPercent = 1f;
    protected float Speed => currentSpeed * speedPercent;
    [SerializeField] protected int point = 5;

    protected float multiple = 1f;
    public bool isDead = false;
    private bool isWater = false;
    private Tween waterTween;
    private bool isPoison = false;
    private Tween poisonTween;
    private Tween shakeTween;
    
    protected PoolManager particlePoolManager;

    protected virtual void Awake()
    {
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        playerController = PlayerController.instance;
        animator = gameObject.GetComponent<Animator>();
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        
        //머테리얼 세팅
        if (0 < spriteRenderers.Count)
        {
            GetComponent<SpriteRenderer>().material = new Material(spriteRenderers[0].material);
            material = GetComponent<SpriteRenderer>().material;
            material.SetFloat("_HitEffectBlend", 0);
            spriteRenderers.RemoveAt(0);
            
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.material = material;
            }
        }
        shakeTween = null;
    }
    
    protected virtual void Init()
    {
        var num = Convert.ToInt32(idName.Remove(0, 1)); // e001에서 번호만 구하는 함수
        //빌드 시 데이터를 불러오지 못함
        var enemyData = EnemyDataManager.instance.enemies.ToList().Find(e => e.id == num);//EnemyDataManager.instance.enemies.Find(e => e.id == num);
        animator.speed = 1f;

        var stageData = StageManager.instance.stageData;
        var stageMultiple = StageManager.instance.StageMultiple;
        multiple = 1f;
        if(stageData.enemies.IndexOf(num) != -1)
            multiple = stageData.enemyMultiple[stageData.enemies.IndexOf(num)] * stageMultiple * GameManager.instance.enemyTimes;
        
        attackPower = enemyData.attackPower * multiple;
        currentAttackPower = attackPower;
        hp = enemyData.hp * multiple;
        currentHp = hp;
        speed = enemyData.moveSpeed; //enemyData.moveSpeed;
        currentSpeed = UnityEngine.Random.Range(speed * 0.9f, speed * 1.1f);
        speedPercent = 1f;
        transform.localScale = new Vector3(enemyData.size, enemyData.size, 1);
        
        isDead = false;
        isWater = false;
        isPoison = false;
        
        material.SetFloat("_HitEffectBlend", 0f);
        material.SetFloat("_GhostBlend", 0f);
    }

    protected virtual void OnEnable()
    {
        Init();
    }
    
    protected virtual void OnDisable()
    {
        waterTween.Pause();
        waterTween.Kill();
        poisonTween.Pause();
        poisonTween.Kill();
        shakeTween.Rewind();
    }

    //일반공격
    public virtual bool TakeDamage(float damage)
    {
        return TakeDamage(damage, Color.white);
    }

    //속성공격
    private bool TakeDamage(float damage, Color color)
    {
        //사망상태에서 데미지를 받는 문제 해결
        if (isDead == true) return true;
        
        if (this.gameObject.activeSelf) {
            damage = Mathf.RoundToInt(damage); // 반올림
            damage = damage <= 0 ? 1 : damage; // 0이 될 경우 1로 초기화
            currentHp -= damage;

            float randomRange = Random.Range(-0.1f, 0.1f);
            var damageText = particlePoolManager.GetFromPool<DamageText>("DamageText",
                (transform.position + Vector3.left * randomRange) + (Vector3.up * 0.35f), Quaternion.identity);
            damageText.PopDamageText(damage, color);
            
            DamageEffect(color);
            
            if (currentHp <= 0f)
            {
                Dead();
                isDead = true;
            }
        }
        return isDead;
    }

    protected virtual void DamageEffect(Color color)
    {
        DOTween.To(() => 0.5f, x => material.SetFloat("_HitEffectBlend", x), 0f, 0.25f);

        if (color == Color.white)
        {
            if (shakeTween.IsActive() && shakeTween.IsPlaying())
            {
                shakeTween.Pause();
                shakeTween.Kill();
            }
            shakeTween = transform.DOShakePosition(0.5f, 0.1f, 20);
            SoundManager.instance.PlayOneShot(SoundManager.GAME_SFX.Damage);
        }
    }

    protected virtual void Dead()
    {
        EnemyManager.instance.PlusKillCount();
        playerController.AddEXP(5);
        GameManager.instance.AddPoint(point);
        
        var num = Convert.ToInt32(idName.Remove(0, 1));
        if (DataManager.instance.userData.enemyClearCheck.Contains(num) == false) {
            DataManager.instance.userData.enemyClearCheck.Add(num);
            DataManager.instance.SaveUserData();
        }
        
        playerController.DrainHp();
        particlePoolManager.GetFromPool<ParticlePool>("DeadEffect", transform.position, Quaternion.identity);
        material.SetFloat("_GhostBlend", 1f);
        DOTween.To(() => 1f, x => animator.speed = x, 0f, 0.5f);

        DOVirtual.DelayedCall(0.5f, ReturnPool, false);
    }

    public void ReturnPool() => EnemyManager.instance.TakeEnemyToPool(idName, this);

    public void TakeFire(float damage)
    {
        float range = 3f;
        EnemyBase[] enemies = GameObject.FindObjectsOfType<EnemyBase>();
        
        List<EnemyBase> nearbyEnemies = enemies
            .Where(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= range / 2f)
            .ToList();
        
        GameObject particle = particlePoolManager.GetFromPool<ParticlePool>("FireRangeEffect", transform.position, Quaternion.identity).gameObject;
        particle.transform.localScale = Vector3.one * (range * 1.5f);
        
        foreach (var enemy in nearbyEnemies)
        {
            DOVirtual.DelayedCall((0.1f), () =>
            {
                enemy.TakeDamage(damage, Color.red);
            }, false).SetLoops(5);
            DOVirtual.DelayedCall((0.1f), () =>
                particlePoolManager.GetFromPool<ParticlePool>("FireEffect", enemy.transform.position,
                    Quaternion.identity));
        }
    }
    
    public void TakeWater(float damage)
    {
        if (isWater)
        {
            DOVirtual.DelayedCall(0.1f, () => TakeDamage(damage, Color.cyan), false);
            waterTween.Pause();
            waterTween.Kill();
        }
        else
        {
            currentSpeed *= 0.75f;
            isWater = true;
        }
        
        waterTween = DOVirtual.DelayedCall(2f, () => currentSpeed /= 0.75f, false)
            .OnComplete(() => isWater = false);
        
        particlePoolManager.GetFromPool<ParticlePool>("WaterEffect", transform.position, Quaternion.identity);

    }
    
    public void TakeLightning(float damage)
    {
        EnemyBase[] enemies = GameObject.FindObjectsOfType<EnemyBase>();

        List<EnemyBase> sortedEnemies = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .ToList();
        sortedEnemies.RemoveAt(0); // 자기자신

        DOVirtual.DelayedCall(0.1f, () =>
        {
            for (int i = 0; i < Mathf.Clamp(sortedEnemies.Count, 0, 2); i++)
            {
                sortedEnemies[i].TakeDamage(damage, Color.yellow);
                GenerateLightning(sortedEnemies[i].transform);
            }
        }, false);
    }

    private void GenerateLightning(Transform target)
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        
        //위치
        Vector3 position = (currentPosition + targetPosition) / 2;
        
        //각도
        Vector3 direction = targetPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        //거리
        float distance = Vector3.Distance(currentPosition, targetPosition);
        
        var particle = particlePoolManager.GetFromPool<ParticlePool>("LightningEffect", position, rotation).gameObject;
        particle.transform.localScale = new Vector3(distance, distance, 1f);
    }

    public void TakePoison(float damage)
    {
        if (isPoison) return;
        isPoison = true;
        
        poisonTween = DOVirtual.DelayedCall(0.2f, () => TakeDamage(damage, Color.magenta), false).SetLoops(-1);
    }

    public virtual void OnCreatedInPool()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnGettingFromPool()
    {
        throw new System.NotImplementedException();
    }
}
