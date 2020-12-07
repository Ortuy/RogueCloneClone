using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum LevelLayoutType { BASIC, LOOP, HUNT, GAUNTLET }

public class LevelGenerator : MonoBehaviour
{
    public struct Room
    {
        public Vector2Int minCorner, maxCorner;
        public int xSize, ySize;
        public Vector2Int centerPoint;

        public Room(Vector2Int min, Vector2Int max)
        {
            minCorner = min;
            maxCorner = max;
            xSize = max.x - min.x + 1;
            ySize = max.y - min.y + 1;
            centerPoint = new Vector2Int(Mathf.FloorToInt(min.x + (xSize / 2)), Mathf.FloorToInt(min.y + (ySize / 2)));
        }
    }
    private List<Room> rooms;

    private List<List<Room>> roomClusters;

    private LevelLayoutType layoutType;

    public int seed;

    public bool startWithRandomSeed;

    private Pathfinding pathfinding;

    [Header("Generation Values")]
    public int minRoomSize;
    public int maxRoomSize;
    public int minRoomClusterAmount, maxRoomClusterAmount;
    public int minDistanceFromStart, maxDistanceFromStart;
    public Vector2Int minCorner, maxCorner;
    public int width, height;

    [Header("Tilemaps")]
    public Tilemap ground;
    public Tilemap walls0, walls1;
    public Tile groundTileBase;
    public Tile wallTileBase;
    public Tile[] wallBottomTiles, wallTopTiles;

    [Header("Room Content")]
    public Item[] itemPool;
    public int[] itemWeightPool;
    public int minItems, maxItems, minEnemies, maxEnemies;
    public int[] enemyPool;
    public int[] enemyWeightPool;

    // Start is called before the first frame update
    void Start()
    {
        if(startWithRandomSeed)
        {
            SetLevelSeed(System.Environment.TickCount);
        }
        else
        {
            SetLevelSeed(seed);
        }

        rooms = new List<Room>();
        roomClusters = new List<List<Room>>();

        GenerateLevel(LevelLayoutType.BASIC);
        //DrawCorridor(rooms[0], rooms[1]);
    }

    public void SetLevelSeed(int newSeed)
    {
        seed = newSeed;
        Random.InitState(seed);
    }

    public void GenerateLevel(LevelLayoutType levelLayoutType)
    {
        layoutType = levelLayoutType;

        if(layoutType == LevelLayoutType.BASIC)
        {
            int roomClusterAmount = Random.Range(minRoomClusterAmount, maxRoomClusterAmount + 1);

            DrawInitialRooms();

            Vector2[] positionWheel = { Vector2.up, Vector2.one, Vector2.right, new Vector2(1, -1), Vector2.down, new Vector2(-1, -1), Vector2.left, new Vector2(-1, 1) };

            int startClusterPosition = Random.Range(0, positionWheel.Length);

            DrawCluster(new Vector2Int(Mathf.FloorToInt(positionWheel[startClusterPosition].normalized.x * Random.Range(minDistanceFromStart, maxDistanceFromStart)), Mathf.FloorToInt(positionWheel[startClusterPosition].normalized.y * Random.Range(minDistanceFromStart, maxDistanceFromStart))), GetRandomClusterSize());

            ConnectClusters(roomClusters[0], roomClusters[1]);

            for (int i = 1; i < roomClusterAmount; i++)
            {
                startClusterPosition++;
                if (startClusterPosition >= positionWheel.Length)
                {
                    startClusterPosition = 0;
                }

                DrawCluster(new Vector2Int(Mathf.FloorToInt(positionWheel[startClusterPosition].normalized.x * Random.Range(minDistanceFromStart, maxDistanceFromStart)), Mathf.FloorToInt(positionWheel[startClusterPosition].normalized.y * Random.Range(minDistanceFromStart, maxDistanceFromStart))), GetRandomClusterSize());
                ConnectClusters(roomClusters[i], roomClusters[i + 1]);
                ConnectClusters(roomClusters[0], roomClusters[i + 1]);
            }
        }

        minCorner = Vector2Int.zero;
        maxCorner = Vector2Int.zero;

        for(int i = 0; i < rooms.Count; i++)
        {
            if(rooms[i].minCorner.x < minCorner.x)
            {
                minCorner = new Vector2Int(rooms[i].minCorner.x, minCorner.y);
            }
            if (rooms[i].minCorner.y < minCorner.y)
            {
                minCorner = new Vector2Int(minCorner.x, rooms[i].minCorner.y);
            }

            if (rooms[i].maxCorner.x > maxCorner.x)
            {
                maxCorner = new Vector2Int(rooms[i].maxCorner.x, maxCorner.y);
            }
            if (rooms[i].maxCorner.y > maxCorner.y)
            {
                maxCorner = new Vector2Int(maxCorner.x, rooms[i].maxCorner.y);
            }
        }

        DrawWalls();

        PopulateWithItems();
        PopulateWithEnemies();

        width = maxCorner.x - minCorner.x + 1;
        height = maxCorner.y - minCorner.y + 1;
        Debug.Log(minCorner.x + " " + minCorner.y);
        Debug.Log(maxCorner.x + " " + maxCorner.y);
        pathfinding = new Pathfinding(width, height, new Vector3(minCorner.x, minCorner.y), ground);
    }

