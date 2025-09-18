using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceEnemyProjectile : EnemyProjectile {

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected override void Movement()
    {
        
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (other.gameObject.CompareTag("Wall")) {
            Reflect();
        }
    }

    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        rigidbody2D.AddForce(force, mode);
    }

    private void Reflect()
    {
        var velocity = rigidbody2D.velocity;
        var reflectVector = new Vector2(-velocity.x, velocity.y);
        velocity = Vector2.zero;
        rigidbody2D.velocity = velocity;
        rigidbody2D.AddForce(reflectVector, ForceMode2D.Impulse);
    }
}
