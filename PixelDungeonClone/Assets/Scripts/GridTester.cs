using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTester : MonoBehaviour
{
    [SerializeField]
    private int width, height;

    //private Grid<int> grid;
    private Pathfinding pathfinding;

    private PlayerMovement player;

    public Tilemap ground;

    // Start is called before the first frame update
    void Start()
    {
        pathfinding = new Pathfinding(40, 40, ground);
        player = FindObjectOfType<PlayerMovement>();
        //grid = new Grid<int>(width, height, 1, transform.position, () => 0);
    }

    // Update is called once per frame
    void Update()
    {       
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPos, out int x, out int y);
            pathfinding.GetGrid().GetXY(player.transform.position, out int playerX, out int playerY);
            List<PathNode> path = pathfinding.FindPath(playerX, playerY, x, y);
            /**
            if (path != null)
            {
                for(int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) + Vector3.one * 0.5f, new Vector3(path[i+1].x, path[i+1].y) + Vector3.one * 0.5f, Color.yellow, 100f);
                }
            }
            **/
        }
        
        /**
        if (Input.GetMouseButtonDown(1))
        {
            pathfinding.GetGrid().GetXY(GetMouseWorldPosition(), out int x, out int y);
            pathfinding.GetNode(x, y).SetWalkability(!pathfinding.GetNode(x, y).walkable);
            if(pathfinding.GetNode(x, y).walkable)
            {
                pathfinding.GetGrid().SetCellMessage(x, y, x + "," + y);
            }
            else
            {
                pathfinding.GetGrid().SetCellMessage(x, y, "");
            }
        }
        **/
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0f;
        return mPos;
    }
}
