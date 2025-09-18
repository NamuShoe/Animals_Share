using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelProjectile : EnemyProjectile {
    public ExplosiveEnemyProjectile parentProjectile;
    
    protected override void Movement()
    {
        base.Movement();
    }

    protected override void TakeEnemyProjectileToPool()
    {
        parentProjectile.TakeShrapnelProjectile(this);
    }
}
