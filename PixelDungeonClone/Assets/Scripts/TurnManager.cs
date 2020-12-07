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
        /**
        Enemy temp = SpawnManager.instance.SpawnEnemy(0, new Vector2(10.5f, 6.5f));
        if(temp != null)
        {
            enemies.Add(temp);
        }**/
        //SpawnEnemy(new Vector2(10.5f, 6.5f));
    }

    // Update is called once per frame
    void Update()
    {
        if(Pathfinding.instance != null)
        {
            if (turnState == TurnState.PLAYER)
            {
                Player.instance.DoPlayerTurn();
            }
            else
            {
                Player.movement.StopMovement(false);

                if (currentActingEnemy >= enemies.Count)
                {
                    SwitchTurn();
                }
                else
                {
                    if(Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7)
                    {
                        enemies[currentActingEnemy].DoTurn();
                    }
                    else
                    {
                        PassToNextEnemy();
                    }
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
    }

    /**
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
    **/

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
