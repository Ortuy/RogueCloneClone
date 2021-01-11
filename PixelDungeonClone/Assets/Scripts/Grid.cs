using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid<TGridObject>
{

    private int width, height;

    private TGridObject[,] gridArray;

    private float cellSize;

    private TextMesh[,] debugTextArray;

    private Vector2 cellOffset;

    private Vector2 originPos;

    public Grid(int gridWidth, int gridHeight, float gridCellSize, Vector3 origin, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        width = gridWidth;
        height = gridHeight;
        cellSize = gridCellSize;
        originPos = origin;

        gridArray = new TGridObject[width, height];
        debugTextArray = new TextMesh[width, height];

        Vector2 cellOffset = new Vector2(cellSize, cellSize) * 0.5f;       

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for(int y = 0; y < gridArray.GetLength(1); y++)
            {
                //debugTextArray[x, y] = CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + cellOffset, 5, Color.cyan, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
            
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    public void SetCellMessage(Vector2 worldPos, string message)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetCellMessage(x, y, message);
    }

    public void SetCellMessage(int x, int y, string message)
    {
        debugTextArray[x, y].text = message;       
    }

    public Vector2 GetOrigin()
    {
        return originPos;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x, y) * cellSize + originPos;
    }

    public void GetXY(Vector2 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - originPos).x / cellSize);
        y = Mathf.FloorToInt((worldPos - originPos).y / cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x, y].ToString();
        }      
    }

    public void SetGridObject(Vector2 worldPos, TGridObject value)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector2 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetGridObject(x, y);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    private TextMesh CreateWorldText(string text, Transform parent, Vector3 localPos, int fontSize, Color color, TextAnchor anchor, TextAlignment alignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("WorldText", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPos;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = anchor;
        textMesh.alignment = alignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }
}
