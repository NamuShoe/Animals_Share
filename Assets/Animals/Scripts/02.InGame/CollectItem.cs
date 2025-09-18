using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redcode.Pools;
using DG.Tweening;

public class CollectItem : MonoBehaviour, IPoolObject
{
    [SerializeField] private RectTransform target;
    private PoolManager particlePoolManager;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        //target = GameObject.Find("Gold").GetComponent<RectTransform>();
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void ExplosionItem(Vector2 from, float range)
    {
        transform.position = from;
        Sequence sequence = DOTween.Sequence().SetAutoKill(true);
        sequence.Append(transform.DOMove(from + Random.insideUnitCircle * range, 0.25f).SetEase(Ease.OutCubic));
        sequence.Append(transform.DOMove(target.position, 0.5f).SetEase(Ease.InCubic));
        sequence.AppendCallback(() => particlePoolManager.TakeToPool<CollectItem>(this));
    }

    public void PopItem(Vector2 from, float distance, Sprite sprite)
    {
        transform.position = from;
        spriteRenderer.sprite = sprite;

        transform.DOMove(from + Vector2.up * distance, 0.25f).SetEase(Ease.OutBack)
            .OnComplete(() => particlePoolManager.TakeToPool<CollectItem>(this));
    }
    
    public void OnCreatedInPool()
    {

    }

    public void OnGettingFromPool()
    {
        
    }
}
