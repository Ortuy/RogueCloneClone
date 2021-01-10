using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;

    public List<Vector2> path;

    public float movementSpeed;
    
    private int currentPathIndex;

    private bool pathChangeQueued;

    public bool debugMode;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    public AudioClip[] footsteps;
    public AudioClip[] attacks;

    //public static PlayerMovement instance;

    // Start is called before the first frame update
    void Start()
    {
        /**
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }**/
        
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        path = null;
        spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
    }
    
    // Update is called once per frame
    void Update()
    {
        //DoPlayerTurn();
    }

    public void SetSortingOrder()
    {
        GetComponent<SpriteRenderer>().sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
    }

    public void DoPlayerTurn()
    {
        
        /**
        if (Input.GetMouseButtonDown(0))
        {
            if (path == null)
            {
                path = Pathfinding.instance.FindPath(transform.position, GridTester.GetMouseWorldPosition());
                currentPathIndex = 0;
            }
            else
            {
                pathChangeQueued = true;
            }
        }**/
        MoveOnPath();        
    }

    public void PlayFootstep()
    {
        var rand = Random.Range(0, 4);
        if(GameManager.instance.currentFloor > 5)
        {
            rand += 4;
        }
        PlaySound(footsteps[rand]);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayAttack()
    {
        var rand = Random.Range(0, attacks.Length);
        PlaySound(attacks[rand]);
    }

    public void QueueMovement(Vector2 target)
    {
        if (path == null)
        {
            path = Pathfinding.instance.FindPath(transform.position, target);
            if (path != null && debugMode)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(path[i], path[i + 1], Color.cyan, 1000);
                }
            }           
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
        
        if (path != null && currentPathIndex < path.Count)
        {
            Vector2 targetPos = path[currentPathIndex];
            spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(targetPos.y + 0.5f)) + 1;

            if (Pathfinding.instance.FindEnemyOnTile(targetPos))
            {
                Debug.Log("Enemy!");
                path = null;
                currentPathIndex = 0;
            }
            else
            {
                if (Vector2.Distance(transform.position, targetPos) > 0.09f)
                {
                    Vector2 movementDir = (targetPos - new Vector2(transform.position.x, transform.position.y)).normalized;

                    if(movementDir.x != 0)
                    {
                        animator.SetFloat("MoveDirection", movementDir.x);
                    }
                    
                    if(!animator.GetBool("Moving"))
                    {
                        animator.SetBool("Moving", true);
                    }
                    
                    body.velocity = movementDir * movementSpeed;
                }
                else
                {
                    
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
                            var item = Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items"));
                            if (item != null)
                            {
                                if (!Physics2D.OverlapCircle(transform.position, 2f, LayerMask.GetMask("Enemies")))
                                {
                                    Player.actions.QueueItemPickup();
                                }
                                else
                                {
                                    UIManager.instance.pickUpButton.gameObject.SetActive(true);
                                    UIManager.instance.itemPickupImage.sprite = item.GetComponent<ItemPickup>().itemInside.itemImage;
                                }
                            }
                            
                            StopMovement();
                        }
                    }
                    if(currentPathIndex != 1)
                    {
                        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
                    }                   
                }
            }            
        }
        else
        {
            body.velocity = Vector2.zero;
            animator.SetBool("Moving", false);
        }
    }

    public void StopMovement()
    {
        path = null;
        currentPathIndex = 0;
        body.velocity = Vector2.zero;
        //animator.SetBool("Moving", false);
        //spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
        animator.speed = 0;
    }

    public void StopMovement(bool abandonPath)
    {
        if(abandonPath)
        {
            path = null;
            currentPathIndex = 0;
        }
        //animator.SetBool("Moving", false);
        //spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
        body.velocity = Vector2.zero;
    }
}
