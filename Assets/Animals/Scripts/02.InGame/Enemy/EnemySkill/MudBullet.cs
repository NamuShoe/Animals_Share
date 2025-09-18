using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class MudBullet : EnemyProjectile
{
    private NutriaBossController nutriaBossController;
    public void SetNutriaBossController(NutriaBossController NBC) => nutriaBossController = NBC;

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            nutriaBossController.Blind(true);
            isCollide = true;
            nutriaBossController.TakeMudBullet(this);
        }
    }
    
    protected virtual void OnBecameInvisible()
    {
        if(!isCollide)
            nutriaBossController.TakeMudBullet(this);
    }
}
