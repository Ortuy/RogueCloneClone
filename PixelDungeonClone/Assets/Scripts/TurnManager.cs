using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { PLAYER, ENEMY }

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public TurnState turnState = TurnState.PLAYER;

    public List<Enemy> enemies;

    private int currentActingEnemy;

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
        SpawnEnemy(new Vector2(10.5f, 6.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if(turnState == TurnState.PLAYER)
        {
            Player.instance.DoPlayerTurn();
        }
        else
        {
            //Enemy turn handling
            //In the future every enemy will have to wait for the one before to take its turn
            Player.movement.StopMovement(false);

            if(currentActingEnemy >= enemies.Count)
            {
                SwitchTurn();
            }
            else
            {
                enemies[currentActingEnemy].DoTurn();
                /**
                if (enemies[currentActingEnemy].actionState == ActionState.WAITING)
                {
                    enemies[currentActingEnemy].DoTurn();
                }
                **/
            }

            /**
            for(int i = 0; i < enemies.Count; i++)
            {
                enemies[i].DoTurn();
            }
            SwitchTurn();
            **/
        }
    }

    public void SpawnEnemy(Vector2 position)
    {
        GameObject enemyToSpawn = ObjectPooler.instance.GetNewPooledObject();
        if(enemyToSpawn != null && enemyToSpawn.GetComponent<Enemy>() != null)
        {
            enemyToSpawn.transform.position = position;
            enemies.Add(enemyToSpawn.GetComponent<Enemy>());
            enemyToSpawn.SetActive(true);
        }
    }

    public void PassToNextEnemy()
    {
        currentActingEnemy++;
    }

    public void SwitchTurn()
    {
        turnState = (turnState == TurnState.PLAYER) ? TurnState.ENEMY : TurnState.PLAYER;
        currentActingEnemy = 0;
    }

    public void SwitchTurn(TurnState turn)
    {
        turnState = turn;
        currentActingEnemy = 0;
    }
}
