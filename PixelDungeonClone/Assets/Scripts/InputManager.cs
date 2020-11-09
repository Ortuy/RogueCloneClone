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

    public Tilemap ground;

    public ParticleSystem clickFX;

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

        pathfinding = new Pathfinding(40, 40, ground);
        player = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = GetMouseWorldPosition();
            

            if(Pathfinding.instance.FindEnemyOnTile(mousePos))
            {
                PlayerActions.instance.QueueAttack(true);
            }
            else
            {
                Pathfinding.instance.GetGrid().GetXY(mousePos, out int x, out int y);
                clickFX.transform.position = new Vector2(x + 0.5f, y + 0.5f);
                clickFX.Play();
                PlayerActions.instance.QueueAttack(false);
                PlayerMovement.instance.QueueMovement(mousePos);
            }

            /**
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPos, out int x, out int y);
            pathfinding.GetGrid().GetXY(player.transform.position, out int playerX, out int playerY);
            List<PathNode> path = pathfinding.FindPath(playerX, playerY, x, y);  
            **/
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0f;
        return mPos;
    }
}
