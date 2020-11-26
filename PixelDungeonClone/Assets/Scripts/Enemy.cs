using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ActionState { WAITING, ACTIVE }

public enum AIState { SLEEPING, UNALERTED, ALERTED }

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    private int level;
    [SerializeField]
    private int strength;
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

    [Header("Other")]
    public ActionState actionState = ActionState.WAITING;

    [SerializeField]
    private ParticleSystem hitFX;

    [SerializeField]
    private ParticleSystem deathFX;

    [SerializeField]
    private Slider healthBar;

    [SerializeField]
    private AIState behaviourState;

    private Vector3 lastPlayerPosition;

    [SerializeField]
    private float awakeRange, alertRange;

    [Header("Movement")]
    private Rigidbody2D body;

    public List<Vector2> path;

    public float movementSpeed;

    private int currentPathIndex;

    private bool pathChangeQueued;

    private Vector2 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        path = null;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DoTurn()
    {
        switch(behaviourState)
        {
            case AIState.SLEEPING:
                Debug.Log("Sleeping");
                DoSleepingTurn();
                break;
            case AIState.UNALERTED:
                Debug.Log("Unalerted");
                DoUnalertedTurn();
                break;
            case AIState.ALERTED:
                Debug.Log("Alerted");
                DoAlertedTurn();
                break;
        }

        /**
        var distance = Vector2.Distance(transform.position, Player.instance.transform.position);

        if (distance < 1.5f && health > 0 && actionState == ActionState.WAITING)
        {
            var baseDamage = Random.Range(minBaseDamage, maxBaseDamage + 1);
            StartCoroutine(AttackPlayer(baseDamage));
        }
        else if(actionState == ActionState.WAITING)
        {
            TurnManager.instance.PassToNextEnemy();
        }
        **/
    }

    private void DoSleepingTurn()
    {
        //If the condition is met, wake up and get alerted
        if(Vector2.Distance(transform.position, Player.instance.transform.position) <= awakeRange && Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
        {
            behaviourState = AIState.ALERTED;
        }

        TurnManager.instance.PassToNextEnemy();
    }

    private void DoUnalertedTurn()
    {
        //If the condition is met, get alerted
        if (Vector2.Distance(transform.position, Player.instance.transform.position) <= alertRange && Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
        {
            behaviourState = AIState.ALERTED;
        }

        //If not, roam (this will need procedural generation to be done)

        TurnManager.instance.PassToNextEnemy();
    }

    private void DoAlertedTurn()
    {
        if(Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
        {
            lastPlayerPosition = Player.instance.transform.position;

            if (Vector2.Distance(transform.position, Player.instance.transform.position) < 1.5f && actionState == ActionState.WAITING)
            {
                Debug.Log("Trying to attack (Player moved)");
                var baseDamage = Random.Range(minBaseDamage, maxBaseDamage + 1);
                StartCoroutine(AttackPlayer(baseDamage));
            }
            else if (path == null && actionState == ActionState.WAITING)
            {
                QueueMovement(Player.instance.transform.position);
                MoveOnPath();
            }
            else
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
            else if(!Pathfinding.instance.CheckLineOfSight(transform.position, Player.instance.transform.position))
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
        if (path != null)
        {
            actionState = ActionState.ACTIVE;

            targetPos = path[currentPathIndex];

            if (Pathfinding.instance.FindPlayerOnTile(targetPos))
            {
                Debug.Log("Player!");
                path = null;
                currentPathIndex = 0;
            }
            else
            {
                Vector2 tempPos = new Vector2(transform.position.x, transform.position.y);
                if (Vector2.Distance(tempPos, targetPos) > 0.05f)
                {
                    Debug.Log("SettingVelocity");
                    Vector2 movementDir = (targetPos - new Vector2(transform.position.x, transform.position.y)).normalized;

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
                        if (currentPathIndex >= path.Count)
                        {
                            StopMovement();
                        }
                    }

                    if(currentPathIndex != 1)
                    {
                        actionState = ActionState.WAITING;
                        body.velocity = Vector2.zero;
                    }                    
                }
            }
        }
        else
        {
            actionState = ActionState.WAITING;
            body.velocity = Vector2.zero;
        }
    }

    public void StopMovement()
    {
        path = null;
        currentPathIndex = 0;
        body.velocity = Vector2.zero;
        actionState = ActionState.WAITING;
    }

    IEnumerator AttackPlayer(int damage)
    {
        actionState = ActionState.ACTIVE;       
        yield return new WaitForSeconds(0.5f);
        Player.stats.TakeDamage(damage, accuracy, transform.position);
        TurnManager.instance.PassToNextEnemy();
        actionState = ActionState.WAITING;
    }


    public void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos)
    {
        //Evasion chance
        int evasionPercent = Mathf.FloorToInt(((evasion - attackerAccuracy) / (evasion + 10)) * 100);
        Debug.Log("Enemy evasion chacne: " + evasionPercent + "%");
        int evasionRoll = Random.Range(1, 101);
        if(evasionRoll <= evasionPercent)
        {
            Debug.Log("Dodge!");
        }
        else
        {
            if (!healthBar.gameObject.activeInHierarchy)
            {
                healthBar.gameObject.SetActive(true);
            }

            var hitDirection = (transform.position - attackerPos).normalized;
            var angle = Mathf.Atan2(hitDirection.y, hitDirection.x);
            //hitFX.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
            hitFX.Play();

            var defence = Random.Range(minDefence, maxDefence + 1);
            health -= (damage - defence);
            float value = (health / maxHealth);
            healthBar.value = value;

            if (health <= 0)
            {
                Debug.Log("ENEMY DEATH!");
                TurnManager.instance.enemies.Remove(this);
                Instantiate(deathFX, transform.position, Quaternion.identity).GetComponent<ParticleSystem>().Play();
                ResetState();
                gameObject.SetActive(false);
            }
        }        
    }

    public void ResetState()
    {
        health = maxHealth;
        behaviourState = AIState.SLEEPING;
    }
}
