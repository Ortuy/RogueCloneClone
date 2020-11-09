using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { PLAYER, ENEMY }

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public TurnState turnState = TurnState.PLAYER;

    [SerializeField]
    private Transform enemy;
    private bool enemyMoveUp;

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
    }

    // Update is called once per frame
    void Update()
    {
        if(turnState == TurnState.PLAYER)
        {
            PlayerMovement.instance.DoPlayerTurn();
            PlayerActions.instance.DoPlayerTurn();
        }
        else
        {
            EnemyTurnTest();
        }
    }

    //Placeholder enemy handling
    //"Real" enemies will handle all that in their own scripts
    //Like the player
    public void EnemyTurnTest()
    {
        Debug.Log("EnemyTurn");
        
        if (enemyMoveUp)
        {
            enemy.position += Vector3.up;
        }
        else
        {
            enemy.position -= Vector3.up;
        }

        enemyMoveUp = !enemyMoveUp;
        
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        turnState = (turnState == TurnState.PLAYER) ? TurnState.ENEMY : TurnState.PLAYER;
    }

    public void SwitchTurn(TurnState turn)
    {
        turnState = turn;
    }
}
