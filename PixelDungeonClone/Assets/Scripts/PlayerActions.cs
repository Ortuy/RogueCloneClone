using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActions : MonoBehaviour
{
    private bool attackQueued;

    private Enemy attackTarget;

    private Rigidbody2D body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }  

    public void DoPlayerTurn()
    {
        //Attacking:
        if (attackTarget != null)
        {
            var distance = Vector2.Distance(transform.position, attackTarget.transform.position);
            if (distance <= 1.5f && attackQueued && body.velocity == Vector2.zero && Player.instance.actionState == ActionState.WAITING)
            {
                var baseDamage = Player.stats.GetRandomDamageValue();
                StartCoroutine(Attack(attackTarget, baseDamage));            
            }         
        }
    }

    IEnumerator Attack(Enemy target, int damage)
    {
        Player.movement.StopMovement();
        Player.instance.actionState = ActionState.ACTIVE;
        yield return new WaitForSeconds(0.5f);
        target.TakeDamage(damage, Player.stats.getAccuracy(), transform.position);
        attackQueued = false;
        attackTarget = null;
        Player.instance.actionState = ActionState.WAITING;
        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }

    public void QueueAttack(bool targetFound)
    {
        if (targetFound)
        {
            attackQueued = true;
            attackTarget = FindEnemy(GridTester.GetMouseWorldPosition());
        }
        else
        {
            attackQueued = false;
            attackTarget = null;
        }
    }

    public void Pass()
    {
        if(TurnManager.instance.turnState == TurnState.PLAYER)
        {
            TurnManager.instance.SwitchTurn();
        }
    }

    public void PickUpItem()
    {
        if (TurnManager.instance.turnState == TurnState.PLAYER)
        {
            ItemPickup itemPickup = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items")).GetComponent<ItemPickup>();
            GoldPickup goldPickup = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items")).GetComponent<GoldPickup>();

            if (itemPickup != null)
            {
                InventoryManager.instance.AddItem(itemPickup.itemInside, out bool itemDelivered);
                if (itemDelivered)
                {
                    Destroy(itemPickup.gameObject);
                }
            }
            if (goldPickup != null)
            {
                InventoryManager.instance.AddGold(goldPickup.amount);

                Destroy(goldPickup.gameObject);
            }
            TurnManager.instance.SwitchTurn();
            UIManager.instance.pickUpButton.gameObject.SetActive(false);
        }        
    }

    public Enemy FindEnemy(Vector2 target)
    {
        var enemyCollider = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Enemies"));

        if(enemyCollider.TryGetComponent(out Enemy enemy))
        {
            return enemy;
        }
        else
        {
            return null;
        }
        
    }

    public List<Collider2D> FindEnemiesNearby(Vector2 target, float range)
    {
        List<Collider2D> enemyColliders = Physics2D.OverlapCircleAll(target, range).ToList();

        return enemyColliders;
    }
}
