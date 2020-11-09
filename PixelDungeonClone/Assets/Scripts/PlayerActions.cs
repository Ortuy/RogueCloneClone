using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActions : MonoBehaviour
{
    private bool attackQueued;

    private GameObject attackTarget;

    private Rigidbody2D body;

    public static PlayerActions instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        body = GetComponent<Rigidbody2D>();
    }  

    // Update is called once per frame
    void Update()
    {
        //DoPlayerTurn();
    }

    public void DoPlayerTurn()
    {
        /**
        if (Input.GetMouseButtonDown(0))
        {
            if (Pathfinding.instance.FindEnemyOnTile(GridTester.GetMouseWorldPosition()))
            {
                attackQueued = true;
                attackTarget = FindEnemy(GridTester.GetMouseWorldPosition()).gameObject;
            }
            else
            {
                attackQueued = false;
                attackTarget = null;
            }
        }**/

        if (attackTarget != null)
        {
            var distance = Vector2.Distance(transform.position, attackTarget.transform.position);
            if (distance <= 1.5f && attackQueued && body.velocity == Vector2.zero)
            {
                Destroy(attackTarget);
                PlayerMovement.instance.StopMovement();
                attackTarget = null;
            }

            TurnManager.instance.SwitchTurn(TurnState.ENEMY);
        }
    }

    public void QueueAttack(bool targetFound)
    {
        if (targetFound)
        {
            attackQueued = true;
            attackTarget = FindEnemy(GridTester.GetMouseWorldPosition()).gameObject;
        }
        else
        {
            attackQueued = false;
            attackTarget = null;
        }
    }

    public Collider2D FindEnemy(Vector2 target)
    {
        var enemy = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Enemies"));

        return enemy;
    }

    public List<Collider2D> FindEnemiesNearby(Vector2 target, float range)
    {
        List<Collider2D> enemyColliders = Physics2D.OverlapCircleAll(target, range).ToList();

        return enemyColliders;
    }
}
