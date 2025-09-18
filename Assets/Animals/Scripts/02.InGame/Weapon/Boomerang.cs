using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Boomerang : Projectile
{
    private bool isStraight = false;
    
    Tween rotateTween = null;
    Tweener returnTweener = null;
    Vector3[] path = new Vector3[3] { new Vector3(1, 2), new Vector3(0, 3), new Vector3(-1, 2) };

    private AudioSource _audioSource;
    private float volume = 0f;

    protected override void Awake()
    {
        base.Awake();
        // _audioSource = GetComponent<AudioSource>();
        volume = PlayerPrefs.GetFloat("SFXSlider", 0.75f);
    }

    protected override void Init()
    {
        isStraight = weaponController.isEpic;
        // _audioSource.volume = 0;
        // _audioSource.Play();
        base.Init();
    }

    private void Start()
    {
        SkillManager.instance.RemoveSkill("관통력 증가");
    }

    // protected override void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.gameObject.CompareTag("Enemy") && this.gameObject.activeSelf)
    //     {
    //         var enemyController = other.gameObject.GetComponent<EnemyBase>();
    //         
    //         //충돌 이펙트
    //         var collisionPoint = other.ClosestPoint(Vector2.zero);
    //         particlePoolManager.GetFromPool<ParticlePool>("CollisionEffect", collisionPoint, Quaternion.identity);
    //         
    //         //데미지
    //         enemyController.TakeDamage(damage);
    //         if(isFire) enemyController.TakeFire(normalDamage * firePercent);
    //         if(isWater) enemyController.TakeWater(waterPercent);
    //         if(isLightning) enemyController.TakeLightning(normalDamage * lightningPercent);
    //         if(isPoison) enemyController.TakePoison(normalDamage * poisonPercent);
    //     }
    // }

    protected override void OnDisable()
    {
        base.OnDisable();

        rotateTween.Pause();
        rotateTween.Kill();
        returnTweener.Pause();
        returnTweener.Kill();
        
        // _audioSource.Stop();
    }

    protected override void Pierce()
    {
        // 관통을 무시를 위한 빈 함수
    }

    protected override void Movement()
    {
        if(isStraight)
            StraightMovement();
        else
            CurvedMovement();
    }

    private void StraightMovement()
    {
        moveTween = transform.DOMove(transform.up * 3, duration)
            .SetEase(Ease.OutSine).SetRelative(true);
        moveTween.OnComplete(ReturnMovement);

        RotateMovement();
    }

    private void CurvedMovement()
    {
        moveTween = transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.Linear).SetRelative(true);
        moveTween.OnComplete(ReturnMovement);

        RotateMovement();
    }

    private void RotateMovement()
    {
        rotateTween = transform.GetChild(0).DORotate(new Vector3(0, 0, 360f * 15),
                duration + 0.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
        
        //SoundManager.instance.PlayOneShot(SoundManager.GAME_SFX.BoomerangSpin);
        // _audioSource.DOFade(volume, (duration + 0.5f) / 2).SetEase(Ease.InExpo);
        // DOVirtual.DelayedCall((duration + 0.5f) / 2,
        //     () => _audioSource.DOFade(0, (duration + 0.5f) / 2).SetEase(Ease.InExpo));
    }

    private void ReturnMovement()
    {
        returnTweener = transform.DOMove(PlayerController.instance.transform.position, 0.5f).SetEase(Ease.Linear);
        returnTweener.OnUpdate(() =>
        {
            var position = PlayerController.instance.transform.position;
            returnTweener.ChangeEndValue(position,
                returnTweener.Duration() - returnTweener.Elapsed(), true);
        });
        DOVirtual.DelayedCall(0.5f, () => WeaponController.instance.TakeProjectile(this));

        // isCollide = true;
        // WeaponController.instance.TakeProjectile(this);
    }

    protected override void OnBecameInvisible()
    {
        return;
    }
}
