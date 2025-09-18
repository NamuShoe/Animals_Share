using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TrackingEnemyProjectile : EnemyProjectile {
    private Transform target;

    private void Awake()
    {
        target = PlayerController.instance.transform;
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected override void Movement()
    {
        coroutine = StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        var WFFU = new WaitForFixedUpdate();

        while (true) {
            if (target.position.y < transform.position.y) {
                Vector3 dirVector = target.position - transform.position;
                float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
                var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.05f);
            }

            var moveVector = -transform.up * Time.fixedDeltaTime;
            rigidbody2D.MovePosition(transform.position + moveVector);

            yield return WFFU;
        }
    }
}
