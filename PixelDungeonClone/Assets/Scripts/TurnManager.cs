using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState { PLAYER, ENEMY }

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public TurnState turnState = TurnState.PLAYER;

    //public List<Enemy> enemies;
    public List<Enemy> nearbyEnemies;

    private int currentActingEnemy;

    public bool playerFrozen;
    private bool passing;
    public int playerExtraTurns;

    public List<Gas> gases;
    private bool doEnemyTurns;

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
        StartCoroutine(SearchForEnemiesNearPlayer());
        StartCoroutine(ManageTurns());
    }
    /**
    // Update is called once per frame
    void Update()
    {
        if(Pathfinding.instance != null)
        {
            if (turnState == TurnState.PLAYER)
            {
                if(!playerFrozen)
                {
                    Player.actions.turnCost--;
                    if(Player.actions.turnCost <= 0)
                    {
                        Player.instance.DoPlayerTurn();
                    }
                    else
                    {
                        SwitchTurn(TurnState.ENEMY);
                    }                    
                }
                else if(!passing)
                {
                    StartCoroutine(FreezePlayer());
                }                                
            }
            else if(doEnemyTurns)
            {
                if (currentActingEnemy == 0)
                {
                    Player.movement.StopMovement(false);
                }

                nearbyEnemies[currentActingEnemy].turnCost--;
                //Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7 && 
                if (Player.stats.GetHealth() > 0 && nearbyEnemies[currentActingEnemy].turnCost <= 0)
                {
                    nearbyEnemies[currentActingEnemy].DoTurn();
                    //PassToNextEnemy();
                }
                else
                {
                    PassToNextEnemy();
                }

                /**
                else
                {    
                    /**
                    if(enemies[currentActingEnemy].behaviourState == AIState.ALERTED)
                    {
                        Player.movement.StopMovement(false);
                    }
                    **          

                    
                    enemies[currentActingEnemy].turnCost--;
                    //Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7 && 
                    if (Player.stats.GetHealth() > 0 && enemies[currentActingEnemy].turnCost <= 0)
                    {
                        //enemies[currentActingEnemy].DoTurn();
                        PassToNextEnemy();                       
                    }
                    else
                    {
                        PassToNextEnemy();
                    }
                }
                **//**
            }
            else
            {
                SwitchTurn();
            }
        }       
    }**/

    IEnumerator ManageTurns()
    {
        while(true)
        {
            yield return null;
            if (Pathfinding.instance != null)
            {
                if (turnState == TurnState.PLAYER)
                {
                    if (!playerFrozen)
                    {
                        Player.actions.turnCost--;
                        if (Player.actions.turnCost <= 0)
                        {
                            Player.instance.DoPlayerTurn();
                        }
                        else
                        {
                            SwitchTurn(TurnState.ENEMY);
                        }
                    }
                    else if (!passing)
                    {
                        StartCoroutine(FreezePlayer());
                    }
                }
                else if (doEnemyTurns && nearbyEnemies.Count > 0)
                {
                    if (currentActingEnemy == 0)
                    {
                        Player.movement.StopMovement(false);
                    }

                    nearbyEnemies[currentActingEnemy].turnCost--;
                    //Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7 && 
                    if (Player.stats.GetHealth() > 0 && nearbyEnemies[currentActingEnemy].turnCost <= 0)
                    {
                        //nearbyEnemies[currentActingEnemy].DoTurn();

                        yield return nearbyEnemies[currentActingEnemy].StartCoroutine(nearbyEnemies[currentActingEnemy].DoTurn());
                        
                        PassToNextEnemy();
                    }
                    else
                    {
                        PassToNextEnemy();
                    }
                }
                else
                {
                    SwitchTurn();
                }
            }
        }        
    }

    IEnumerator SearchForEnemiesNearPlayer()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);

            if(turnState == TurnState.PLAYER)
            {
                var temp = Physics2D.OverlapCircleAll(Player.instance.transform.position, 7f, LayerMask.GetMask("Enemies"));

                if (temp.Length > 0)
                {
                    doEnemyTurns = true;
                    nearbyEnemies.Clear();
                    foreach (Collider2D collider in temp)
                    {
                        nearbyEnemies.Add(collider.GetComponent<Enemy>());
                    }
                }
                else
                {
                    doEnemyTurns = false;
                }
            }          
        }        
    }

    public void PassToNextEnemy()
    {        
        if(Vector2.Distance(Player.instance.transform.position, nearbyEnemies[currentActingEnemy].transform.position) < 7f)
        {
            if(nearbyEnemies[currentActingEnemy].statusEffects.Count > 0)
            {
                nearbyEnemies[currentActingEnemy].TickStatusEffects();
            }           

            if(gases.Count > 0)
            {
                var gasNearEnemy = Physics2D.OverlapCircle(nearbyEnemies[currentActingEnemy].transform.position, 0.2f, LayerMask.GetMask("Gases"));

                if (gasNearEnemy != null)
                {
                    gasNearEnemy.GetComponent<GasTile>().parentGas.DoGasEffect(nearbyEnemies[currentActingEnemy]);
                }
            }           
        }

        currentActingEnemy++;
        if (currentActingEnemy >= nearbyEnemies.Count)
        {
            SwitchTurn();
        }
    }

    IEnumerator FreezePlayer()
    {
        passing = true;
        yield return new WaitForSeconds(0.5f);
        SwitchTurn(TurnState.ENEMY);
        passing = false;
    }

    public bool EnemiesAlerted()
    {
        var temp = false;
        for(int i =0; i < nearbyEnemies.Count; i++)
        {
            if(nearbyEnemies[i].gameObject.activeInHierarchy && nearbyEnemies[i].behaviourState == AIState.ALERTED)
            {
                temp = true;
                break;
            }
        }
        return temp;
    }

    public void SwitchTurn()
    {
        if (turnState == TurnState.PLAYER)
        {
            Player.stats.TickStatusEffects();

            if(gases.Count > 0)
            {
                var gasNearPlayer = Physics2D.OverlapCircle(Player.instance.transform.position, 0.2f, LayerMask.GetMask("Gases"));

                if (gasNearPlayer != null)
                {
                    gasNearPlayer.GetComponent<GasTile>().parentGas.DoGasEffect(Player.stats);
                }

                for (int i = 0; i < gases.Count; i++)
                {
                    gases[i].TickGas();
                }
            }            
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

            if (gases.Count > 0)
            {
                var gasNearPlayer = Physics2D.OverlapCircle(Player.instance.transform.position, 0.2f, LayerMask.GetMask("Gases"));

                if (gasNearPlayer != null)
                {
                    gasNearPlayer.GetComponent<GasTile>().parentGas.DoGasEffect(Player.stats);
                }

                for (int i = 0; i < gases.Count; i++)
                {
                    gases[i].TickGas();
                }
            }
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
