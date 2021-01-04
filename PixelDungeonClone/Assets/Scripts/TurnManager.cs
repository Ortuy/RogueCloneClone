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

    public List<Gas> gases;

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
            else
            {
                Player.movement.StopMovement(false);

                if (currentActingEnemy >= enemies.Count)
                {
                    SwitchTurn();
                }
                else
                {
                    enemies[currentActingEnemy].turnCost--;
                    if (Vector2.Distance(Player.instance.transform.position, enemies[currentActingEnemy].transform.position) <= 7 && Player.stats.GetHealth() > 0 && enemies[currentActingEnemy].turnCost <= 0)
                    {
                        enemies[currentActingEnemy].DoTurn();
                    }
                    else
                    {
                        PassToNextEnemy();
                    }
                }
            }
        }       
    }

    public void PassToNextEnemy()
    {
        enemies[currentActingEnemy].TickStatusEffects();

        var gasNearEnemy = Physics2D.OverlapCircle(enemies[currentActingEnemy].transform.position, 0.2f, LayerMask.GetMask("Gases"));

        if (gasNearEnemy != null)
        {
            gasNearEnemy.GetComponent<GasTile>().parentGas.DoGasEffect(enemies[currentActingEnemy]);
        }

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

            var gasNearPlayer = Physics2D.OverlapCircle(Player.instance.transform.position, 0.2f, LayerMask.GetMask("Gases"));

            if(gasNearPlayer != null)
            {
                gasNearPlayer.GetComponent<GasTile>().parentGas.DoGasEffect(Player.stats);
            }

            for (int i = 0; i < gases.Count; i++)
            {
                gases[i].TickGas();
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
