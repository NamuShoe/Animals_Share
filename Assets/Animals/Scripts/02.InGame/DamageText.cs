using System;
using DG.Tweening;
using Redcode.Pools;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour, IPoolObject
{
    private PoolManager particlePoolManager;
    private TextMeshPro textMeshPro;
    private Sequence seq;

    private void Awake()
    {
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        textMeshPro = this.GetComponent<TextMeshPro>();
    }

    private void OnDisable()
    {
        seq.Pause();
        seq.Kill();
    }

    public void PopDamageText(float damageAmount, Color color)
    {
        transform.localScale = Vector3.zero;
        textMeshPro.text = damageAmount.ToString();
        textMeshPro.color = color;
        textMeshPro.alpha = 0f;
        
        seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1f, 2f)).SetEase(Ease.OutBack)
            .Join(textMeshPro.DOFade(1f, 2f))
            .Append(transform.DOMove(Vector2.up * 0.5f, 1.0f).SetRelative())
            .Insert(2.5f, textMeshPro.DOFade(0f, 0.5f))
            .AppendCallback(() => particlePoolManager.TakeToPool<DamageText>(this));
        // transform.DOMove(from + Vector2.up * distance, 0.5f).SetEase(Ease.OutCirc)
        //     .OnComplete(() => particlePoolManager.TakeToPool<DamageText>(this));

    }
    
    public void OnCreatedInPool()
    {
        
    }

    public void OnGettingFromPool()
    {
        
    }
}
