using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Redcode.Pools;

public class Projectile : MonoBehaviour
{
    protected WeaponController weaponController;
    protected PoolManager particlePoolManager;
    protected TrailRenderer trailRenderer;
    
    [SerializeField] protected float damage;
    [SerializeField] protected float normalDamage;
    [SerializeField] protected int piercing = 0;
    [SerializeField] protected int currentPiercing;
    protected float duration = 0f;
    private bool isCollide = false;
    private bool isTake = false;
    protected Tween moveTween = null;
    
    [Header("Skill Point")]
    [SerializeField] protected bool isFire = false;
    protected float firePercent = 1f;
    [SerializeField] protected bool isWater = false;
    protected float waterPercent = 0f;
    [SerializeField] protected bool isLightning = false;
    protected float lightningPercent = 1f;
    [SerializeField] protected bool isPoison = false;
    protected float poisonPercent = 1f;
    

    protected virtual void Awake()
    {
        weaponController = WeaponController.instance;;
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    protected virtual void OnEnable()
    {
        Init();
    }

    protected virtual void OnDisable()
    {
        moveTween.Pause();
        moveTween.Kill();
    }

    protected virtual void Init()
    {
        damage = weaponController.AttackPower;
        normalDamage = weaponController.NormalAttackPower;
        piercing = weaponController.PiercingCount;
        currentPiercing = piercing;
        duration = 1 + (1 / weaponController.ProjectileSpeed);
        
        isFire = weaponController.IsFire;
        firePercent = weaponController.firePercent;
        isWater = weaponController.IsWater;
        waterPercent = weaponController.waterPercent;
        isLightning = weaponController.IsLightning;
        lightningPercent = weaponController.lightningPercent;
        isPoison = weaponController.IsPoison;
        poisonPercent = weaponController.poisonPercent;
        
        Movement();
        isCollide = false;
        isTake = false;
    }

    protected virtual void Movement()
    {
        moveTween = transform.DOMove(transform.up * 10f, duration)
            .SetEase(Ease.Linear).SetRelative(true).SetLoops(-1, LoopType.Incremental);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && this.gameObject.activeSelf)
        {
            var enemy = other.gameObject.GetComponent<EnemyBase>();
            if (enemy.isDead) return;
            
            float piercingPercent = GetPiercingPercent(piercing, currentPiercing);

            //충돌 이펙트
            particlePoolManager.GetFromPool<ParticlePool>("CollisionEffect", transform.position, Quaternion.identity);
            
            //데미지
            enemy.TakeDamage(damage * piercingPercent);
            if (isFire || isWater || isLightning || isPoison)
            {
                if (isFire) {
                    enemy.TakeFire((normalDamage * firePercent) * piercingPercent);
                    isFire = false;
                }

                if (isWater) {
                    enemy.TakeWater((normalDamage * waterPercent) * piercingPercent);
                    isWater = false;
                }

                if (isLightning) {
                    enemy.TakeLightning((normalDamage * lightningPercent) * piercingPercent);
                    isLightning = false;
                }

                if (isPoison) {
                    enemy.TakePoison((normalDamage * poisonPercent) * piercingPercent);
                    isPoison = false;}
            }

            Pierce();
        }
    }

    protected virtual void Pierce()
    {
        --currentPiercing;
        if(currentPiercing < 0)
        {
            isCollide = true;
            isTake = true;
            WeaponController.instance.TakeProjectile(this);
        }
    }

    protected virtual float GetPiercingPercent(int max, int current)
    {
        var piercingPercent = (max - current) switch
        {
            0 => 1.0f, 
            1 => 0.75f, 
            2 => 0.5f, 
            _ => 0.25f
        };
        return piercingPercent;
    }

    protected virtual void OnBecameInvisible()
    {
        if (isCollide == false && isTake == false) {
            isTake = true;
            WeaponController.instance.TakeProjectile(this);
        }
        if(trailRenderer != null)
            trailRenderer.Clear();
    }
}
