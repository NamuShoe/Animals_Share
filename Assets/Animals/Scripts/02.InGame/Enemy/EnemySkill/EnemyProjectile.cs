using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour {
    [SerializeField] private string idName;
    
    public float attackPower;
    protected bool isCollide = false;
    
    protected Rigidbody2D rigidbody2D;
    protected Tweener tweener;
    protected Coroutine coroutine;
    
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        tweener.Pause();
        tweener.Kill();
        if(coroutine != null) StopCoroutine(coroutine);
    }

    private void Init()
    {
        isCollide = false;
        Movement();
    }

    protected virtual void Movement()
    {
        tweener = transform.DOMove(-transform.up * 10f, 2.5f).SetEase(Ease.Linear).SetRelative(true)
            .SetLoops(-1, LoopType.Incremental);
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            isCollide = other.GetComponent<PlayerController>().TakeDamage(attackPower);
            if (isCollide)
                TakeEnemyProjectileToPool();
        }
    }
    
    protected virtual void OnBecameInvisible()
    {
        if (!isCollide)
            TakeEnemyProjectileToPool();
    }

    protected virtual void TakeEnemyProjectileToPool()
    {
        EnemyManager.instance.TakeEnemyProjectileToPool(idName, this);
    }
}
