using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding instance;

    private Grid<PathNode> grid;

    private List<PathNode> openList;
    private List<PathNode> closedList;

    private Tilemap walkableGround;
    /**
    public Pathfinding(int width, int height, Tilemap tilemap)
    {
        instance = this;
        grid = new Grid<PathNode>(width, height, 1f, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        walkableGround = tilemap;

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode tempNode = grid.GetGridObject(x, y);
                if(walkableGround.GetTile(new Vector3Int(x, y, 0)))
                {
                    tempNode.walkable = true;
                }               
            }
        }
    }
    **/
    public Pathfinding(int width, int height, Vector3 origin, Tilemap tilemap)
    {
        instance = this;
        grid = new Grid<PathNode>(width, height, 1f, origin, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        walkableGround = tilemap;

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode tempNode = grid.GetGridObject(x, y);
                if (walkableGround.GetTile(new Vector3Int(x + Mathf.FloorToInt(origin.x), tempNode.y + Mathf.FloorToInt(origin.y), 0)))
                {
                    tempNode.walkable = true;
                }
            }
        }
    }

    public void CreateGrid(int width, int height, Vector3 origin, Tilemap ground)
    {
        grid = new Grid<PathNode>(width, height, 1f, origin, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        walkableGround = ground;

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode tempNode = grid.GetGridObject(x, y);
                if (walkableGround.GetTile(new Vector3Int(x, y, 0)))
                {
                    tempNode.walkable = true;
                }
            }
        }
    }

    public Grid<PathNode> GetGrid()
    {
        return grid;
    }

    public List<Vector2> FindPath(Vector2 startWorldPos, Vector2 endWorldPos)
    {
        grid.GetXY(startWorldPos, out int startX, out int startY);
        grid.GetXY(endWorldPos, out int endX, out int endY);

        //float startX = startWorldPos.x;
        //float startY = startWorldPos.y;
        //float endX = endWorldPos.x;
        //float endY = endWorldPos.y;        

        List<PathNode> path = FindPath(startX, startY, endX, endY);
        if(path == null)
        {
            return null;
        }
        else
        {
            List<Vector2> vectorPath = new List<Vector2>();
            foreach (PathNode node in path)
            {
                vectorPath.Add(new Vector2(node.x, node.y) * grid.GetCellSize() + GetGrid().GetOrigin() + Vector2.one * grid.GetCellSize() * 0.5f);
                Debug.Log("Path node: " + vectorPath[vectorPath.Count - 1].x + "," + vectorPath[vectorPath.Count - 1].y);
            }
            return vectorPath;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        Debug.Log("Start node: " + startNode.x + "," + startNode.y);
        PathNode endNode = grid.GetGridObject(endX, endY);
        Debug.Log("End node: " + endNode.x + "," + endNode.y);
        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode tempNode = grid.GetGridObject(x, y);
                tempNode.gCost = int.MaxValue;
                tempNode.CalculateFCost();
                tempNode.previousNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);

        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if(currentNode == endNode)
            {
                //Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.walkable)
                {
                    closedList.Add(neighbourNode);                   
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if(!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        //Open list searched
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        //Left 3 nodes
        if(currentNode.x - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
            if(currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            if(currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        //Right 3 nodes
        if (currentNode.x + 1 < grid.GetWidth())
        {
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }
        //Up and down
        if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return neighbourList;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while(currentNode.previousNode != null)
        {
            path.Add(currentNode.previousNode);
            currentNode = currentNode.previousNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {       
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for(int i = 1; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    public bool FindEnemyOnTile(Vector2 target)
    {
        if (Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Enemies")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool FindInteractibleOnTile(Vector2 target)
    {
        var temp = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Decor"));
        if (temp != null && temp.gameObject.CompareTag("Interactible"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool FindAnotherEnemyOnTile(Vector2 target, Enemy notTarget)
    {
        var temp = Physics2D.OverlapCircle(target, 0.5f, LayerMask.GetMask("Enemies"));
        if (temp != null)
        {
            if(temp.GetComponent<Enemy>() != notTarget)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool FindPlayerOnTile(Vector2 target)
    {
        if (new Vector2(Player.instance.transform.position.x, Player.instance.transform.position.y) == target)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //A bit clunky
    public bool CheckLineOfSight(Vector2 origin, Vector2 target)
    {
        grid.GetXY(origin, out int oX, out int oY);
        Vector2 oCell = grid.GetWorldPosition(oX, oY);
        grid.GetXY(target, out int tX, out int tY);
        Vector2 tCell = grid.GetWorldPosition(tX, tY);

        Vector2 originCell0 = new Vector2(oCell.x + 0.1f, oCell.y + 0.1f);
        Vector2 originCell1 = new Vector2(oCell.x + 0.9f, oCell.y + 0.9f);
        Vector2 originCell2 = new Vector2(oCell.x + 0.9f, oCell.y + 0.1f);
        Vector2 originCell3 = new Vector2(oCell.x + 0.1f, oCell.y + 0.9f);
        Vector2 targetCell = new Vector2(tCell.x + 0.5f, tCell.y + 0.5f);

        var wallCheck0 = Physics2D.Raycast(originCell0, targetCell - originCell0, Vector2.Distance(originCell0, targetCell), LayerMask.GetMask("Walls"));
        var wallCheck1 = Physics2D.Raycast(originCell1, targetCell - originCell1, Vector2.Distance(originCell1, targetCell), LayerMask.GetMask("Walls"));
        var wallCheck2 = Physics2D.Raycast(originCell2, targetCell - originCell2, Vector2.Distance(originCell2, targetCell), LayerMask.GetMask("Walls"));
        var wallCheck3 = Physics2D.Raycast(originCell3, targetCell - originCell3, Vector2.Distance(originCell3, targetCell), LayerMask.GetMask("Walls"));

        
        Debug.DrawLine(originCell0, targetCell, Color.red, 50);
        Debug.DrawLine(originCell1, targetCell, Color.red, 50);
        Debug.DrawLine(originCell2, targetCell, Color.red, 50);
        Debug.DrawLine(originCell3, targetCell, Color.red, 50);
          

        if (wallCheck0 && wallCheck1 && wallCheck2 && wallCheck3)
        {
            //Debug.Log("LoS: " + false);
            return false;
        }
        else
        {
            return true;
        }        
    }

    public bool CheckLineOfSightOptimised(Vector2 origin, Vector2 target)
    {
        grid.GetXY(origin, out int oX, out int oY);
        Vector2 oCell = grid.GetWorldPosition(oX, oY);
        grid.GetXY(target, out int tX, out int tY);
        Vector2 tCell = grid.GetWorldPosition(tX, tY);

        Vector2 originCell = new Vector2(oCell.x + 0.5f, oCell.y + 0.5f);
        Vector2 targetCell = new Vector2(tCell.x + 0.5f, tCell.y + 0.5f);

        var wallCheck = Physics2D.Raycast(originCell, targetCell - originCell, Vector2.Distance(originCell, targetCell), LayerMask.GetMask("Walls"));


        Debug.DrawLine(originCell, targetCell, Color.red, 50);


        if (wallCheck)
        {
            //Debug.Log("LoS: " + false);
            return false;
        }
        else
        {
            return true;
        }
    }
}
