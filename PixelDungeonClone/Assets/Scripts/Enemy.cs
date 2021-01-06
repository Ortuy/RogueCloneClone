using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ActionState { WAITING, ACTIVE }

public enum AIState { SLEEPING, UNALERTED, ALERTED, AMOK }

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Entity
{
    /**
    [Header("Stats")]
    [SerializeField]
    private int level;
    [SerializeField]
    private int xpDrop;
    //[SerializeField]
    //private int strength;
    [SerializeField]
    private float health, maxHealth;
    [SerializeField]
    private float evasion;
    [SerializeField]
    private float accuracy;
    [SerializeField]
    private int minDefence, maxDefence;
    //Minimum and maximum damage dealt. INCLUSIVE
    [SerializeField]
    private int minBaseDamage, maxBaseDamage;
    **/
    [Header("Other")]
    [SerializeField]
    private int xpDrop;
    public SpriteRenderer mapImage;

    public ActionState actionState = ActionState.WAITING;

    [SerializeField]
    private ParticleSystem hitFX, sleepFX, alertFX;

    [SerializeField]
    private ParticleSystem deathFX;

    [SerializeField]
    private Slider healthBar;

    [SerializeField]
    public AIState behaviourState;

    private Vector3 lastPlayerPosition;

    [SerializeField]
    public float awakeRange, alertRange;

    public int turnCost;

    private SpriteRenderer spriteRenderer;

    private Animator animator;

    [SerializeField]
    private AudioSource hitPlayer;
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip hitSound, trueDamageSound, deathSound, alertSound;
    [SerializeField]
    private AudioClip[] footsteps;

    [Header("Movement")]
    private Rigidbody2D body;

    public List<Vector2> path;

    public float movementSpeed;

    private int currentPathIndex;

    private bool pathChangeQueued;

    private Vector2 targetPos;

    public Entity attackTarget;

    [Header("Drops")]
    [SerializeField]
    private Vector2Int[] dropPercentRanges;

    [SerializeField]
    private Item[] drops;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        path = null;
        health = maxHealth;
        statusEffects = new List<StatusEffect>();
        behaviourState = AIState.SLEEPING;
        sleepFX.Play();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = -Mathf.FloorToInt(transform.position.y + 0.5f);
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void DoTurn()
    {
        
        switch (behaviourState)
        {
            case AIState.SLEEPING:
                //Debug.Log("Sleeping");
                DoSleepingTurn();
                break;
            case AIState.UNALERTED:
                //Debug.Log("Unalerted");
                DoUnalertedTurn();
                break;
            case AIState.ALERTED:
                //Debug.Log("Alerted");
                DoAlertedTurn();
                break;
            case AIState.AMOK:
                DoAmokTurn();
                break;
        }
    }

    private void DoSleepingTurn()
    {
        //If the condition is met, wake up and get alerted
        
        if(!Player.stats.invisible && Vector2.Distance(transform.position, Player.instance.transform.position) <= awakeRange)
        {
            if(Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
            {
                Debug.Log("Wake up!");
                sleepFX.Stop();
                alertFX.Play();
                behaviourState = AIState.ALERTED;
                PlaySound(alertSound);
            }
        }

        TurnManager.instance.PassToNextEnemy();
    }

    private void DoUnalertedTurn()
    {
        //If the condition is met, get alerted
        if (!Player.stats.invisible && Vector2.Distance(transform.position, Player.instance.transform.position) <= alertRange)
        {
            if(Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
            {
                Debug.Log("I see you!");
                sleepFX.Stop();
                alertFX.Play();
                behaviourState = AIState.ALERTED;
                PlaySound(alertSound);
            }
            
        }

        //If not, roam (this will need procedural generation to be done)

        TurnManager.instance.PassToNextEnemy();
    }

    private void DoAlertedTurn()
    {
        if(!Player.stats.invisible && Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
        {
            lastPlayerPosition = Player.instance.transform.position;

            if (Vector2.Distance(transform.position, Player.instance.transform.position) < 1.5f && actionState == ActionState.WAITING)
            {
                Debug.LogWarning("Queueing attack");
                var baseDamage = Random.Range(minBaseDamage, maxBaseDamage + 1);
                actionState = ActionState.ACTIVE;
                StartCoroutine(AttackPlayer(baseDamage, 1));
                
            }
            else if (path == null && actionState == ActionState.WAITING)
            {
                QueueMovement(Player.instance.transform.position);
                MoveOnPath();
            }
            else if(path != null)
            {
                MoveOnPath();
            }
        }

        //If they aren't visible, move to their last known position and then search for them
        else
        {
            if (path == null && actionState == ActionState.WAITING && transform.position != lastPlayerPosition)
            {
                QueueMovement(lastPlayerPosition);
                MoveOnPath();
            }
            else if(path != null)
            {
                MoveOnPath();
            }
            //If they are not found, become unalerted
            else if(Player.stats.invisible || !Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
            {
                behaviourState = AIState.UNALERTED;
            }
        }

        if (actionState == ActionState.WAITING)
        {
            Debug.Log("Passing");
            TurnManager.instance.PassToNextEnemy();
        }
    }

    public void GoAmok(Entity target)
    {
        behaviourState = AIState.AMOK;
        attackTarget = target;
    }

    private void DoAmokTurn()
    {
        if (Vector2.Distance(transform.position, attackTarget.transform.position) < 1.5f && actionState == ActionState.WAITING)
        {
            var baseDamage = Random.Range(minBaseDamage, maxBaseDamage + 1);
            Debug.Log(actionState);
            StartCoroutine(AttackTarget(baseDamage, 1));
        }
        else if (path == null && actionState == ActionState.WAITING)
        {
            QueueMovement(attackTarget.transform.position);
            MoveOnPath();
        }
        else
        {
            MoveOnPath();
        }

        if (actionState == ActionState.WAITING)
        {
            Debug.Log("Passing");
            if(Random.Range(0, 6) == 0)
            {
                behaviourState = AIState.ALERTED;
            }
            TurnManager.instance.PassToNextEnemy();
        }
    }

    public void QueueMovement(Vector2 target)
    {
        if (path == null)
        {
            path = Pathfinding.instance.FindPath(transform.position, target);
            currentPathIndex = 0;
        }
        else
        {
            pathChangeQueued = true;
        }
    }

    private void MoveOnPath()
    {
        animator.speed = 1;
        
        if (path != null)
        {
            actionState = ActionState.ACTIVE;

            targetPos = path[currentPathIndex];

            if (Pathfinding.instance.FindPlayerOnTile(targetPos) || Pathfinding.instance.FindAnotherEnemyOnTile(path[currentPathIndex], this))
            {
                Debug.Log("Player!");
                actionState = ActionState.WAITING;
                path = null;
                currentPathIndex = 0;
            }
            else
            {
                spriteRenderer.sortingOrder = -Mathf.FloorToInt(transform.position.y + 0.5f);
                Vector2 tempPos = new Vector2(transform.position.x, transform.position.y);
                if (Vector2.Distance(tempPos, targetPos) > 0.09f)
                {
                    Debug.Log("SettingVelocity");
                    Vector2 movementDir = (targetPos - new Vector2(transform.position.x, transform.position.y)).normalized;

                    if (movementDir.x != 0)
                    {
                        animator.SetFloat("MoveDirection", movementDir.x);
                    }

                    if (!animator.GetBool("Moving"))
                    {
                        animator.SetBool("Moving", true);
                    }

                    body.velocity = movementDir * movementSpeed;
                }
                else
                {
                    Debug.Log("FoundTile");
                    transform.position = targetPos;
                    if (pathChangeQueued)
                    {
                        path = Pathfinding.instance.FindPath(transform.position, GridTester.GetMouseWorldPosition());
                        currentPathIndex = 0;
                        pathChangeQueued = false;
                    }
                    else
                    {
                        currentPathIndex++;
                        if (currentPathIndex >= path.Count || Pathfinding.instance.FindEnemyOnTile(path[currentPathIndex]))
                        {
                            StopMovement();
                        }
                    }

                    if(currentPathIndex != 1)
                    {
                        actionState = ActionState.WAITING;
                        body.velocity = Vector2.zero;
                        animator.SetBool("Moving", false);
                        //animator.speed = 0;
                    }                    
                }
            }
        }
        else
        {
            actionState = ActionState.WAITING;
            body.velocity = Vector2.zero;
            animator.SetBool("Moving", false);
            //animator.speed = 0;
        }
    }

    public void StopMovement()
    {
        path = null;
        currentPathIndex = 0;
        body.velocity = Vector2.zero;
        actionState = ActionState.WAITING;
        //animator.speed = 0;
    }

    IEnumerator AttackPlayer(int damage, int cost)
    {
        StopMovement();
        

        var dir = transform.position.x - Player.instance.transform.position.x;

        if (dir != 0)
        {
            animator.SetFloat("MoveDirection", -dir);
        }

        animator.speed = 1;
        animator.Play("Attack");
        actionState = ActionState.ACTIVE;       
        yield return new WaitForSeconds(0.5f);
        Debug.LogWarning("Attackin");
        Player.stats.TakeDamage(damage + dmgModifier, accuracy + accModifier, transform.position);
        for (int i = Player.stats.statusEffects.Count - 1; i >= 0; i--)
        {
            if (Player.stats.statusEffects[i].effectID == 3)
            {
                Player.stats.EndStatusEffect(i);
            }
        }
        turnCost = cost;
        TurnManager.instance.PassToNextEnemy();
        actionState = ActionState.WAITING;
    }

    IEnumerator AttackTarget(int damage, int cost)
    {
        StopMovement();
        if (attackTarget.gameObject.activeInHierarchy)
        {
            
            var dir = transform.position.x - attackTarget.transform.position.x;

            if (dir != 0)
            {
                animator.SetFloat("MoveDirection", -dir);
            }
            animator.speed = 1;
            animator.Play("Attack");
            actionState = ActionState.ACTIVE;
            yield return new WaitForSeconds(0.5f);
            Debug.LogWarning("Attackin");
            attackTarget.TakeDamage(damage + dmgModifier, accuracy + accModifier, transform.position);
            for (int i = attackTarget.statusEffects.Count - 1; i >= 0; i--)
            {
                if (attackTarget.statusEffects[i].effectID == 3)
                {
                    attackTarget.EndStatusEffect(i);
                }
            }
            turnCost = cost;
            TurnManager.instance.PassToNextEnemy();
            actionState = ActionState.WAITING;
        }
        else
        {
            yield return null;
            turnCost = cost;
            TurnManager.instance.PassToNextEnemy();
            actionState = ActionState.WAITING;
        }
        
    }

    public override void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos)
    {
        //Evasion chance
        int evasionPercent = Mathf.FloorToInt((((evasion + evaModifier) - attackerAccuracy) / ((evasion + evaModifier) + 10)) * 100);
        Debug.Log("Enemy evasion chacne: " + evasionPercent + "%");
        int evasionRoll = Random.Range(1, 101);
        if(evasionRoll <= evasionPercent)
        {
            Debug.Log("Dodge!");
            ShowDamageText("Miss!");
        }
        else
        {
            if (!healthBar.gameObject.activeInHierarchy)
            {
                healthBar.gameObject.SetActive(true);
            }
            PlayHitSound(hitSound);
            var hitDirection = (transform.position - attackerPos).normalized;
            var angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
            hitFX.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
            hitFX.Play();

            var defence = Random.Range(minDefence, maxDefence + 1);
            var totaldmg = damage - (defence + defModifier);
            if (totaldmg < 1)
            {
                totaldmg = 1;
            }

            ShowDamageText(totaldmg.ToString());

            health -= totaldmg;
            float value = (health / maxHealth);
            healthBar.value = value;

            if (health <= 0)
            {
                Debug.Log("ENEMY DEATH!");
                Die(angle - 90);
            }
        }        
    }

    public override void TakeTrueDamage(int damage)
    {
        if (!healthBar.gameObject.activeInHierarchy)
        {
            healthBar.gameObject.SetActive(true);
        }

        

        //var hitDirection = (transform.position - attackerPos).normalized;
        //var angle = Mathf.Atan2(hitDirection.y, hitDirection.x);
        //hitFX.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
        hitFX.Play();
        PlayHitSound(trueDamageSound);

        //var defence = Random.Range(minDefence, maxDefence + 1);
        //var totaldmg = damage - defence;
        //if (totaldmg < 1)
        //{
        //    totaldmg = 1;
        //}

        ShowDamageText(damage.ToString());

        health -= damage;
        float value = (health / maxHealth);
        healthBar.value = value;

        if (health <= 0)
        {
            Debug.Log("ENEMY DEATH!");
            Die(-90);
        }
    }

    private void Die(float angle)
    {
        TurnManager.instance.enemies.Remove(this);
        Instantiate(deathFX, transform.position, Quaternion.AngleAxis(angle - 45, Vector3.forward)).GetComponent<ParticleSystem>().Play();

        SpawnManager.instance.PlaySound(deathSound);

        int randomDrop = Random.Range(0, 100);
        Debug.Log(randomDrop);
        for (int i = 0; i < dropPercentRanges.Length && i < drops.Length; i++)
        {
            if (randomDrop >= dropPercentRanges[i].x && randomDrop <= dropPercentRanges[i].y)
            {
                ItemPickup temp = Instantiate(InventoryManager.instance.itemTemplate, transform.position, Quaternion.identity);
                temp.SetItem(new ItemInstance(drops[i], 1));
                break;
            }
        }

        int xpModifier = 0;

        if (InventoryManager.instance.ringEquipped[0] == 7)
        {
            int chance = 40 + (10 * InventoryManager.instance.inventoryItems[2].baseStatChangeMax);
            if (InventoryManager.instance.inventoryItems[2].cursed)
            {
                if(Random.Range(0, 100) < chance)
                {
                    xpModifier -= InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
                }
            }
            else
            {
                if (Random.Range(0, 100) < chance)
                {
                    xpModifier += InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
                }
            }
        }
        if (InventoryManager.instance.ringEquipped[1] == 7)
        {
            int chance = 40 + (10 * InventoryManager.instance.inventoryItems[3].baseStatChangeMax);
            if (InventoryManager.instance.inventoryItems[3].cursed)
            {
                if (Random.Range(0, 100) < chance)
                {
                    xpModifier -= InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
                }
            }
            else
            {
                if (Random.Range(0, 100) < chance)
                {
                    xpModifier += InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
                }
            }
        }

        Player.stats.AddXP(xpDrop+xpModifier);

        ResetState();

        if(gameObject.CompareTag("SpiritJar"))
        {
            FindObjectOfType<SpawnManager>().SpawnEnemy(3, transform.position);
        }

        gameObject.SetActive(false);       
    }

    public void ResetState()
    {
        health = maxHealth;
        behaviourState = AIState.SLEEPING;
        sleepFX.Play();
    }

    public void PlayHitSound(AudioClip clip)
    {
        hitPlayer.clip = clip;
        hitPlayer.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayFootstep()
    {
        var rand = Random.Range(0, footsteps.Length);
        PlaySound(footsteps[rand]);
    }
}
