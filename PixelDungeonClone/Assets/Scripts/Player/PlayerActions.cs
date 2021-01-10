using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActions : MonoBehaviour
{
    private bool attackQueued, pickUpQueued, interactionQueued;

    public bool throwQueued;
    public ItemInstance itemToThrow;

    private Enemy attackTarget;
    private InteractibleObject interactionTarget;

    private Rigidbody2D body;

    public int turnCost;
    public GameObject popupText;
    private Animator animator;
    public int attackExtraTurnCost = 0;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip itemDropSound, itemThrowSound, itemPickupSound, mapSound;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }  

    public void DoPlayerTurn()
    {
        //Attacking:
        if (attackTarget != null)
        {
            var distance = Vector2.Distance(transform.position, attackTarget.transform.position);
            if (distance <= 1.5f && attackQueued && body.velocity == Vector2.zero && Player.instance.actionState == ActionState.WAITING && Player.stats.GetHealth() > 0)
            {
                var baseDamage = Player.stats.GetRandomDamageValue();
                StartCoroutine(Attack(attackTarget, baseDamage, 1));            
            }         
        }
        if(interactionTarget != null)
        {
            var distance = Vector2.Distance(transform.position, interactionTarget.transform.position);
            if (distance <= 1.5f && interactionQueued && body.velocity == Vector2.zero && Player.stats.GetHealth() > 0)
            {
                StartCoroutine(StartInteraction());
                interactionQueued = false;                
            }
        }
        //Queued item pickups
        if(pickUpQueued)
        {
            PickUpItem();
        }
    }

    IEnumerator StartInteraction()
    {
        Debug.Log(interactionTarget);
        yield return new WaitForSeconds(0.2f);
        
        interactionTarget.StartInteraction();
    }

    public void ThrowItem(Vector2 destination)
    {
        //StartCoroutine(InstantiateThrownItem(destination));
        var thrown = Instantiate(InventoryManager.instance.itemTemplate, transform.position, Quaternion.identity);
        thrown.SetItem(itemToThrow);
        thrown.SetThrown(destination);
        PlaySound(itemThrowSound);
        UIManager.instance.inventoryButton.interactable = true;
        throwQueued = false;
    }

    IEnumerator InstantiateThrownItem(Vector2 destination)
    {
        yield return null;
        var thrown = Instantiate(InventoryManager.instance.itemTemplate, transform.position, Quaternion.identity);
        thrown.SetItem(itemToThrow);
        thrown.SetThrown(destination);
        PlaySound(itemThrowSound);
        UIManager.instance.inventoryButton.interactable = true;
    }

    IEnumerator Attack(Enemy target, int damage, int cost)
    {
        var dir = transform.position.x - target.transform.position.x;

        if(dir != 0)
        {
            animator.SetFloat("MoveDirection", -dir);
        }

        Player.movement.StopMovement();
        animator.speed = 1;
        animator.Play("Attack");
        Player.instance.actionState = ActionState.ACTIVE;        
        yield return new WaitForSeconds(0.5f);

        int bonus = 0;
        if(InventoryManager.instance.ringEquipped[0] == 4)
        {
            var tempbonus = Mathf.RoundToInt(((Player.stats.GetStrength() - 10) / 2) * InventoryManager.instance.inventoryItems[2].baseStatChangeMax);
            if(InventoryManager.instance.inventoryItems[2].cursed)
            {
                tempbonus = -tempbonus;
            }
            bonus += tempbonus;
        }
        if (InventoryManager.instance.ringEquipped[1] == 4)
        {
            var tempbonus = Mathf.RoundToInt(((Player.stats.GetStrength() - 10) / 2) * InventoryManager.instance.inventoryItems[3].baseStatChangeMax);
            if (InventoryManager.instance.inventoryItems[3].cursed)
            {
                tempbonus = -tempbonus;
            }
            bonus += tempbonus;
        }

        target.TakeDamage(damage + Player.stats.dmgModifier + bonus, Player.stats.GetAccuracy() + Player.stats.accModifier, transform.position);

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

        Player.stats.invisible = false;

        attackQueued = false;
        attackTarget = null;
        Player.instance.actionState = ActionState.WAITING;
        turnCost = cost + attackExtraTurnCost;
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

    public void QueueInteraction(InteractibleObject interactible)
    {
        interactionQueued = true;
        attackQueued = false;
        interactionTarget = interactible;
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
            var foundObject = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items"));
            ItemPickup itemPickup = foundObject.GetComponent<ItemPickup>();
            GoldPickup goldPickup = foundObject.GetComponent<GoldPickup>();

            if(foundObject.CompareTag("Map"))
            {
                //UIManager.instance.mapButton.gameObject.SetActive(true);
                GameManager.instance.mapRevealed = true;
                InventoryManager.instance.potionUseFX[10].Play();
                PlaySound(mapSound);
                Destroy(foundObject.gameObject);
            }
            if (itemPickup != null)
            {
                InventoryManager.instance.AddItem(itemPickup.itemInside, out bool itemDelivered);
                if (itemDelivered)
                {
                    ShowItemText(itemPickup.itemInside.itemName);
                    Destroy(itemPickup.gameObject);
                    PlaySound(itemPickupSound);
                }
                else
                {
                    PlaySound(itemDropSound);
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
            var item = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items"));
            if (item == null)
            {
                UIManager.instance.pickUpButton.gameObject.SetActive(false);
            }      
            else
            {
                UIManager.instance.pickUpButton.gameObject.SetActive(true);
                UIManager.instance.itemPickupImage.sprite = item.GetComponent<ItemPickup>().itemInside.itemImage;
            }
        }        
    }

    protected void ShowItemText(string text)
    {
        var popup = Instantiate(popupText, transform.position, Quaternion.identity).GetComponent<TextMesh>();
        popup.text = text;
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

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
