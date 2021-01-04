using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField]
    private int width, height;

    //private Grid<int> grid;
    private Pathfinding pathfinding;

    private PlayerMovement player;

    //public Tilemap ground;

    public ParticleSystem clickFX;

    //public LevelGenerator temp;

    

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

        //ground = GameObject.FindGameObjectsWithTag("Ground")[0].GetComponent<Tilemap>();


        //pathfinding = new Pathfinding(width, height, Vector3.zero, ground);
        player = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !MouseBlocker.mouseBlocked)
        {
            Vector2 mousePos = GetMouseWorldPosition();
            
            if(Player.actions.throwQueued)
            {
                Pathfinding.instance.GetGrid().GetXY(mousePos, out int x0, out int y0);
                var temp = new Vector2(Pathfinding.instance.GetGrid().GetWorldPosition(x0, y0).x + 0.5f, Pathfinding.instance.GetGrid().GetWorldPosition(x0, y0).y + 0.5f);
                if (Pathfinding.instance.CheckLineOfSight(Player.instance.transform.position, temp) && Pathfinding.instance.GetGrid().GetGridObject(mousePos).walkable)
                {
                    Player.actions.ThrowItem(temp);
                }
                
            }
            else if(Pathfinding.instance.FindEnemyOnTile(mousePos))
            {
                Player.actions.QueueAttack(true);
            }
            else
            {
                Pathfinding.instance.GetGrid().GetXY(mousePos, out int x, out int y);
                Debug.Log("mouse pos: " + new Vector2(x, y));

                Debug.Log("TEST: " + Pathfinding.instance.GetGrid().GetWorldPosition(x, y) + " = " + mousePos);
                clickFX.transform.position = new Vector2(Pathfinding.instance.GetGrid().GetWorldPosition(x, y).x + 0.5f, Pathfinding.instance.GetGrid().GetWorldPosition(x, y).y + 0.5f);
                clickFX.Play();
                Player.actions.QueueAttack(false);
                Player.movement.QueueMovement(mousePos);
            }

            /**
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPos, out int x, out int y);
            pathfinding.GetGrid().GetXY(player.transform.position, out int playerX, out int playerY);
            List<PathNode> path = pathfinding.FindPath(playerX, playerY, x, y);  
            **/
        }
        if(Input.GetMouseButtonDown(1) && !MouseBlocker.mouseBlocked)
        {
            if(TurnManager.instance.turnState == TurnState.PLAYER)
            {
                TurnManager.instance.SwitchTurn(TurnState.ENEMY);
            }
            //Debug.Log("LoS: " + Pathfinding.instance.CheckLineOfSight(Player.instance.transform.position, GetMouseWorldPosition()));
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0f;
        return mPos;
    }
}
