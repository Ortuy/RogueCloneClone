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
        public bool hasTiledFloor;

        public Room(Vector2Int min, Vector2Int max)
        {
            minCorner = min;
            maxCorner = max;
            xSize = max.x - min.x + 1;
            ySize = max.y - min.y + 1;
            hasTiledFloor = false;
            centerPoint = new Vector2Int(Mathf.FloorToInt(min.x + (xSize / 2)), Mathf.FloorToInt(min.y + (ySize / 2)));
        }

        public Room(Vector2Int min, Vector2Int max, bool tiled)
        {
            minCorner = min;
            maxCorner = max;
            xSize = max.x - min.x + 1;
            ySize = max.y - min.y + 1;
            hasTiledFloor = tiled;
            centerPoint = new Vector2Int(Mathf.FloorToInt(min.x + (xSize / 2)), Mathf.FloorToInt(min.y + (ySize / 2)));
        }
    }

    public struct Corridor
    {
        public Vector2Int minCorner, maxCorner;

        public Corridor(Vector2Int min, Vector2Int max)
        {
            minCorner = min;
            maxCorner = max;
        }
    }
    public List<Room> rooms;

    public List<Corridor> corridors;

    private List<List<Room>> roomClusters;

    private LevelLayoutType layoutType;

    public int seed;

    public bool startWithRandomSeed;

    private Pathfinding pathfinding;

    public int floorID;

    public FloorExit exit;

    public Vector2 playerStartPos;

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
    public Tilemap checkerboard;
    public Tile groundTileBase;
    public Tile wallTileBase;
    public Tile[] wallBottomTiles, wallTopTiles;
    public Tile checkerboardTile;

    [Header("Special Rooms")]
    [SerializeField]
    private SpecialRoomData specialRoomData;
    [SerializeField]
    private int specialRoomMinAmount, specialRoomMaxAmount;
    [SerializeField]
    private int specialRoomMinDistance, specialRoomMaxDistance;
    public int spRoomAmount;

    [Header("Room Content")]
    [SerializeField]
    private ItemDropTable dropTable;   
    public int minItems, maxItems, minEnemies, maxEnemies;
    public int[] enemyPool;
    public int[] enemyWeightPool;
    [SerializeField]
    private bool giveStarterItem;
    [SerializeField]
    private Chest chestTemplate;

    private List<Item> keysToGenerate = new List<Item>();
    public bool generateShop;
    private ItemInstance[] shopInventory = new ItemInstance[13];

    [Header("Minimap")]
    public GameObject tilemapGrid;
    public Camera minimapCamera;
    public Tilemap minimapTilemapBase;
    public Tile[] minimapWallTiles, minimapCornerTiles, minimapCorridorTiles;

    public GameObject mapPickup;

    private LevelDecorator decorator;

    private int itemWeightTotal;
    private List<Item> itemWeightedList;

    // Start is called before the first frame update
    void Start()
    {
        if(startWithRandomSeed)
        {
            if(GameManager.instance != null)
            {
                //SetLevelSeed(GameManager.instance.seed);
            }
            else
            {
                SetLevelSeed(System.Environment.TickCount);
                StartCoroutine(SetGameManagerSeed());
            }            
        }
        else
        {
            SetLevelSeed(seed);
        }

        rooms = new List<Room>();
        corridors = new List<Corridor>();
        roomClusters = new List<List<Room>>();

        SetUpWeightedItemPool();

        if(InventoryManager.instance != null)
        {
            if(InventoryManager.instance.ringEquipped[0] == 6)
            {
                if(InventoryManager.instance.inventoryItems[2].cursed)
                {
                    maxItems -= InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
                }
                else
                {
                    maxItems += InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
                }
            }
            if (InventoryManager.instance.ringEquipped[1] == 6)
            {
                if (InventoryManager.instance.inventoryItems[3].cursed)
                {
                    maxItems -= InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
                }
                else
                {
                    maxItems += InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
                }
            }
        }

        decorator = GetComponent<LevelDecorator>();

        GenerateLevel(LevelLayoutType.BASIC);
        //DrawCorridor(rooms[0], rooms[1]);
    }

    IEnumerator SetGameManagerSeed()
    {
        yield return null;
        GameManager.instance.seed = seed;
    }

    public void SetLevelSeed(int newSeed)
    {
        seed = newSeed;
        Random.InitState(seed);
    }

    public void ShuffleShopInventory()
    {
        for (int i = shopInventory.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);

            ItemInstance temp = shopInventory[i];
            shopInventory[i] = shopInventory[rand];
            shopInventory[rand] = temp;
        }
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

        DrawSpecialRooms();

        if(generateShop)
        {
            DrawShop();
            spRoomAmount++;
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

        width = maxCorner.x - minCorner.x + 1 + 12;
        height = maxCorner.y - minCorner.y + 1 + 12;
        pathfinding = new Pathfinding(width, height, new Vector3(minCorner.x - 6, minCorner.y - 6), ground);

        float currentMaxDistance = 0;
        int currentCandidateRoom = 1;

        for (int i = 1; i < rooms.Count - spRoomAmount; i++)
        {
            float distance = Vector2Int.Distance(rooms[0].centerPoint, rooms[i].centerPoint);
            if (distance > currentMaxDistance)
            {
                currentMaxDistance = distance;
                currentCandidateRoom = i;
            }
        }

        int rX = Random.Range(rooms[currentCandidateRoom].minCorner.x + 1, rooms[currentCandidateRoom].maxCorner.x);
        int rY = Random.Range(rooms[currentCandidateRoom].minCorner.y + 1, rooms[currentCandidateRoom].maxCorner.y);
        Instantiate(exit, new Vector3(rX + 0.5f, rY + 0.5f), Quaternion.identity);
        checkerboard.SetTile(new Vector3Int(rX, rY, 0), null);

        PopulateWithItems();
        PopulateWithEnemies();

        rX = Random.Range(rooms[0].minCorner.x + 1, rooms[0].maxCorner.x);
        rY = Random.Range(rooms[0].minCorner.y + 1, rooms[0].maxCorner.y);
        playerStartPos = new Vector2(rX + 0.5f, rY + 0.5f);

        DrawMinimap();
    }

    private void DrawSpecialRooms()
    {
        //Drawing special rooms
        spRoomAmount = Random.Range(specialRoomMinAmount, specialRoomMaxAmount + 1);
        if (GameManager.instance != null && GameManager.instance.guaranteedSpecialRoom != -1)
        {
            if (spRoomAmount == 0)
            {
                spRoomAmount = 1;
            }
        }

        var spRoomIDs = new List<int>();

        for (int i = 0; i < spRoomAmount; i++)
        {
            spRoomIDs.Add(Random.Range(0, 9));
        }

        if (GameManager.instance != null && GameManager.instance.guaranteedSpecialRoom != -1)
        {
            spRoomIDs[0] = GameManager.instance.guaranteedSpecialRoom;
        }

        var candidateClusters = roomClusters;

        for (int i = 0; i < spRoomAmount; i++)
        {
            int randomCluster = Random.Range(0, candidateClusters.Count);
            bool roomPlaceFound = false;

            var clust = candidateClusters[randomCluster];

            var count = 500;

            while (!roomPlaceFound || count > 0)
            {
                count--;
                var dirSphere = Random.onUnitSphere;
                var direction = new Vector2(dirSphere.x, dirSphere.y).normalized;

                var distance = Random.Range(specialRoomMinDistance, specialRoomMaxDistance + 1);
                var startPoint = new Vector2Int(Mathf.RoundToInt(clust[clust.Count - 1].centerPoint.x + (direction.x * distance)), Mathf.RoundToInt(clust[clust.Count - 1].centerPoint.y + (direction.y * distance)));

                var roomX = Random.Range(5, maxRoomSize + 1);
                var roomY = Random.Range(5, maxRoomSize + 1);

                Debug.Log("Trying...");

                Debug.DrawLine(new Vector2(startPoint.x - 1, startPoint.y - 1), new Vector2(startPoint.x + roomX + 1, startPoint.y - 1), Color.yellow, 500);
                Debug.DrawLine(new Vector2(startPoint.x + roomX + 1, startPoint.y - 1), new Vector2(startPoint.x + roomX + 1, startPoint.y + roomY + 1), Color.yellow, 500);
                Debug.DrawLine(new Vector2(startPoint.x + roomX + 1, startPoint.y + roomY + 1), new Vector2(startPoint.x - 1, startPoint.y + roomY + 1), Color.yellow, 500);
                Debug.DrawLine(new Vector2(startPoint.x - 1, startPoint.y - 1), new Vector2(startPoint.x - 1, startPoint.y + roomY + 1), Color.yellow, 500);

                bool test;
                bool above = false;

                if (spRoomIDs[i] == 5)
                {
                    if (clust[clust.Count - 1].centerPoint.y > startPoint.y)
                    {
                        test = CheckForGround(new Vector2Int(startPoint.x - 1, startPoint.y - 6), new Vector2Int(startPoint.x + roomX + 1, startPoint.y + roomY + 1));
                        above = false;
                    }
                    else
                    {
                        test = CheckForGround(new Vector2Int(startPoint.x - 1, startPoint.y - 1), new Vector2Int(startPoint.x + roomX + 1, startPoint.y + roomY + 6));
                        above = true;
                    }
                }
                else
                {
                    test = CheckForGround(new Vector2Int(startPoint.x - 1, startPoint.y - 1), new Vector2Int(startPoint.x + roomX + 1, startPoint.y + roomY + 1));
                }

                Debug.Log(test);

                if (!test)
                {
                    bool tiled = false;
                    if(spRoomIDs[i] == 2 || spRoomIDs[i] == 3 || spRoomIDs[i] == 4)
                    {
                        tiled = true;
                    }
                    DrawRoom(startPoint, roomX, roomY, tiled);
                    var currentRoom = rooms[rooms.Count - 1];

                    switch (spRoomIDs[i])
                    {
                        case 0:
                            int gold = Random.Range(specialRoomData.minGold, specialRoomData.maxGold + 1);
                            int baseGold = gold;

                            while (gold > 0)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y + 1, startPoint.y + roomY);
                                var gPickup = Instantiate(InventoryManager.instance.goldTemplate, new Vector3(posX + 0.5f, posY + 0.5f, 0), Quaternion.identity);
                                var gAmount = Random.Range(Mathf.RoundToInt(baseGold / 16), Mathf.RoundToInt(baseGold / 3) + 1);
                                gPickup.amount = gAmount;
                                gold -= gAmount;
                            }

                            break;
                        case 1:
                            int amount = Random.Range(3, 5);
                            for (int j = 0; j < amount; j++)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y + 1, startPoint.y + roomY);
                                GenerateRandomItem(new Vector3(posX + 0.5f, posY + 0.5f, 0), false, true);
                            }
                            break;
                        case 2:
                            amount = Random.Range(2, 5);
                            for (int j = 0; j < amount; j++)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y + 1, startPoint.y + roomY);
                                ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f, 0), Quaternion.identity);

                                ItemInstance itemInstance;

                                if (Random.Range(0, 2) == 0)
                                {
                                    itemInstance = new ItemInstance(IdentifyingMenager.instance.weapons[Random.Range(0, IdentifyingMenager.instance.weapons.Length)], 1);
                                }
                                else
                                {
                                    itemInstance = new ItemInstance(IdentifyingMenager.instance.armour[Random.Range(0, IdentifyingMenager.instance.armour.Length)], 1);
                                }
                                itemInstance.identified = false;
                                if (Random.Range(0, 6) == 0)
                                {
                                    itemInstance.cursed = true;
                                    itemInstance.LevelUp(Random.Range(-2, 0));
                                }
                                else
                                {
                                    if (Random.Range(0, 6) == 0) itemInstance.LevelUp(1);

                                    if (Random.Range(0, 6) == 0) itemInstance.LevelUp(1);

                                    if (Random.Range(0, 6) == 0) itemInstance.LevelUp(1);

                                    if (Random.Range(0, 6) == 0) itemInstance.LevelUp(1);
                                }
                                itemDrop.SetItem(itemInstance);
                            }
                            break;
                        case 3:
                            amount = Random.Range(2, 4);
                            for (int j = 0; j < amount; j++)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y + 1, startPoint.y + roomY);
                                ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f, 0), Quaternion.identity);

                                ItemInstance itemInstance = new ItemInstance(IdentifyingMenager.instance.potions[Random.Range(0, IdentifyingMenager.instance.potions.Length)], 1);

                                itemDrop.SetItem(itemInstance);
                            }

                            Instantiate(specialRoomData.roomContents[0], new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, rooms[rooms.Count - 1].centerPoint.y + 0.5f), Quaternion.identity);
                            break;
                        case 4:
                            amount = Random.Range(2, 4);
                            for (int j = 0; j < amount; j++)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y + 1, startPoint.y + roomY);
                                ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f, 0), Quaternion.identity);

                                ItemInstance itemInstance = new ItemInstance(IdentifyingMenager.instance.scrolls[Random.Range(0, IdentifyingMenager.instance.scrolls.Length)], 1);

                                itemDrop.SetItem(itemInstance);
                            }

                            Instantiate(specialRoomData.roomContents[1], new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, rooms[rooms.Count - 1].centerPoint.y + 0.5f), Quaternion.identity);
                            break;
                        case 5:
                            //amount = Random.Range(1, 3);
                            amount = 1;

                            for (int j = 0; j < amount; j++)
                            {
                                var posX = Random.Range(startPoint.x, startPoint.x + roomX);
                                var posY = Random.Range(startPoint.y, startPoint.y + roomY);
                                if(GameManager.instance.currentFloor < 6)
                                {
                                    SpawnManager.instance.SpawnEnemy(Random.Range(4, 6), new Vector2(posX + 0.5f, posY + 0.5f));
                                }
                                else
                                {
                                    SpawnManager.instance.SpawnEnemy(Random.Range(8, 10), new Vector2(posX + 0.5f, posY + 0.5f));
                                }
                                
                            }

                            if(above)
                            {
                                ground.SetTile(new Vector3Int(rooms[rooms.Count - 1].centerPoint.x, startPoint.y + roomY, 0), groundTileBase);
                                ground.SetTile(new Vector3Int(rooms[rooms.Count - 1].centerPoint.x, startPoint.y + roomY + 1, 0), groundTileBase);

                                for (int x = startPoint.x; x < currentRoom.maxCorner.x + 1; x++)
                                {
                                    for (int y = startPoint.y + roomY + 2; y < currentRoom.maxCorner.y + 5; y++)
                                    {
                                        ground.SetTile(new Vector3Int(x, y, 0), groundTileBase);
                                    }
                                }

                                rooms.RemoveAt(rooms.Count - 1);
                                rooms.Add(new Room(startPoint, new Vector2Int(currentRoom.maxCorner.x, currentRoom.maxCorner.y + 4)));

                                Chest chest = Instantiate(chestTemplate, new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, startPoint.y + roomY + 3.5f), Quaternion.identity);
                                chest.chestItem = new ItemInstance(IdentifyingMenager.instance.rings[Random.Range(0, IdentifyingMenager.instance.rings.Length)], 1);

                                GenerateRandomItem(chest.transform.position + Vector3.left + Vector3.left, false, true);
                                GenerateRandomItem(chest.transform.position + Vector3.right + Vector3.right, false, true);
                            }
                            else
                            {
                                ground.SetTile(new Vector3Int(rooms[rooms.Count - 1].centerPoint.x, startPoint.y - 1, 0), groundTileBase);
                                ground.SetTile(new Vector3Int(rooms[rooms.Count - 1].centerPoint.x, startPoint.y - 2, 0), groundTileBase);

                                for (int x = startPoint.x; x < currentRoom.maxCorner.x + 1; x++)
                                {
                                    for (int y = startPoint.y - 5; y < startPoint.y - 2; y++)
                                    {
                                        ground.SetTile(new Vector3Int(x, y, 0), groundTileBase);
                                    }
                                }

                                rooms.RemoveAt(rooms.Count - 1);
                                rooms.Add(new Room(new Vector2Int(currentRoom.minCorner.x, currentRoom.minCorner.y - 5), currentRoom.maxCorner));

                                Chest chest = Instantiate(chestTemplate, new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, startPoint.y - 3.5f), Quaternion.identity);
                                chest.chestItem = new ItemInstance(IdentifyingMenager.instance.rings[Random.Range(0, IdentifyingMenager.instance.rings.Length)], 1);

                                GenerateRandomItem(chest.transform.position + Vector3.left + Vector3.left, false, true);
                                GenerateRandomItem(chest.transform.position + Vector3.right + Vector3.right, false, true);
                            }

                            break;
                        case 6:
                            Instantiate(specialRoomData.roomContents[2], new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, rooms[rooms.Count - 1].centerPoint.y + 0.5f), Quaternion.identity);
                            break;
                        case 7:
                            Instantiate(specialRoomData.roomContents[3], new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, rooms[rooms.Count - 1].centerPoint.y + 0.5f), Quaternion.identity);
                            break;
                        case 8:
                            Instantiate(specialRoomData.roomContents[4], new Vector3(rooms[rooms.Count - 1].centerPoint.x + 0.5f, rooms[rooms.Count - 1].centerPoint.y + 0.5f), Quaternion.identity);
                            break;
                    }

                    
                    DrawLockedCorridor(currentRoom, clust[clust.Count - 1]);
                    candidateClusters.RemoveAt(randomCluster);
                    break;
                }
            }
        }
        GameManager.instance.guaranteedSpecialRoom = -1;
    }

    private void DrawShop()
    {
        bool roomPlaceFound = false;

        var startRoom = rooms[0];

        var count = 500;

        while (!roomPlaceFound || count > 0)
        {
            count--;
            var dirSphere = Random.onUnitSphere;
            var direction = new Vector2(dirSphere.x, dirSphere.y).normalized;

            var distance = Random.Range(specialRoomMinDistance/2, specialRoomMaxDistance + 1);
            var startPoint = new Vector2Int(Mathf.RoundToInt(startRoom.centerPoint.x + (direction.x * distance)), Mathf.RoundToInt(startRoom.centerPoint.y + (direction.y * distance)));

            var roomX = 7;
            var roomY = 5;

            Debug.Log("Trying Shop...");

            Debug.DrawLine(new Vector2(startPoint.x - 1, startPoint.y - 1), new Vector2(startPoint.x + roomX + 1, startPoint.y - 1), Color.cyan, 500);
            Debug.DrawLine(new Vector2(startPoint.x + roomX + 1, startPoint.y - 1), new Vector2(startPoint.x + roomX + 1, startPoint.y + roomY + 1), Color.cyan, 500);
            Debug.DrawLine(new Vector2(startPoint.x + roomX + 1, startPoint.y + roomY + 1), new Vector2(startPoint.x - 1, startPoint.y + roomY + 1), Color.cyan, 500);
            Debug.DrawLine(new Vector2(startPoint.x - 1, startPoint.y - 1), new Vector2(startPoint.x - 1, startPoint.y + roomY + 1), Color.cyan, 500);

            bool test;

            test = CheckForGround(new Vector2Int(startPoint.x - 1, startPoint.y - 1), new Vector2Int(startPoint.x + roomX + 1, startPoint.y + roomY + 1));

            Debug.Log(test);

            if (!test)
            {
                DrawRoom(startPoint, roomX, roomY, true);
                var currentRoom = rooms[rooms.Count - 1];

                
                for(int i = 0; i < 4; i++)
                {
                    if(Random.Range(0, 2) == 0)
                    {
                        shopInventory[i] = new ItemInstance(IdentifyingMenager.instance.potions[0], 1);
                    }
                    else
                    {
                        shopInventory[i] = new ItemInstance(IdentifyingMenager.instance.potions[Random.Range(2, IdentifyingMenager.instance.potions.Length)], 1);
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        shopInventory[i+4] = new ItemInstance(IdentifyingMenager.instance.scrolls[0], 1);
                    }
                    else
                    {
                        shopInventory[i+4] = new ItemInstance(IdentifyingMenager.instance.scrolls[Random.Range(2, IdentifyingMenager.instance.scrolls.Length)], 1);
                    }
                }
                shopInventory[8] = new ItemInstance(IdentifyingMenager.instance.rings[Random.Range(0, IdentifyingMenager.instance.rings.Length)], 1);
                shopInventory[9] = new ItemInstance(specialRoomData.shopExtras[0], 1);
                shopInventory[10] = new ItemInstance(specialRoomData.shopExtras[1], 1);
                shopInventory[11] = new ItemInstance(specialRoomData.shopWeapons[Random.Range(0, specialRoomData.shopWeapons.Length)], 1);
                shopInventory[12] = new ItemInstance(specialRoomData.shopArmour[Random.Range(0, specialRoomData.shopArmour.Length)], 1);
                ShuffleShopInventory();

                
                if(currentRoom.centerPoint.y <= startRoom.centerPoint.y)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.minCorner.x + 0.5f, currentRoom.minCorner.y + 2.5f + i), Quaternion.identity).shopItem = shopInventory[i];
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.maxCorner.x + 0.5f, currentRoom.minCorner.y + 2.5f + i), Quaternion.identity).shopItem = shopInventory[i + 3];
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.minCorner.x + 0.5f + i, currentRoom.minCorner.y + 1.5f), Quaternion.identity).shopItem = shopInventory[i + 6];
                    }
                    Instantiate(specialRoomData.shopMerchant, new Vector3(currentRoom.centerPoint.x + 0.5f, currentRoom.centerPoint.y + 1.5f), Quaternion.identity);
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.minCorner.x + 0.5f, currentRoom.minCorner.y + 1.5f + i), Quaternion.identity).shopItem = shopInventory[i];
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.maxCorner.x + 0.5f, currentRoom.minCorner.y + 1.5f + i), Quaternion.identity).shopItem = shopInventory[i + 3];
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        Instantiate(specialRoomData.shopItemTemplate, new Vector3(currentRoom.minCorner.x + 0.5f + i, currentRoom.maxCorner.y + 0.5f), Quaternion.identity).shopItem = shopInventory[i + 6];
                    }
                    Instantiate(specialRoomData.shopMerchant, new Vector3(currentRoom.centerPoint.x + 0.5f, currentRoom.centerPoint.y + 0.5f), Quaternion.identity);
                }
                

                DrawShopCorridor(currentRoom, startRoom);
                break;
            }
        }
    }

    //GROAN
    private bool CheckForGround(Vector2Int pointA, Vector2Int pointB)
    {
        Vector2Int minPoint = new Vector2Int(Mathf.Min(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y));
        Vector2Int maxPoint = new Vector2Int(Mathf.Max(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y));

        var output = false;
        for(int x = minPoint.x; x <= maxPoint.x; x++)
        {
            for (int y = minPoint.y; y <= maxPoint.y; y++)
            {
                if(ground.GetTile(new Vector3Int(x, y, 0)))
                {
                    output = true;
                    break;
                }
            }
        }
        return output;
    }

    private void SetUpWeightedItemPool()
    {
        itemWeightTotal = 0;
        for (int i = 0; i < dropTable.itemWeightPool.Length; i++)
        {
            itemWeightTotal += dropTable.itemWeightPool[i];
        }

        itemWeightedList = new List<Item>();
        for (int i = 0; i < dropTable.itemPool.Length; i++)
        {
            for (int j = 0; j < dropTable.itemWeightPool[i]; j++)
            {
                itemWeightedList.Add(dropTable.itemPool[i]);
            }
        }
    }

    private void GenerateRandomItem(Vector3 position, bool canBeNoChest, bool canBeChest)
    {       
        int roll = Random.Range(0, itemWeightTotal);

        var itemToDrop = new ItemInstance(itemWeightedList[roll], 1);
        if (itemToDrop.type == ItemType.POTION)
        {
            itemToDrop = new ItemInstance(IdentifyingMenager.instance.potions[Random.Range(0, IdentifyingMenager.instance.potions.Length)], 0);
        }
        else if (itemToDrop.type == ItemType.SCROLL)
        {
            itemToDrop = new ItemInstance(IdentifyingMenager.instance.scrolls[Random.Range(0, IdentifyingMenager.instance.scrolls.Length)], 0);
        }
        else if (itemToDrop.type == ItemType.RING && itemToDrop.effectID != 10)
        {
            itemToDrop = new ItemInstance(IdentifyingMenager.instance.rings[Random.Range(0, IdentifyingMenager.instance.rings.Length)], 0);
            if (Random.Range(0, 4) == 0 && itemToDrop.effectID != 9)
            {
                itemToDrop.cursed = true;
            }
            if (Random.Range(0, 6) == 0) itemToDrop.LevelUp(1);

            if (Random.Range(0, 6) == 0) itemToDrop.LevelUp(1);
        }
        else if (itemToDrop.type == ItemType.ARMOR || itemToDrop.type == ItemType.WEAPON)
        {
            itemToDrop.identified = false;
            if (Random.Range(0, 4) == 0)
            {
                itemToDrop.cursed = true;
                itemToDrop.LevelUp(Random.Range(-3, 0));
            }
            else
            {
                if (Random.Range(0, 6) == 0) itemToDrop.LevelUp(1);

                if (Random.Range(0, 6) == 0) itemToDrop.LevelUp(1);

                if (Random.Range(0, 6) == 0) itemToDrop.LevelUp(1);
            }
        }

        //var objectAbove = Physics2D.OverlapPoint(new Vector3(position.x, position.y + 1f), LayerMask.GetMask("Decor"));
        //var objectBelow = Physics2D.OverlapPoint(new Vector3(position.x, position.y - 1f), LayerMask.GetMask("Decor"));
        var objectHere = Physics2D.OverlapPoint(position, LayerMask.GetMask("Decor"));
        if (canBeChest
            /**&& ((objectAbove != null && !objectAbove.CompareTag("Interactible")) || objectAbove == null)**/
            && ((objectHere != null && !objectHere.CompareTag("Interactible")) || objectHere == null))
        {
            Chest chest = Instantiate(chestTemplate, position, Quaternion.identity);
            chest.chestItem = new ItemInstance(itemToDrop, 1);
        }
        else if(canBeNoChest)
        {
            ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, position, Quaternion.identity);
            itemDrop.SetItem(new ItemInstance(itemToDrop, 1));
        }
    }

    private void PopulateWithItems()
    {
        int amount = Random.Range(minItems, maxItems + 1);       

        for(int i = 0; i < amount; i++)
        {
            
            int roomID = Random.Range(1, rooms.Count - spRoomAmount);

            float posX = Random.Range(rooms[roomID].minCorner.x, rooms[roomID].maxCorner.x);
            float posY = Random.Range(rooms[roomID].minCorner.y + 1, rooms[roomID].maxCorner.y);

            bool chest = false;
            if(Random.Range(0, 6) == 0)
            {
                chest = true;
            }

            GenerateRandomItem(new Vector3(posX + 0.5f, posY + 0.5f, 0), true, chest);
            
        }

        bool giveExtras = false;
        if (GameManager.instance.currentFloor % 2 == 1)
        {
            giveExtras = true;
        }

        if(giveExtras)
        {
            var extraPotion = (Random.Range(0, 3) == 0) ? 1 : 0;
            var extraScroll = (Random.Range(0, 3) == 0) ? 1 : 0;
            dropTable.guaranteedItems.Add(IdentifyingMenager.instance.potions[extraPotion]);
            dropTable.guaranteedItems.Add(IdentifyingMenager.instance.scrolls[extraScroll]);
        }        

        for (int i = 0; i < dropTable.guaranteedItems.Count; i++)
        {
            int roomID = Random.Range(1, rooms.Count - 1 - spRoomAmount);

            float posX = Random.Range(rooms[roomID].minCorner.x, rooms[roomID].maxCorner.x);
            float posY = Random.Range(rooms[roomID].minCorner.y + 1, rooms[roomID].maxCorner.y);

            ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f), Quaternion.identity);
            itemDrop.SetItem(new ItemInstance(dropTable.guaranteedItems[i], 1));
        }

        if (giveExtras)
        {
            dropTable.guaranteedItems.RemoveAt(dropTable.guaranteedItems.Count - 1);
            dropTable.guaranteedItems.RemoveAt(dropTable.guaranteedItems.Count - 1);
        }            

        for (int i = 0; i < keysToGenerate.Count; i++)
        {
            int roomID = Random.Range(1, rooms.Count - 1 - spRoomAmount);

            float posX = Random.Range(rooms[roomID].minCorner.x, rooms[roomID].maxCorner.x);
            float posY = Random.Range(rooms[roomID].minCorner.y + 1, rooms[roomID].maxCorner.y);

            ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f), Quaternion.identity);
            itemDrop.SetItem(new ItemInstance(keysToGenerate[i], 1));
        }

        if (giveStarterItem)
        {
            float posX = Random.Range(rooms[0].minCorner.x, rooms[0].maxCorner.x);
            float posY = Random.Range(rooms[0].minCorner.y + 1, rooms[0].maxCorner.y);

            ItemPickup itemDrop = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(posX + 0.5f, posY + 0.5f), Quaternion.identity);
            itemDrop.SetItem(new ItemInstance(dropTable.starterItems[Random.Range(0, dropTable.starterItems.Count)], 1));
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
            int roomID = Random.Range(1, rooms.Count - spRoomAmount);

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

    private void DrawRoom(Vector2Int startCorner, int xSize, int ySize, bool tiled, out bool success)
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
            rooms.Add(new Room(startCorner, new Vector2Int(startCorner.x - 1 + xSize, startCorner.y - 1 + ySize), tiled));
        }       
    }

    private void DrawRoom(Vector2Int startCorner, int xSize, int ySize, bool tiled)
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                ground.SetTile(new Vector3Int(startCorner.x + x, startCorner.y + y, 0), groundTileBase);
            }
        }
        rooms.Add(new Room(startCorner, new Vector2Int(startCorner.x - 1 + xSize, startCorner.y - 1 + ySize), tiled));
    }

    //Make sure that the initial cluster room always has space
    private void DrawCluster(Vector2Int startCorner, int clusterSize)
    {
        int lastRoom = rooms.Count;

        int xSize = Random.Range(minRoomSize, maxRoomSize + 1);
        int ySize = Random.Range(minRoomSize, maxRoomSize + 1);
        DrawRoom(startCorner, xSize, ySize, false);

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
                    int oldSize = xSize;
                    startCorner = new Vector2Int(startCorner.x, startCorner.y + 2 + ySize);
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    DrawRoom(startCorner, xSize, ySize, false);
                    var xTemp = Random.Range(0, Mathf.Min(oldSize, xSize));
                    ground.SetTile(new Vector3Int(startCorner.x + xTemp, startCorner.y - 1, 0), groundTileBase);
                    ground.SetTile(new Vector3Int(startCorner.x + xTemp, startCorner.y - 2, 0), groundTileBase);
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xTemp + 0.5f, startCorner.y - 1 + 0.5f, 0));
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xTemp + 0.5f, startCorner.y - 2 + 0.5f, 0));
                    break;
                case 1:
                    startCorner = new Vector2Int(startCorner.x + 2 + xSize, startCorner.y);
                    oldSize = ySize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    DrawRoom(startCorner, xSize, ySize, false);
                    var yTemp = Random.Range(0, Mathf.Min(oldSize, ySize));
                    ground.SetTile(new Vector3Int(startCorner.x - 1, startCorner.y + yTemp, 0), groundTileBase);
                    ground.SetTile(new Vector3Int(startCorner.x - 2, startCorner.y + yTemp, 0), groundTileBase);
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x - 1 + 0.5f, startCorner.y + yTemp + 0.5f, 0));
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x - 2 + 0.5f, startCorner.y + yTemp + 0.5f, 0));
                    break;
                case 2:
                    oldSize = xSize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    startCorner = new Vector2Int(startCorner.x, startCorner.y - 2 - ySize);                    
                    DrawRoom(startCorner, xSize, ySize, false);
                    xTemp = Random.Range(0, Mathf.Min(oldSize, xSize));
                    ground.SetTile(new Vector3Int(startCorner.x + xTemp, startCorner.y + ySize, 0), groundTileBase);
                    ground.SetTile(new Vector3Int(startCorner.x + xTemp, startCorner.y + ySize + 1, 0), groundTileBase);
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xTemp + 0.5f, startCorner.y + ySize + 0.5f, 0));
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xTemp + 0.5f, startCorner.y + ySize + 1 + 0.5f, 0));
                    break;
                case 3:
                    oldSize = ySize;
                    xSize = Random.Range(minRoomSize, maxRoomSize + 1);
                    ySize = Random.Range(minRoomSize, maxRoomSize + 1);
                    startCorner = new Vector2Int(startCorner.x - 2 - xSize, startCorner.y);
                    DrawRoom(startCorner, xSize, ySize, false);
                    yTemp = Random.Range(0, Mathf.Min(oldSize, ySize));
                    ground.SetTile(new Vector3Int(startCorner.x + xSize, startCorner.y + yTemp, 0), groundTileBase);
                    ground.SetTile(new Vector3Int(startCorner.x + xSize + 1, startCorner.y + yTemp, 0), groundTileBase);
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xSize + 0.5f, startCorner.y + yTemp + 0.5f, 0));
                    decorator.PlaceTallVegetation(new Vector3(startCorner.x + xSize + 1 + 0.5f, startCorner.y + yTemp + 0.5f, 0));
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

        //Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);

        //Technically there should be +1/-1 here, but it shouldn't matter
        int distanceX = room1.centerPoint.x - room0.centerPoint.x;
        int distanceY = room1.centerPoint.y - room0.centerPoint.y;

        var min = new Vector2Int(Mathf.Min(room0.centerPoint.x, room1.centerPoint.x), Mathf.Min(room0.centerPoint.y, room1.centerPoint.y));

        var max = new Vector2Int(Mathf.Max(room0.centerPoint.x, room1.centerPoint.x), Mathf.Max(room0.centerPoint.y, room1.centerPoint.y));

        Corridor corridor = new Corridor(min, max);

        corridors.Add(corridor);



        //This line may make something crash and burn :)
        Vector2Int direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        bool moveX = true;
        if (Random.Range(0, 2) == 0 && distanceY > 0 || distanceX == 0)
        {
            moveX = false;
        }


        int currentX = room0.centerPoint.x;
        int currentY = room0.centerPoint.y;

        int safeguard = 100;

        while (!roomReached)
        {
            if (moveX)
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

            if (distanceX == 0)
            {
                moveX = false;
                if (distanceY == 0)
                {
                    roomReached = true;
                }
            }
            if (distanceY == 0)
            {
                moveX = true;
            }
            safeguard--;
            if (safeguard == 0)
            {
                Debug.LogError("Corridor fuckup (room0 - " + room0.centerPoint + " ,room1 - " + room1.centerPoint + ")");
                Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);
                break;
            }
        }
    }

    private void DrawLockedCorridor(Room room0, Room room1)
    {
        var gateAvaliable = true;
        //Basic algorithm
        //TODO a better one
        bool roomReached = false;

        //Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);

        //Technically there should be +1/-1 here, but it shouldn't matter
        int distanceX = room1.centerPoint.x - room0.centerPoint.x;
        int distanceY = room1.centerPoint.y - room0.centerPoint.y;

        float dirY = Mathf.Sign(distanceY);

        

        //This line may make something crash and burn :)
        Vector2Int direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        int startDistanceX = distanceX;
        int startDistanceY = distanceY;

        bool moveX = true;

        int currentX = room0.centerPoint.x;
        int currentY = room0.centerPoint.y;
        
        for(int i = 0; i < room0.ySize; i++)
        {
            currentY += Mathf.FloorToInt(dirY);
            if (gateAvaliable && !ground.GetTile(new Vector3Int(currentX - 1, currentY, 0)) && !ground.GetTile(new Vector3Int(currentX + 1, currentY, 0)))
            {
                gateAvaliable = false;
                Instantiate(specialRoomData.gates[0], new Vector3(currentX + 0.5f, currentY + 0.5f, 0), Quaternion.identity);
                keysToGenerate.Add(specialRoomData.keys[0]);
            }
            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);
        }
        for(int i = 0; i < 2; i++)
        {
            currentX += direction.x;
            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);
        }

        var temp = new Vector3Int(currentX, currentY, 0);
        distanceX = room1.centerPoint.x - temp.x;
        distanceY = room1.centerPoint.y - temp.y;

        direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        currentX = temp.x;
        currentY = temp.y;

        var min = new Vector2Int(Mathf.Min(room0.centerPoint.x, room1.centerPoint.x, temp.x), Mathf.Min(room0.centerPoint.y, room1.centerPoint.y, temp.y));

        var max = new Vector2Int(Mathf.Max(room0.centerPoint.x, room1.centerPoint.x, temp.x), Mathf.Max(room0.centerPoint.y, room1.centerPoint.y, temp.y));

        Corridor corridor = new Corridor(min, max);

        corridors.Add(corridor);

        int safeguard = 100;

        if(distanceX == 0)
        {
            moveX = false;
        }

        while (!roomReached)
        {
            if (moveX)
            {
                currentX += direction.x;
                distanceX--;
            }
            else
            {
                currentY += direction.y;
                distanceY--;
                /**
                if (gateAvaliable && (distanceX < startDistanceX - (room0.xSize / 2) + 2 || distanceY < startDistanceY - (room0.ySize / 2) + 2) 
                    && !ground.GetTile(new Vector3Int(currentX - 1, currentY, 0)) && !ground.GetTile(new Vector3Int(currentX + 1, currentY, 0)))
                {
                    gateAvaliable = false;
                    Debug.Log("AAAAAAAAAAAA");
                    Instantiate(specialRoomData.gates[0], new Vector3(currentX + 0.5f, currentY + 0.5f, 0), Quaternion.identity);
                    keysToGenerate.Add(specialRoomData.keys[0]);                   
                }      
                **/
            }

            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);

            if (Random.Range(0, 6) == 0)
            {
                moveX = !moveX;
            }

            if (distanceX == 0)
            {
                moveX = false;
                if (distanceY == 0)
                {
                    roomReached = true;
                }
            }
            if (distanceY == 0)
            {
                moveX = true;
            }
            safeguard--;
            if (safeguard == 0)
            {
                Debug.LogError("Corridor fuckup (room0 - " + room0.centerPoint + " ,room1 - " + room1.centerPoint + ")");
                Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);
                break;
            }
        }
    }

    private void DrawShopCorridor(Room room0, Room room1)
    {
        //Basic algorithm
        //TODO a better one
        bool roomReached = false;

        //Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);

        //Technically there should be +1/-1 here, but it shouldn't matter
        int distanceX = room1.centerPoint.x - room0.centerPoint.x;
        int distanceY = room1.centerPoint.y - room0.centerPoint.y;

        float dirY = Mathf.Sign(distanceY);



        //This line may make something crash and burn :)
        Vector2Int direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        int startDistanceX = distanceX;
        int startDistanceY = distanceY;

        bool moveX = true;

        int currentX = room0.centerPoint.x;
        int currentY = room0.centerPoint.y;

        for (int i = 0; i < room0.ySize; i++)
        {
            currentY += Mathf.FloorToInt(dirY);            
            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);
            decorator.tiledFloor.SetTile(new Vector3Int(currentX, currentY, 0), decorator.tiledFloorTiles[Random.Range(0, decorator.tiledFloorTiles.Length)]);
        }
        for (int i = 0; i < 2; i++)
        {
            currentX += direction.x;
            ground.SetTile(new Vector3Int(currentX, currentY, 0), groundTileBase);
            decorator.tiledFloor.SetTile(new Vector3Int(currentX, currentY, 0), decorator.tiledFloorTiles[Random.Range(0, decorator.tiledFloorTiles.Length)]);
        }

        var temp = new Vector3Int(currentX, currentY, 0);
        distanceX = room1.centerPoint.x - temp.x;
        distanceY = room1.centerPoint.y - temp.y;

        direction = new Vector2Int(Mathf.RoundToInt(Mathf.Sign(distanceX)), Mathf.RoundToInt(Mathf.Sign(distanceY)));

        distanceX = Mathf.Abs(distanceX);
        distanceY = Mathf.Abs(distanceY);

        currentX = temp.x;
        currentY = temp.y;

        var min = new Vector2Int(Mathf.Min(room0.centerPoint.x, room1.centerPoint.x, temp.x), Mathf.Min(room0.centerPoint.y, room1.centerPoint.y, temp.y));

        var max = new Vector2Int(Mathf.Max(room0.centerPoint.x, room1.centerPoint.x, temp.x), Mathf.Max(room0.centerPoint.y, room1.centerPoint.y, temp.y));

        Corridor corridor = new Corridor(min, max);

        corridors.Add(corridor);

        int safeguard = 100;

        if (distanceX == 0)
        {
            moveX = false;
        }

        while (!roomReached)
        {
            if (moveX)
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
            if(distanceX > room1.xSize / 2 || distanceY > room1.ySize / 2)
            {
                decorator.tiledFloor.SetTile(new Vector3Int(currentX, currentY, 0), decorator.tiledFloorTiles[Random.Range(0, decorator.tiledFloorTiles.Length)]);
            }
           

            if (Random.Range(0, 6) == 0)
            {
                moveX = !moveX;
            }

            if (distanceX == 0)
            {
                moveX = false;
                if (distanceY == 0)
                {
                    roomReached = true;
                }
            }
            if (distanceY == 0)
            {
                moveX = true;
            }
            safeguard--;
            if (safeguard == 0)
            {
                Debug.LogError("Corridor fuckup (room0 - " + room0.centerPoint + " ,room1 - " + room1.centerPoint + ")");
                Debug.DrawLine(new Vector3(room1.centerPoint.x, room1.centerPoint.y), new Vector3(room0.centerPoint.x, room0.centerPoint.y), Color.cyan, 1000);
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
            bool isXEven = false;
            if(x%2 == 0)
            {
                isXEven = true;
            }

            for (int y = minCorner.y - 10; y <= maxCorner.y + 10; y++)
            {
                bool isYEven = false;
                if (y % 2 == 0)
                {
                    isYEven = true;
                }

                if (isXEven)
                {
                    if(!isYEven)
                    {
                        checkerboard.SetTile(new Vector3Int(x, y, 0), checkerboardTile);
                    }
                }
                else if(isYEven)
                {
                    checkerboard.SetTile(new Vector3Int(x, y, 0), checkerboardTile);
                }

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
                    walls1.SetTile(new Vector3Int(x, y, 0), wallTileBase);
                }
            }
        }
    }

    private void DrawMinimap()
    {
        Vector3 tempPos = new Vector3(0, 0, 20);

        for(int i = 0; i < rooms.Count; i++)
        {
            var tilemap = Instantiate(minimapTilemapBase, tempPos, Quaternion.identity, tilemapGrid.transform);
            for(int x = rooms[i].minCorner.x - 1; x <= rooms[i].maxCorner.x + 1; x++)
            {
                for (int y = rooms[i].minCorner.y - 1; y <= rooms[i].maxCorner.y + 1; y++)
                {
                    if(ground.GetTile(new Vector3Int(x, y, 0))
                        && !(ground.GetTile(new Vector3Int(x, y - 1, 0)) && ground.GetTile(new Vector3Int(x - 1, y, 0)) && ground.GetTile(new Vector3Int(x + 1, y, 0)) && ground.GetTile(new Vector3Int(x, y + 1, 0))))
                    {
                        if(!ground.GetTile(new Vector3Int(x - 1, y, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCorridorTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCorridorTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)) && !ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[2]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[3]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[2]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[3]);
                        }
                    }
                }
            }

            tilemap.GetComponent<MapObject>().minCorner = rooms[i].minCorner;
            tilemap.GetComponent<MapObject>().maxCorner = rooms[i].maxCorner;
        }

        for (int i = 0; i < corridors.Count; i++)
        {
            var tilemap = Instantiate(minimapTilemapBase, tempPos, Quaternion.identity, tilemapGrid.transform);
            for (int x = corridors[i].minCorner.x - 1; x <= corridors[i].maxCorner.x + 1; x++)
            {
                for (int y = corridors[i].minCorner.y - 1; y <= corridors[i].maxCorner.y + 1; y++)
                {
                    if (ground.GetTile(new Vector3Int(x, y, 0))
                        && !(ground.GetTile(new Vector3Int(x, y - 1, 0)) && ground.GetTile(new Vector3Int(x - 1, y, 0)) && ground.GetTile(new Vector3Int(x + 1, y, 0)) && ground.GetTile(new Vector3Int(x, y + 1, 0))))
                    {
                        if (!ground.GetTile(new Vector3Int(x - 1, y, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCorridorTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCorridorTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)) && !ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)) && !ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[2]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)) && !ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapCornerTiles[3]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y - 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[0]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x + 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[1]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x, y + 1, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[2]);
                        }
                        else if (!ground.GetTile(new Vector3Int(x - 1, y, 0)))
                        {
                            tilemap.SetTile(new Vector3Int(x, y, 0), minimapWallTiles[3]);
                        }
                    }
                }
            }

            tilemap.GetComponent<MapObject>().minCorner = corridors[i].minCorner;
            tilemap.GetComponent<MapObject>().maxCorner = corridors[i].maxCorner;
        }

        Vector3 camPos = new Vector3(minCorner.x + width / 2, minCorner.y + height / 2, 10);
        minimapCamera.transform.position = camPos;
        if(width < height)
        {
            minimapCamera.orthographicSize = (height + 2) / 2;
        }
        else
        {
            minimapCamera.orthographicSize = (width + 2) / (2 * minimapCamera.aspect);
        }

        int roomID = Random.Range(roomClusters[0].Count, rooms.Count - spRoomAmount);

        float posX = Random.Range(rooms[roomID].minCorner.x + 1, rooms[roomID].maxCorner.x);
        float posY = Random.Range(rooms[roomID].minCorner.y + 1, rooms[roomID].maxCorner.y);

        Instantiate(mapPickup, new Vector3(posX + 0.5f, posY + 0.5f), Quaternion.identity).transform.GetChild(2).GetComponent<SpriteRenderer>().sortingOrder = (-3 * Mathf.FloorToInt(posY + 1f)) + 1;
    }
}
