using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private Grid<PathNode> grid;
    public int x, y;

    public int gCost, hCost, fCost;

    public bool walkable;

    public PathNode previousNode;

    public PathNode(Grid<PathNode> nodeGrid, int posX, int posY)
    {
        grid = nodeGrid;
        x = posX;
        y = posY;
        walkable = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    public void SetWalkability(bool state)
    {
        walkable = state;
    }
}
