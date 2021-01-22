using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RunLog
{
    public int maxFloor;

    public string characterName;
    public string description;

    //public ItemInstance[] inventory;
    public int level;
    public int xp;
    public int health, maxHealth, strength, evasion, accuracy;

    public RunLog(string causeOfDeath, bool dead)
    {
        maxFloor = GameManager.instance.currentFloor;
        characterName = PlayerPrefs.GetString("lastName");
        /**
        inventory = new ItemInstance[23];
        for(int i = 0; i < 23; i++)
        {
            if(InventoryManager.instance.inventoryItems[i] != null)
            {
                inventory[i] = InventoryManager.instance.inventoryItems[i];
            }
            else
            {
                inventory[i] = null;
            }
        }
        **/
        level = Player.stats.GetLevel();
        xp = Player.stats.GetCurrentXP();
        health = Mathf.RoundToInt(Player.stats.GetHealth());
        maxHealth = Mathf.RoundToInt(Player.stats.GetMaxHealth());
        evasion = Mathf.RoundToInt(Player.stats.GetEvasion());
        accuracy = Mathf.RoundToInt(Player.stats.GetAccuracy());
        strength = Player.stats.GetStrength();

        description = "Managed to reach floor " + maxFloor + " and level " + level + ". ";

        if (dead)
        {
            description = description + "Died a horrible death, " + causeOfDeath + "\nMay " + GameManager.instance.playerPronouns[GameManager.instance.playerPronounID] + " rest in peace.";
        }
        else
        {
            description = description + "Descended into unfathomable depths.\nMay fortune aid " + GameManager.instance.playerPronouns[GameManager.instance.playerPronounID] + " in " + GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 6] + " quest.";
        }
    }
}
