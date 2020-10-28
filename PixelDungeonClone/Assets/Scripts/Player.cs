using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D body;

    public List<Vector2> path;

    public float movementSpeed;

    private int currentPathIndex;

    private bool pathChange;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        path = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {          
            if(path == null)
            {
                path = Pathfinding.instance.FindPath(transform.position, GridTester.GetMouseWorldPosition());
                currentPathIndex = 0;
            }
            else
            {
                pathChange = true;
            }
        }

        MoveOnPath();
    }

    private void MoveOnPath()
    {
        if (path != null)
        {
            Vector2 targetPos = path[currentPathIndex];
            if(Vector2.Distance(transform.position, targetPos) > 0.05f)
            {
                Vector2 movementDir = (targetPos - new Vector2(transform.position.x, transform.position.y)).normalized;

                body.velocity = movementDir * movementSpeed;
            }
            else
            {
                transform.position = targetPos;
                if(pathChange)
                {
                    path = Pathfinding.instance.FindPath(transform.position, GridTester.GetMouseWorldPosition());
                    currentPathIndex = 0;
                    pathChange = false;
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= path.Count)
                    {
                        path = null;
                        currentPathIndex = 0;
                    }
                }            
            }
        }
        else
        {
            body.velocity = Vector2.zero;
        }
    }
}
