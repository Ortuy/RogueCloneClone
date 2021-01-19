using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public Enemy enemy;
    public Transform beam;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    public void TrackTarget()
    {
        var targetPos = enemy.attackTarget.transform.position + new Vector3(0, 0.5f, 0);

        var hitDirection = (targetPos - beam.position).normalized;
        var angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;

        beam.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        beam.localScale = new Vector3(0.5f, (targetPos - transform.position).magnitude + 0.5f, 0.5f);
    }
}
