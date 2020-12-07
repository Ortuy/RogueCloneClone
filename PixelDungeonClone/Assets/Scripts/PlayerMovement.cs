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
        path = null;
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
        if (path != null)
        {
            Vector2 targetPos = path[currentPathIndex];

            if(Pathfinding.instance.FindEnemyOnTile(targetPos))
            {
                Debug.Log("Enemy!");
                path = null;
                currentPathIndex = 0;
            }
            else
            {
                if (Vector2.Distance(transform.position, targetPos) > 0.05f)
                {
                    Vector2 movementDir = (targetPos - new Vector2(transform.position.x, transform.position.y)).normalized;

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
                            if(Physics2D.OverlapCircle(transform.position, 0.2f, LayerMask.GetMask("Items")) && !Physics2D.OverlapCircle(transform.position, 2f, LayerMask.GetMask("Enemies")))
                            {
                                Player.actions.QueueItemPickup();
                            }
                            else
                            {
                                UIManager.instance.pickUpButton.gameObject.SetActive(true);
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
        }
    }

    public void StopMovement()
    {
        path = null;
        currentPathIndex = 0;
        body.velocity = Vector2.zero;
    }

    public void StopMovement(bool abandonPath)
    {
        if(abandonPath)
        {
            path = null;
            currentPathIndex = 0;
        }
        
        body.velocity = Vector2.zero;
    }
}
