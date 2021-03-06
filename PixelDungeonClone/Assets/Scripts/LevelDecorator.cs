﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class LevelDecorator : MonoBehaviour
{
    public LevelGenerator generator;

    public bool drawOverlay = true, generateParticles = true, drawDecor = true;

    [Header("Overlay")]
    public Tilemap blackOverlay;
    public Tile[] overlayBottomTiles, overlayTopTiles, overlayLeftTiles, overlayRightTiles;
    public Tile overlayTile, overlayCornerTL, overlayCornerTR, overlayCornerBL, overlayCornerBR;
    public Tile overlayInnerCornerTL, overlayInnerCornerTR, overlayInnerCornerBL, overlayInnerCornerBR;
    public Tile overlayWeirdCornerL, overlayWeirdCornerR;

    public GameObject[] roots;

    public bool keepRootScale;

    [Header("Wall and Floor Decor")]
    public Tilemap wallDecor;
    public Tilemap wallExtraDecor, floorDecor;
    public Tile[] wallDecorTiles;
    public Tile[] extraWallDecorTiles;
    public Tile[] floorDecorTiles;

    public GameObject extraLight;

    public ParticleSystem[] enviroParticles;

    [Header("Decor Objects")]
    public GameObject tallVegetation;
    public GameObject smallVegetation;

    public Tilemap tiledFloor;
    public Tile[] tiledFloorTiles;
    public Tile[] tiledFloorEdgeTiles;

    [Header("Other")]
    public Vector2Int minCorner;
    public Vector2Int maxCorner;

    public OverlayPreset overlayPreset;

    // Start is called before the first frame update
    void Start()
    {
        minCorner = generator.minCorner;
        maxCorner = generator.maxCorner;
        if(drawOverlay)
        {
            DrawOverlayFromPreset();
        }
        if (drawDecor)
        {
            DrawDecor();
            DrawDecorObjects();
        }
        if (generateParticles)
        {
            SetUpParticles();
        }
        
        
    }

    private void DrawOverlay()
    {
        var walls0 = generator.walls0;
        var walls1 = generator.walls1;

        for (int x = minCorner.x - 10; x <= maxCorner.x + 10; x++)
        {
            for (int y = minCorner.y - 10; y <= maxCorner.y + 10; y++)
            {
                //If there is a wall tile directly here and it's possible to place a tile
                if(walls1.GetTile(new Vector3Int(x, y, 0))
                    && (walls1.GetTile(new Vector3Int(x, y - 1, 0)) || walls1.GetTile(new Vector3Int(x, y + 1, 0)))
                    && (walls1.GetTile(new Vector3Int(x - 1, y, 0)) || walls1.GetTile(new Vector3Int(x + 1, y, 0))))
                {
                    //If all four corners are filled...
                    if(walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayTile);
                    }
                    //If a corner and adjacent tiles are missing...
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y, 0))
                        && !walls1.GetTile(new Vector3Int(x, y - 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerBR);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y, 0))
                        && !walls1.GetTile(new Vector3Int(x, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerTR);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y, 0))
                        && !walls1.GetTile(new Vector3Int(x, y - 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerBL);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x + 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y, 0))
                        && !walls1.GetTile(new Vector3Int(x, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerTL);
                    }
                    //If one corner is missing...
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        if (!walls1.GetTile(new Vector3Int(x - 1, y, 0)) && walls1.GetTile(new Vector3Int(x, y - 1, 0)))
                        {
                            int roll = Random.Range(0, overlayRightTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayRightTiles[roll]);
                        }
                        else if (!walls1.GetTile(new Vector3Int(x, y - 1, 0)) && walls1.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayBottomTiles[roll]);
                        }
                        else
                        {
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayInnerCornerTR);
                            var difference0 = Random.Range(-1.2f, 1.2f);
                            var difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 2, 0.8f);
                            }
                            difference0 = Random.Range(-1.2f, 1.2f);
                            difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 3, 0.6f);
                            }
                        }
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        if(!walls1.GetTile(new Vector3Int(x - 1, y, 0)) && walls1.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayRightTiles[roll]);
                        }
                        else if(!walls1.GetTile(new Vector3Int(x, y + 1, 0)) && walls1.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayTopTiles[roll]);
                        }
                        else
                        {
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayInnerCornerBR);
                            var difference0 = Random.Range(-1.2f, 1.2f);
                            var difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 2, 0.8f);
                            }
                            difference0 = Random.Range(-1.2f, 1.2f);
                            difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 3, 0.6f);
                            }
                        }
                    }                   
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        if (!walls1.GetTile(new Vector3Int(x + 1, y, 0)) && walls1.GetTile(new Vector3Int(x, y - 1, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayLeftTiles[roll]);
                        }
                        else if (!walls1.GetTile(new Vector3Int(x, y - 1, 0)) && walls1.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayBottomTiles[roll]);
                        }
                        else
                        {
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayInnerCornerTL);
                            var difference0 = Random.Range(-1.2f, 1.2f);
                            var difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 2, 0.8f);
                            }
                            difference0 = Random.Range(-1.2f, 1.2f);
                            difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 3, 0.6f);
                            }
                        }
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        if (!walls1.GetTile(new Vector3Int(x + 1, y, 0)) && walls1.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayLeftTiles[roll]);
                        }
                        else if (!walls1.GetTile(new Vector3Int(x, y + 1, 0)) && walls1.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            int roll = Random.Range(0, overlayBottomTiles.Length);
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayTopTiles[roll]);
                        }
                        else
                        {
                            blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayInnerCornerBL);
                            var difference0 = Random.Range(-1.2f, 1.2f);
                            var difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 2, 0.8f);
                            }
                            difference0 = Random.Range(-1.2f, 1.2f);
                            difference1 = Random.Range(-1.2f, 1.2f);
                            if (walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                && walls1.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls1.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                            {
                                PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 3, 0.6f);
                            }
                        }
                    }                  
                    //If two corners are missing...
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        int roll = Random.Range(0, overlayBottomTiles.Length);
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayRightTiles[roll]);
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        int roll = Random.Range(0, overlayBottomTiles.Length);
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayTopTiles[roll]);
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        int roll = Random.Range(0, overlayBottomTiles.Length);
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayLeftTiles[roll]);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        int roll = Random.Range(0, overlayBottomTiles.Length);
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayBottomTiles[roll]);
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayWeirdCornerL);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayWeirdCornerR);
                    }
                    //If 3 corners are missing...
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerBR);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerTR);
                    }
                    else if (!walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerBL);
                    }
                    else if (walls1.GetTile(new Vector3Int(x - 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x - 1, y + 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y - 1, 0))
                        && !walls1.GetTile(new Vector3Int(x + 1, y + 1, 0)))
                    {
                        blackOverlay.SetTile(new Vector3Int(x, y, 0), overlayCornerTL);
                    }
                }
            }
        }

        for (int x = minCorner.x; x <= maxCorner.x; x++)
        {
            for (int y = minCorner.y; y <= maxCorner.y; y++)
            {
                //Placing roots...
                if (!walls1.GetTile(new Vector3Int(x, y, 0)))
                {
                    //Horizontal
                    if(blackOverlay.GetTile(new Vector3Int(x - 1, y, 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, y, 0)))
                    {
                        var difference0 = Random.Range(-1.2f, 1.2f);
                        var difference1 = Random.Range(-1.2f, 1.2f);
                        if(blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1), 0))
                            && blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0 + (Mathf.Sign(difference0))), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1 + (Mathf.Sign(difference1))), 0))
                            && blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0 - (Mathf.Sign(difference0))), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1 - (Mathf.Sign(difference1))), 0)))
                        {                            
                            PlaceRoot(new Vector2(x - 1f, y + difference0 + 0.5f), new Vector2(x + 2f, y + difference1 + 0.5f), 2);
                        }
                    }
                    //Vertical
                    if (blackOverlay.GetTile(new Vector3Int(x, y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(x, y - 1, 0)))
                    {
                        var difference0 = Random.Range(-1f, 1f);
                        var difference1 = Random.Range(-1f, 1f);
                        if (blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1), y + 1, 0))
                            && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0 + (Mathf.Sign(difference0))), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1 + (Mathf.Sign(difference1))), y + 1, 0))
                            && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0 - (Mathf.Sign(difference0))), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1 - (Mathf.Sign(difference1))), y + 1, 0)))
                        {
                            PlaceRoot(new Vector2(x + difference0 + 0.5f, y - 1f), new Vector2(x + difference1 + 0.5f, y + 2f), 2);
                        }
                    }
                }
            }
        }

        //PlaceRoot(new Vector2(0.5f,0.5f), new Vector2(3.5f, 4.5f), 1);
    }

    private void DrawOverlayFromPreset()
    {
        var walls = generator.walls1;

        for (int x = minCorner.x - 10; x <= maxCorner.x + 10; x++)
        {
            for (int y = minCorner.y - 10; y <= maxCorner.y + 10; y++)
            {

                List<bool> neighbours = new List<bool>();

                for (int i = 0; i < overlayPreset.tiles.Length; i++)
                {
                    neighbours.Clear();
                    neighbours.Add(walls.GetTile(new Vector3Int(x - 1, y + 1, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x, y + 1, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x + 1, y + 1, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x - 1, y, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x + 1, y, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x - 1, y - 1, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x, y - 1, 0)));
                    neighbours.Add(walls.GetTile(new Vector3Int(x + 1, y - 1, 0)));

                    bool tileMatch = true;
                    if(walls.GetTile(new Vector3Int(x, y, 0)))
                    {
                        for (int j = 0; j < neighbours.Count; j++)
                        {
                            if (!neighbours[j] == overlayPreset.tiles[i].neededTiles[j])
                            {
                                tileMatch = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        tileMatch = false;
                    }
                    

                    if(tileMatch)
                    {
                        var here = new Vector3Int(x, y, 0);
                        switch(overlayPreset.tiles[i].result)
                        {
                            case 0:
                                blackOverlay.SetTile(here, overlayTile);
                                break;
                            case 1:
                                blackOverlay.SetTile(here, overlayTopTiles[Random.Range(0, overlayTopTiles.Length)]);
                                break;
                            case 2:
                                blackOverlay.SetTile(here, overlayLeftTiles[Random.Range(0, overlayLeftTiles.Length)]);
                                break;
                            case 3:
                                blackOverlay.SetTile(here, overlayBottomTiles[Random.Range(0, overlayBottomTiles.Length)]);
                                break;
                            case 4:
                                blackOverlay.SetTile(here, overlayRightTiles[Random.Range(0, overlayRightTiles.Length)]);
                                break;
                            case 5:
                                blackOverlay.SetTile(here, overlayCornerTL);
                                break;
                            case 6:
                                blackOverlay.SetTile(here, overlayCornerBL);
                                break;
                            case 7:
                                blackOverlay.SetTile(here, overlayCornerBR);
                                break;
                            case 8:
                                blackOverlay.SetTile(here, overlayCornerTR);
                                break;
                            case 9:
                                blackOverlay.SetTile(here, overlayWeirdCornerL);
                                break;
                            case 10:
                                blackOverlay.SetTile(here, overlayWeirdCornerR);
                                break;
                            case 11:
                                blackOverlay.SetTile(here, overlayInnerCornerBL);
                                var difference0 = Random.Range(-1.2f, 1.2f);
                                var difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 2, 0.8f);
                                }
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 3, 0.6f);
                                }
                                break;
                            case 12:
                                blackOverlay.SetTile(here, overlayInnerCornerTL);
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 2, 0.8f);
                                }
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x + 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x + 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 3, 0.6f);
                                }
                                break;
                            case 13:
                                blackOverlay.SetTile(here, overlayInnerCornerTR);
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 2, 0.8f);
                                }
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y - 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 - 2f), 3, 0.6f);
                                }
                                break;
                            case 14:
                                blackOverlay.SetTile(here, overlayInnerCornerBR);
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 2, 0.8f);
                                }
                                difference0 = Random.Range(-1.2f, 1.2f);
                                difference1 = Random.Range(-1.2f, 1.2f);
                                if (walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 2 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 2 + difference1), 0))
                                    && walls.GetTile(new Vector3Int(Mathf.RoundToInt(x - 3 + difference0), y, 0)) && walls.GetTile(new Vector3Int(x, Mathf.RoundToInt(y + 3 + difference1), 0)))
                                {
                                    PlaceRoot(new Vector2(x - 2f + difference0, y + 0.5f), new Vector2(x + 0.5f, y + difference1 + 2f), 3, 0.6f);
                                }
                                break;
                        }
                    }
                }
            }
        }

        for (int x = minCorner.x; x <= maxCorner.x; x++)
        {
            for (int y = minCorner.y; y <= maxCorner.y; y++)
            {
                //Placing roots...
                if (!walls.GetTile(new Vector3Int(x, y, 0)))
                {
                    //Horizontal
                    if (blackOverlay.GetTile(new Vector3Int(x - 1, y, 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, y, 0)))
                    {
                        var difference0 = Random.Range(-1.2f, 1.2f);
                        var difference1 = Random.Range(-1.2f, 1.2f);
                        if (blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1), 0))
                            && blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0 + (Mathf.Sign(difference0))), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1 + (Mathf.Sign(difference1))), 0))
                            && blackOverlay.GetTile(new Vector3Int(x - 1, Mathf.RoundToInt(y + difference0 - (Mathf.Sign(difference0))), 0)) && blackOverlay.GetTile(new Vector3Int(x + 1, Mathf.RoundToInt(y + difference1 - (Mathf.Sign(difference1))), 0)))
                        {
                            PlaceRoot(new Vector2(x - 1f, y + difference0 + 0.5f), new Vector2(x + 2f, y + difference1 + 0.5f), 2);
                        }
                    }
                    //Vertical
                    if (blackOverlay.GetTile(new Vector3Int(x, y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(x, y - 1, 0)))
                    {
                        var difference0 = Random.Range(-1f, 1f);
                        var difference1 = Random.Range(-1f, 1f);
                        if (blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1), y + 1, 0))
                            && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0 + (Mathf.Sign(difference0))), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1 + (Mathf.Sign(difference1))), y + 1, 0))
                            && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference0 - (Mathf.Sign(difference0))), y - 1, 0)) && blackOverlay.GetTile(new Vector3Int(Mathf.RoundToInt(x + difference1 - (Mathf.Sign(difference1))), y + 1, 0)))
                        {
                            PlaceRoot(new Vector2(x + difference0 + 0.5f, y - 1f), new Vector2(x + difference1 + 0.5f, y + 2f), 2);
                        }
                    }
                }
            }
        }
    }
    private void PlaceRoot(Vector2 origin, Vector2 destination, int unlikeliness)
    {
        if(Random.Range(0, unlikeliness) == 0)
        {
            var root = Instantiate(roots[Random.Range(0, roots.Length)], new Vector3(origin.x, origin.y), Quaternion.identity);
            var rootLine = destination - origin;
            Debug.DrawRay(origin, rootLine, Color.blue, 1000);

            float angle = Mathf.Atan2(rootLine.y, rootLine.x) * Mathf.Rad2Deg;
            root.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var scaleFactor = rootLine.magnitude / 3;

            
            if(keepRootScale)
            {
                root.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            }
            else
            {
                root.transform.localScale = new Vector3(scaleFactor, scaleFactor * Random.Range(0.8f, 1.3f), 1);
            }
        }
    }

    private void PlaceRoot(Vector2 origin, Vector2 destination, int unlikeliness, float yScale)
    {
        if (Random.Range(0, unlikeliness) == 0)
        {
            var root = Instantiate(roots[Random.Range(0, roots.Length)], new Vector3(origin.x, origin.y), Quaternion.identity);
            var rootLine = destination - origin;
            Debug.DrawRay(origin, rootLine, Color.blue, 1000);

            float angle = Mathf.Atan2(rootLine.y, rootLine.x) * Mathf.Rad2Deg;
            root.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var scaleFactor = rootLine.magnitude / 3;

            
            if (keepRootScale)
            {
                root.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
            }
            else
            {
                root.transform.localScale = new Vector3(scaleFactor, scaleFactor * yScale * Random.Range(0.8f, 1.3f), 1);
            }
        }
    }

    private void DrawDecor()
    {
        var walls0 = generator.walls0;
        var walls1 = generator.walls1;
        var ground = generator.ground;

        for (int x = minCorner.x - 10; x <= maxCorner.x + 10; x++)
        {
            for (int y = minCorner.y - 10; y <= maxCorner.y + 10; y++)
            {
                if(walls0.GetTile(new Vector3Int(x, y, 0)))
                {
                    if(wallDecorTiles.Length != 0)
                    {
                        wallDecor.SetTile(new Vector3Int(x, y, 0), wallDecorTiles[Random.Range(0, wallDecorTiles.Length)]);
                    }
                    
                    if(Random.Range(0, 7) == 0 && extraWallDecorTiles.Length != 0)
                    {
                        wallExtraDecor.SetTile(new Vector3Int(x, y, 0), extraWallDecorTiles[Random.Range(0, extraWallDecorTiles.Length)]);
                        Instantiate(extraLight, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                    }
                }
                else if(ground.GetTile(new Vector3Int(x, y, 0)) && Random.Range(0, 5) == 0 && floorDecorTiles.Length != 0 && !tiledFloor.GetTile(new Vector3Int(x, y, 0)))
                {
                    floorDecor.SetTile(new Vector3Int(x, y, 0), floorDecorTiles[Random.Range(0, floorDecorTiles.Length)]);
                }
            }
        }
    }

    private void SetUpParticles()
    {
        Vector3 levelCentre = new Vector3(maxCorner.x - (maxCorner.x - minCorner.x + 1) / 2, maxCorner.y - (maxCorner.y - minCorner.y + 1) / 2);

        float levelArea = (maxCorner.x - minCorner.x + 1) * (maxCorner.y - minCorner.y + 1);

        for (int i = 0; i < enviroParticles.Length; i++)
        {
            var particle = Instantiate(enviroParticles[i], levelCentre, Quaternion.identity);
            //var sParticle = new SerializedObject(particle);

            var emission = particle.emission;
            var shape = particle.shape;

            //Debug.LogError("RoT: " + particle.emission.rateOverTime.constant);

            //particle.shape.scale
            

            emission.rateOverTime = emission.rateOverTime.constant * levelArea / 100;
            particle.transform.position = levelCentre;
            shape.scale = new Vector3((maxCorner.x - minCorner.x + 1), (maxCorner.y - minCorner.y + 1), 1);
        }
    }

    private void DrawDecorObjects()
    {
        for(int i = 0; i < generator.rooms.Count; i++)
        {
            if(generator.rooms[i].hasTiledFloor && tiledFloor != null)
            {
                for (int x = generator.rooms[i].minCorner.x; x <= generator.rooms[i].maxCorner.x; x++)
                {
                    for (int y = generator.rooms[i].minCorner.y; y <= generator.rooms[i].maxCorner.y; y++)
                    {
                        if (x == generator.rooms[i].minCorner.x || x == generator.rooms[i].maxCorner.x || y == generator.rooms[i].minCorner.y || y == generator.rooms[i].maxCorner.y)
                        {
                            if (Random.Range(0, 3) == 0)
                            {
                                tiledFloor.SetTile(new Vector3Int(x, y, 0), tiledFloorEdgeTiles[Random.Range(0, tiledFloorEdgeTiles.Length)]);
                                floorDecor.SetTile(new Vector3Int(x, y, 0), null);
                            }
                        }
                        
                        else
                        {
                            tiledFloor.SetTile(new Vector3Int(x, y, 0), tiledFloorTiles[Random.Range(0, tiledFloorTiles.Length)]);
                            floorDecor.SetTile(new Vector3Int(x, y, 0), null);
                        }
                    }
                }
            }

            for(int x = generator.rooms[i].minCorner.x; x <= generator.rooms[i].maxCorner.x; x++)
            {
                for (int y = generator.rooms[i].minCorner.y; y <= generator.rooms[i].maxCorner.y; y++)
                {
                    if(x == generator.rooms[i].minCorner.x || x == generator.rooms[i].maxCorner.x || y == generator.rooms[i].minCorner.y || y == generator.rooms[i].maxCorner.y)
                    {
                        if(Random.Range(0, 5) == 0)
                        {
                            if(tallVegetation != null)
                            {
                                PlaceTallVegetation(new Vector3(x + 0.5f, y + 0.5f, 0));
                            }                            
                        }
                    }

                    if (Random.Range(0, 9) == 0)
                    {
                        if(smallVegetation != null)
                        {
                            PlaceSmallVegetation(new Vector3(x + 0.5f, y + 0.5f, 0));
                        }
                        
                    }
                }
            }
        }
    }

    private void PlaceSmallVegetation(Vector3 position)
    {        
        if(!Physics2D.OverlapCircle(position, 0.1f, LayerMask.GetMask("Decor")) && generator.ground.GetTile(new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0)) && !tiledFloor.GetTile(new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0)))
        {
            Instantiate(smallVegetation, position, Quaternion.identity);
        }
    }

    public void PlaceTallVegetation(Vector3 position)
    {
        if (!Physics2D.OverlapCircle(position, 0.1f, LayerMask.GetMask("Decor")) && (tiledFloor == null || !tiledFloor.GetTile(new Vector3Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y), 0))))
        {
            Instantiate(tallVegetation, position, Quaternion.identity);
            if (smallVegetation != null)
            {
                if (Random.Range(0, 4) == 0)
                    PlaceSmallVegetation(new Vector3(position.x - 1, position.y));
                if (Random.Range(0, 4) == 0)
                    PlaceSmallVegetation(new Vector3(position.x + 1, position.y));
                if (Random.Range(0, 4) == 0)
                    PlaceSmallVegetation(new Vector3(position.x, position.y - 1));
                if (Random.Range(0, 4) == 0)
                    PlaceSmallVegetation(new Vector3(position.x, position.y - 1));
            }
            
        }
    }
}
