using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemyController : EnemyController
{
    protected override void SetSpawnPosition()
    {
        //스폰 위치
        List<RectTransform> spawnPositions = EnemyManager.instance.spawnPosition;
        RectTransform spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];
        transform.position = new Vector3(spawnPosition.position.x, spawnPosition.position.y, 0f);

        //방향
        Vector3 dirVector;
        if (playerController.gameObject == null)
            dirVector = new Vector3(0, -6, 0) - transform.position;
        else
            dirVector = playerController.gameObject.transform.position - transform.position - new Vector3(0, 3, 0);
        float angle = Mathf.Atan2(dirVector.y, dirVector.x) * Mathf.Rad2Deg + 90.0f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    protected override void Movement()
    {
        //이동
        Vector2 nextVector = -transform.up.normalized * (Speed * Time.fixedDeltaTime);
        rigidbody.MovePosition(rigidbody.position + nextVector);
    }
}
