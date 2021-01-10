using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerActions))]
[RequireComponent(typeof(PlayerStatistics))]
public class Player : MonoBehaviour
{
    public static Player instance;
    public static PlayerMovement movement;
    public static PlayerActions actions;
    public static PlayerStatistics stats;

    

    public ActionState actionState = ActionState.WAITING;

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
        DontDestroyOnLoad(gameObject);
        movement = GetComponent<PlayerMovement>();
        actions = GetComponent<PlayerActions>();
        stats = GetComponent<PlayerStatistics>();

        transform.position = FindObjectOfType<LevelGenerator>().playerStartPos;
        movement.SetSortingOrder();
    }

    public void DoPlayerTurn()
    {
        movement.DoPlayerTurn();
        actions.DoPlayerTurn();
    }
}
