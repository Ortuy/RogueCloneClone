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

    [SerializeField]
    private string[] groundNames, groundDescs, wallNames, wallDescs;

    //public LevelGenerator temp;

    private LevelGenerator generator;

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

        generator = FindObjectOfType<LevelGenerator>();

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
            else if(Pathfinding.instance.FindInteractibleOnTile(mousePos))
            {
                var temp = Physics2D.OverlapCircle(mousePos, 0.5f, LayerMask.GetMask("Decor")).GetComponent<InteractibleObject>();
                Player.actions.QueueInteraction(temp);

                if(Vector2.Distance(Player.instance.transform.position, temp.transform.position) > 1.5f)
                {
                    Vector3 newPos = new Vector3(temp.transform.position.x + Mathf.Sign(Player.instance.transform.position.x - temp.transform.position.x), temp.transform.position.y + Mathf.Sign(Player.instance.transform.position.y - temp.transform.position.y));

                    if(Player.instance.transform.position.x == temp.transform.position.x)
                    {
                        newPos = new Vector3(temp.transform.position.x, newPos.y);
                    }
                    if (Player.instance.transform.position.y == temp.transform.position.y)
                    {
                        newPos = new Vector3(newPos.x, temp.transform.position.y);
                    }

                    clickFX.transform.position = temp.transform.position;
                    clickFX.Play();
                    Player.movement.QueueMovement(newPos);
                }               
            }
            else
            {
                Pathfinding.instance.GetGrid().GetXY(mousePos, out int x, out int y);
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
            if(generator == null)
            {
                generator = FindObjectOfType<LevelGenerator>();
            }
            var mousePos = GetMouseWorldPosition();

            Vector3Int mouseGridPos = new Vector3Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y), 0);
            Vector3 objectPos = new Vector3(mouseGridPos.x + 0.5f, mouseGridPos.y + 0.5f, 0);

            var obj = Physics2D.OverlapCircleAll(objectPos, 0.2f);

            if(obj != null)
            {
                Debug.Log("Examining");
                ExamineObject(obj);
            }
            else
            {
                ExamineTile(mouseGridPos);
            }
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0f;
        return mPos;
    }

    public void ExamineObject(Collider2D[] objects)
    {
        bool isEnemy = false;
        bool isItem = false;
        bool isDecor = false;

        GameObject examinedEnemy = null;
        GameObject examinedItem = null;
        GameObject examinedDecor = null;

        for (int i = 0; i < objects.Length; i++)
        {
            if(objects[i].gameObject.CompareTag("Enemy"))
            {
                isEnemy = true;
                if(examinedEnemy == null)
                {
                    examinedEnemy = objects[i].gameObject;
                }
            }
            else if(objects[i].gameObject.CompareTag("Item"))
            {
                isItem = true;
                if (examinedItem == null)
                {
                    examinedItem = objects[i].gameObject;
                }
            }
            else if(objects[i].gameObject.CompareTag("Decor") || objects[i].gameObject.CompareTag("Map") || objects[i].gameObject.CompareTag("Player") || objects[i].gameObject.CompareTag("Interactible"))
            {
                isDecor = true;
                if (examinedDecor == null)
                {
                    examinedDecor = objects[i].gameObject;
                }
            }
        }

        if(isEnemy)
        {
            var decorObject = examinedEnemy.GetComponent<DecorativeObject>();

            UIManager.instance.ShowExaminePopup(Input.mousePosition, decorObject.objectName, decorObject.objectDesc);
        }
        else if (isItem)
        {
            Debug.Log("Item!");
            var item = examinedItem.GetComponent<ItemPickup>().itemInside;
            if (item.type == ItemType.POTION)
            {
                IdentifyingMenager.instance.CheckIfPotionIdentified(item);
            }
            else if (item.type == ItemType.SCROLL)
            {
                IdentifyingMenager.instance.CheckIfScrollIdentified(item);
            }
            else if (item.type == ItemType.RING)
            {
                IdentifyingMenager.instance.CheckIfRingIdentified(item);
            }

            UIManager.instance.ShowItemExaminePopup(Input.mousePosition, item);
        }
        else if (isDecor)
        {
            var decorObject = examinedDecor.GetComponent<DecorativeObject>();

            UIManager.instance.ShowExaminePopup(Input.mousePosition, decorObject.objectName, decorObject.objectDesc);
        }   
        else
        {
            var mousePos = GetMouseWorldPosition();
            Vector3Int mouseGridPos = new Vector3Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.y), 0);
            ExamineTile(mouseGridPos);
        }
    }

    public void ExamineTile(Vector3Int position)
    {
        var temp = Mathf.FloorToInt((GameManager.instance.currentFloor - 1) / 5);

        if (generator.ground.GetTile(position))
        {
            UIManager.instance.ShowExaminePopup(Input.mousePosition, groundNames[temp], groundDescs[temp]);
        }
        else
        {
            UIManager.instance.ShowExaminePopup(Input.mousePosition, wallNames[temp], wallDescs[temp]);
        }
    }
}
