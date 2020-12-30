using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActions : MonoBehaviour
{
    private bool attackQueued, pickUpQueued;

    private Enemy attackTarget;

    private Rigidbody2D body;

    public int turnCost;

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
                StartCoroutine(Attack(attackTarget, baseDamage, 1));            
            }         
        }
        //Queued item pickups
        if(pickUpQueued)
        {
            PickUpItem();
        }
    }

    IEnumerator Attack(Enemy target, int damage, int cost)
    {
        Player.movement.StopMovement();
        Player.instance.actionState = ActionState.ACTIVE;        
        yield return new WaitForSeconds(0.5f);
        target.TakeDamage(damage + Player.stats.dmgModifier, Player.stats.GetAccuracy() + Player.stats.accModifier, transform.position);

        //Ends all invisibility effects
        for(int i = Player.stats.statusEffects.Count - 1; i >= 0; i--)
        {
            if(Player.stats.statusEffects[i].effectID == 1)
            {
                Player.stats.EndStatusEffect(i);
            }
        }
        for (int i = target.statusEffects.Count - 1; i >= 0; i--)
        {
            if (target.statusEffects[i].effectID == 3)
            {
                target.EndStatusEffect(i);
            }
        }

        attackQueued = false;
        attackTarget = null;
        Player.instance.actionState = ActionState.WAITING;
        turnCost = cost;
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

    public void QueueItemPickup()
    {
        pickUpQueued = true;
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
            MouseBlocker.mouseBlocked = false;
            pickUpQueued = false;
            if(!Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items")))
            {
                UIManager.instance.pickUpButton.gameObject.SetActive(false);
            }      
            else
            {
                UIManager.instance.pickUpButton.gameObject.SetActive(true);
            }
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
