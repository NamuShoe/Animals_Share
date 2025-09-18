using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GoldbarController : PlayerController
{
    [Space(15f)]
    [SerializeField] Slider jetPackSlider;
    [SerializeField] float jetPackAmount = 0f;

    [SerializeField] private float jetPackDamage = 20f;
    [SerializeField] private float jetPackDuration = 5.0f;
    [SerializeField] bool isJetPackRunning = false;

    public override void AddEXP(int _exp) //현 상황상 enemy를 처치 시 실행되는 유일한 함수
    {
        base.AddEXP(_exp);
        if(!isJetPackRunning)
            AddJetPack(5);
    }
    
    public void AddJetPack(int _jetPack)
    {
        jetPackAmount += _jetPack;
        if (jetPackAmount >= 100)
        {
            RunningJetPack();
            jetPackAmount -= 100;
        }
        if(!isJetPackRunning)
            jetPackSlider.value = jetPackAmount / 100;
    }

    protected override void Awake()
    {
        base.Awake();
        jetPackSlider.value = jetPackAmount;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if(!isJetPackRunning)
            base.OnTriggerEnter2D(other);
        else
            if (other.gameObject.CompareTag("Enemy"))
            {
                var enemyController = other.gameObject.GetComponent<EnemyBase>();
                enemyController.TakeDamage(jetPackDamage);
            }
    }

    void RunningJetPack()
    {
        isJetPackRunning = true;
        var tempMoveSpeed = moveSpeed;
        moveSpeed = 25f;
        jetPackSlider.DOValue(0f, jetPackDuration)
            .OnComplete(() =>
            {
                isJetPackRunning = false;
                moveSpeed = tempMoveSpeed;
            });
        
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.5f, 0.5f);
        this.gameObject.GetComponent<SpriteRenderer>().DOColor(new Color(1, 1, 1), jetPackDuration);
    }
}
