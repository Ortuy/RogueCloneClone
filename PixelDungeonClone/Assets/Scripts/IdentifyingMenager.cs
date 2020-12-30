using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IdentifyingMenager : MonoBehaviour
{
    public static IdentifyingMenager instance;

    public Item[] potions;
    public Item[] scrolls;
    public Item[] weapons;
    public Item[] armour;
    //public bool[] potionsIdentified;

    public string[] potionBaseNames;
    public string[] potionEffectNames;
    public string[] potionEffectDescriptions;

    public string[] scrollBaseNames;
    public string[] scrollEffectNames;
    public string[] scrollEffectDescriptions;

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

        ShufflePotions();
        ShuffleScrolls();
    }

    public void ShuffleScrolls()
    {
        for (int i = 0; i < scrolls.Length; i++)
        {
            scrolls[i].itemName = scrollBaseNames[i];
        }

        for (int i = scrolls.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);

            Item temp = scrolls[i];
            scrolls[i] = scrolls[rand];
            scrolls[rand] = temp;

            scrolls[i].identified = false;
        }
        for (int i = 0; i < scrolls.Length; i++)
        {
            scrolls[i].effectID = i;
            Debug.Log(scrolls[i].name + " " + scrollEffectNames[i] + " " + scrolls[i].identified);
        }
        scrolls[0].identified = true;
    }

    public void IdentifyScroll(ItemInstance scroll)
    {
        var effectID = scroll.effectID;
        scroll.identified = true;
        scrolls[effectID].identified = true;
        //potionsIdentified[effectID] = true;

        scroll.itemName = scrollEffectNames[effectID];
        scroll.description = scroll.description.Replace("The symbol itself seems inscrutable.", scrollEffectDescriptions[effectID]);
    }

    public void ShufflePotions()
    {
        for (int i = 0; i < potions.Length; i++)
        {
            potions[i].itemName = potionBaseNames[i];
            //potions[i] = new Item(potions[i], 1);
        }

        for (int i = potions.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);

            Item temp = potions[i];
            potions[i] = potions[rand];
            potions[rand] = temp;

            //potions[i].effectID = i;

            potions[i].identified = false;
        }
        for (int i = 0; i < potions.Length; i++)
        {
            potions[i].effectID = i;
            Debug.Log(potions[i].name + " " + potionEffectNames[i] + " " + potions[i].identified);
        }
        potions[0].identified = true;
    }

    public void IdentifyPotion(ItemInstance potion)
    {
        var effectID = potion.effectID;
        potion.identified = true;
        potions[effectID].identified = true;
        //potionsIdentified[effectID] = true;

        potion.itemName = potionEffectNames[effectID];
        potion.description = potion.description.Replace("You have no clue what drinking it would entail.", potionEffectDescriptions[effectID]);
    }

    public void IdentifyItem(ItemInstance item)
    {
        if(item.type == ItemType.POTION)
        {
            IdentifyPotion(item);
        }
        else if(item.type == ItemType.SCROLL)
        {
            IdentifyScroll(item);
        }
        else
        {
            item.identified = true;
        }
    }

    public void CheckIfPotionIdentified(ItemInstance potion)
    {
        if(potions[potion.effectID].identified)
        {
            potion.itemName = potionEffectNames[potion.effectID];
            potion.description = potion.description.Replace("You have no clue what drinking it would entail.", potionEffectDescriptions[potion.effectID]);
        }
    }

    public void CheckIfScrollIdentified(ItemInstance scroll)
    {
        if (scrolls[scroll.effectID].identified)
        {
            scroll.itemName = scrollEffectNames[scroll.effectID];
            scroll.description = scroll.description.Replace("The symbol itself seems inscrutable.", scrollEffectDescriptions[scroll.effectID]);
        }
    }

    public void TransmuteItem(int slotID)
    {
        var type = InventoryManager.instance.inventoryItems[slotID].type;
        var cursed = InventoryManager.instance.inventoryItems[slotID].cursed;
        var level = InventoryManager.instance.inventoryItems[slotID].level;
        var amount = InventoryManager.instance.inventoryItems[slotID].amount;
        switch (type)
        {
            case ItemType.POTION:
                InventoryManager.instance.inventoryItems[slotID] = new ItemInstance(potions[Random.Range(0, potions.Length)], amount);
                break;
            case ItemType.SCROLL:
                InventoryManager.instance.inventoryItems[slotID] = new ItemInstance(scrolls[Random.Range(0, scrolls.Length)], amount);
                break;
            case ItemType.WEAPON:
                InventoryManager.instance.inventoryItems[slotID] = new ItemInstance(weapons[Random.Range(0, weapons.Length)], amount);
                InventoryManager.instance.inventoryItems[slotID].cursed = cursed;
                InventoryManager.instance.inventoryItems[slotID].LevelUp(level);
                InventoryManager.instance.inventoryItems[slotID].identified = true;
                break;
            case ItemType.ARMOR:
                InventoryManager.instance.inventoryItems[slotID] = new ItemInstance(armour[Random.Range(0, armour.Length)], amount);
                InventoryManager.instance.inventoryItems[slotID].cursed = cursed;
                InventoryManager.instance.inventoryItems[slotID].LevelUp(level);
                InventoryManager.instance.inventoryItems[slotID].identified = true;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
