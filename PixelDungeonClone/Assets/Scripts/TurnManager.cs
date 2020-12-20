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

    public bool playerFrozen;
    private bool passing;
    public int playerExtraTurns;

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
                /**
                if (Input.GetKeyDown(KeyCode.K))
                {
                    Player.stats.AddStatusEffect(new PoisonEffect(Player.stats.statusIcons[0], Player.stats.GetMaxHealth() / 20, 3));
                }**/
                if(!playerFrozen)
                {
                    Player.instance.DoPlayerTurn();
                }
                else if(!passing)
                {
                    StartCoroutine(FreezePlayer());
                    //SwitchTurn(TurnState.ENEMY);
                }
                                
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
                    if(Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7 && Player.stats.GetHealth() > 0)
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

    IEnumerator FreezePlayer()
    {
        passing = true;
        yield return new WaitForSeconds(0.5f);
        SwitchTurn(TurnState.ENEMY);
        passing = false;
    }

    public void SwitchTurn()
    {
        if (turnState == TurnState.PLAYER)
        {
            Player.stats.TickStatusEffects();
        }
        if(playerExtraTurns == 0)
        {
            turnState = (turnState == TurnState.PLAYER) ? TurnState.ENEMY : TurnState.PLAYER;
            currentActingEnemy = 0;
        }
        else
        {
            playerExtraTurns--;
        }       
    }

    public void SwitchTurn(TurnState turn)
    {
        if (turn == TurnState.ENEMY)
        {
            Player.stats.TickStatusEffects();
        }
        if (playerExtraTurns == 0)
        {
            turnState = turn;
            currentActingEnemy = 0;
        }
        else
        {
            playerExtraTurns--;
        }
    }
}