    private void PopulateWithItems()
    {
        int amount = Random.Range(minItems, maxItems + 1);

        int weightTotal = 0;
        for(int i = 0; i < itemWeightPool.Length; i++)
        {
            weightTotal += itemWeightPool[i];
        }

        List<Item> weightedPool = new List<Item>();
        for(int i = 0; i < itemPool.Length; i++)
        {
            for(int j = 0; j < itemWeightPool[i]; j++)
            {
                weightedPool.Add(itemPool[i]);
            }
        }

        for(int i = 0; i < amount; i++)
        {
            int roll = Random.Range(0, weightTotal);
            int roomID = Random.Range(1, rooms.Count - 1);

            float posX = Random.Range(rooms[roomID].minCorner.x, rooms[roomID].maxCorner.x + 1);
            float posY = Random.Range(rooms[roomID].minCorner.y, rooms[roomID].maxCorner.y + 1);

            ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f), Quaternion.identity);
            itemDrop.SetItem(weightedPool[roll]);
        }
    }

    private void PopulateWithEnemies()
    {
        int amount = Random.Range(minEnemies, maxEnemies + 1);

        int weightTotal = 0;
        for (int i = 0; i < enemyWeightPool.Length; i++)
        {
            weightTotal += enemyWeightPool[i];
        }

        List<int> weightedPool = new List<int>();
        for (int i = 0; i < enemyPool.Length; i++)
        {
            for (int j = 0; j < enemyWeightPool[i]; j++)
            {
                weightedPool.Add(enemyPool[i]);
            }
        }

        for (int i = 0; i < amount; i++)
        {
            int roll = Random.Range(0, weightTotal);
            int roomID = Random.Range(1, rooms.Count - 1);

            float posX = Random.Range(rooms[roomID].minCorner.x, rooms[roomID].maxCorner.x + 1);
            float posY = Random.Range(rooms[roomID].minCorner.y, rooms[roomID].maxCorner.y + 1);

            SpawnManager.instance.SpawnEnemy(weightedPool[roll], new Vector2(posX + 0.5f, posY + 0.5f));
        }
    }

    private LevelLayoutType GetRandomLayoutType()
    {
        int rand = Random.Range(0, 100);
        LevelLayoutType type = LevelLayoutType.BASIC;
        if (rand < 50)
        {
            type = LevelLayoutType.BASIC;
        }
        else if(rand < 80)
        {
            type = LevelLayoutType.LOOP;
        }
        else if(rand < 90)
        {
            type = LevelLayoutType.HUNT;
        }
        else
        {
            type = LevelLayoutType.GAUNTLET;
        }
        return type;
    }

    private int GetRandomClusterSize()
    {
        int roll0 = Random.Range(0, 2);
        int roll1 = Random.Range(0, 2);
        int roll2 = Random.Range(0, 2);
        return 1 + roll0 + roll1 + roll2;
    }

    private void DrawRoom(Vector2Int startCorner, int xSize, int ySize, out bool success)
    {
        success = true;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (ground.GetTile(new Vector3Int(startCorner.x + x, startCorner.y + y, 0)))
                {
                    success = false;
                    break;
                }
            }
        }      

        if(success)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    ground.SetTile(new Vector3Int(startCorner.x + x, startCorner.y + y, 0), groundTileBase);
                }
            }
            rooms.Add(new Room(startCorner, new Vector2Int(startCorner.x - 1 + xSize, startCorner.y - 1 + ySize)));
        }       
    }

    private void DrawRoom(Vector2Int startCorner, int xSize, int ySize)
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                ground.SetTile(new Vector3Int(startCorner.x + x, startCorner.y + y, 0), groundTileBase);
            }
        }
        rooms.Add(new Room(startCorner, new Vector2Int(startCorner.x - 1 + xSize, startCorner.y - 1 + ySize)));
    }

    //Make sure that the initial cluster room always has space
    private void DrawCluster(Vector2Int startCorner, int clusterSize)
    {
        int lastRoom = rooms.Count;

        int xSize = Random.Range(minRoomSize, maxRoomSize + 1);
        int ySize = Random.Range(minRoomSize, maxRoomSize + 1);
        DrawRoom(startCorner, xSize, ySize);

        int oldSide = Random.Range(0, 4);

        List<Room> cluster = new List<Room>();
        cluster.Add(rooms[lastRoom]);

        for (int i = 1; i < clusterSize; i++)
        {
            //Pick a side
            int side = Random.Range(0, 4);
            if(side - oldSide == 2 || side - oldSide == -2)
            {
                side = oldSide;
            }
            oldSide = side;

            switch (side)
            {
                case 0:
                    Debug.Log("Up");                  
                    int oldSize = xSize;
                    startCorner = new Vector2Int(startCorner.x, startCorner.y + 1 + ySize);
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    DrawRoom(startCorner, xSize, ySize);
                    ground.SetTile(new Vector3Int(startCorner.x + Random.Range(0, Mathf.Min(oldSize, xSize)), startCorner.y - 1, 0), groundTileBase);
                    break;
                case 1:
                    Debug.Log("Right");
                    startCorner = new Vector2Int(startCorner.x + 1 + xSize, startCorner.y);
                    oldSize = ySize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    DrawRoom(startCorner, xSize, ySize);
                    ground.SetTile(new Vector3Int(startCorner.x - 1, startCorner.y + Random.Range(0, Mathf.Min(oldSize, ySize)), 0), groundTileBase);
                    break;
                case 2:
                    Debug.Log("Down");
                    oldSize = xSize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    startCorner = new Vector2Int(startCorner.x, startCorner.y - 1 - ySize);                    
                    DrawRoom(startCorner, xSize, ySize);
                    ground.SetTile(new Vector3Int(startCorner.x + Random.Range(0, Mathf.Min(oldSize, xSize)), startCorner.y + ySize, 0), groundTileBase);
                    break;
                case 3:
                    Debug.Log("Left");
                    oldSize = ySize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    startCorner = new Vector2Int(startCorner.x - 1 - xSize, startCorner.y);
                    DrawRoom(startCorner, xSize, ySize);
                    ground.SetTile(new Vector3Int(startCorner.x + xSize, startCorner.y + Random.Range(0, Mathf.Min(oldSize, ySize)), 0), groundTileBase);
                    break;
            }
            cluster.Add(rooms[lastRoom + i]);
        }
        roomClusters.Add(cluster);
    }

    private void ConnectClusters(List<Room> cluster0, List<Room> cluster1)
    {
        float currentMinDistance = maxDistanceFromStart * 6;
        Vector2Int currentCandidateRooms = Vector2Int.zero;

        for(int i = 0; i < cluster0.Count; i++)
        {
            for(int j = 0; j < cluster1.Count; j++)
            {
                float distance = Vector2Int.Distance(cluster0[i].centerPoint, cluster1[j].centerPoint);
                if(distance < currentMinDistance)
                {
                    currentMinDistance = distance;
                    currentCandidateRooms = new Vector2Int(i, j);
                }
            }
        }

        DrawCorridor(cluster0[currentCandidateRooms.x], cluster1[currentCandidateRooms.y]);
    }

    private void DrawCorridor(Room room0, Room room1)
    {
        //Basic algorithm
        //TODO a better one
        bool roomReached = false;
        bool moveX = true;
        if (Random.Range(0, 2) == 0)
        {
            moveX = false;
        }

        //Technically there should be +1/-1 here, but it shouldn't matter
        int distanceX = room1.centerPoint.x - room0.centerPoint.x;
        int distanceY = room1.centerPoint.y - room0.centerPoint.y;

        //This line may make something crash and burn :)
        Vector2Int direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        int currentX = room0.centerPoint.x;
        int currentY = room0.centerPoint.y;

        int safeguard = 100;

        while (!roomReached)
        {
            if(moveX)
            {
                currentX += direction.x;
                distanceX--;
            }
            else
            {
                currentY += direction.y;
                distanceY--;
            }

            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);

            if (Random.Range(0, 6) == 0)
            {
                moveX = !moveX;
            }

            if(distanceX == 0)
            {
                moveX = false;
                if(distanceY == 0)
                {
                    roomReached = true;
                }
            }
            if(distanceY == 0)
            {
                moveX = true;
            }
            safeguard--;
            if(safeguard == 0)
            {
                break;
            }
        }
    }

    private void DrawInitialRooms()
    {
        if(layoutType == LevelLayoutType.BASIC)
        {
            int size = GetRandomClusterSize();
            DrawCluster(new Vector2Int(0, 0), size);
        }
    }

    private void DrawWalls()
    {
        for(int x = minCorner.x - 10; x <= maxCorner.x + 10; x++)
        {
            for (int y = minCorner.y - 10; y <= maxCorner.y + 10; y++)
            {
                if(ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x, y, 0)))
                {
                    //Wall bottom
                    int roll = Random.Range(0, wallBottomTiles.Length);
                    walls0.SetTile(new Vector3Int(x, y, 0), wallBottomTiles[roll]);
                }
                else if(!ground.GetTile(new Vector3Int(x, y - 1, 0)) && ground.GetTile(new Vector3Int(x, y, 0)))
                {
                    //Wall top
                    int roll = Random.Range(0, wallTopTiles.Length);
                    walls1.SetTile(new Vector3Int(x, y, 0), wallTopTiles[roll]);
                }
                else if(!ground.GetTile(new Vector3Int(x, y, 0)))
                {
                    //Wall
                    walls0.SetTile(new Vector3Int(x, y, 0), wallTileBase);
                }
            }
        }
    }
}
