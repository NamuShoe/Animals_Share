using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlePool : MonoBehaviour
{
    private PoolManager particlePoolManager;
    
    [SerializeField] private float duration;
    [SerializeField] private string name;
    private ParticleSystem particle;

    private void Awake()
    {
        particlePoolManager = GameObject.Find("ParticlePoolManager").GetComponent<PoolManager>();
        particle = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        particle.Play();
        
        if(duration > 0)
            DOVirtual.DelayedCall(duration, () => particlePoolManager.TakeToPool<ParticlePool>(name, this), false);
    }

    private void OnDisable()
    {
        particle.Stop();
    }

    private void Reset()
    {
        duration = GetComponent<ParticleSystem>().main.startLifetime.constant;
        name = gameObject.name;
    }
}
