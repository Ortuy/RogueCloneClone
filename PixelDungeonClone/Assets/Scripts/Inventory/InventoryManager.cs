using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    //4 equipment slots +
    //19 inventory slots
    //0-3 - equipment
    //4+ - inventory
    public Item[] inventoryItems;
    public InventorySlot[] inventorySlots;

    //public Item weapon, armour;
    //public InventorySlot weaponSlot, armourSlot;

    public int goldAmount;
    public Text goldText;

    public ItemPickup itemTemplate;

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

        inventoryItems = new Item[23];
        
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].ResetItem();
        }

        UIManager.instance.ToggleInventory();

        goldText.text = "" + goldAmount;
    }

    public void AddItem(Item itemToAdd)
    {
        for(int i = 4; i < inventoryItems.Length; i++)
        {
            if(inventoryItems[i] != null && inventoryItems[i].itemName == itemToAdd.itemName && itemToAdd.stackable)
            {
                inventoryItems[i].amount += itemToAdd.amount;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else if(inventoryItems[i] == null)
            {
                inventoryItems[i] = itemToAdd;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
        }
    }

    public void AddItem(Item itemToAdd, out bool success)
    {
        success = true;
        for (int i = 4; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] != null && inventoryItems[i].itemName == itemToAdd.itemName && itemToAdd.stackable)
            {
                inventoryItems[i].amount += itemToAdd.amount;
                success = true;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else if (inventoryItems[i] == null)
            {
                inventoryItems[i] = itemToAdd;
                success = true;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else
            {
                Debug.Log("asdfghhdgs");
                success = false;
            }
        }       
    }

    public void SubtractItem(Item itemToSubtract)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] != null && inventoryItems[i].itemName == itemToSubtract.itemName)
            {
                inventoryItems[i].amount--;
                if(inventoryItems[i].amount > 0)
                {
                    inventorySlots[i].UpdateItem(inventoryItems[i]);
                }
                else
                {
                    inventoryItems[i] = null;
                    inventorySlots[i].ResetItem();
                }
                break;
            }           
        }
    }

    public void SubtractItem(int slotID)
    {
        inventoryItems[slotID].amount--;
        if (inventoryItems[slotID].amount > 0)
        {
            inventorySlots[slotID].UpdateItem(inventoryItems[slotID]);
        }
        else
        {
            inventoryItems[slotID] = null;
            inventorySlots[slotID].ResetItem();
        }
    }

    public void EquipItem(int slotID)
    {
        if(slotID >= 4)
        {
            if (inventoryItems[slotID].type == ItemType.WEAPON && Player.stats.GetStrength() >= inventoryItems[slotID].strengthRequired)
            {
                Item temp = inventoryItems[slotID];
                SubtractItem(slotID);
                if (inventoryItems[0] != null)
                {
                    AddItem(inventoryItems[0]);
                }

                inventoryItems[0] = temp;
                Player.stats.minBaseDamage = inventoryItems[0].statChangeMin;
                Player.stats.maxBaseDamage = inventoryItems[0].statChangeMax;
                inventorySlots[0].UpdateItem(inventoryItems[0]);
            }
            else if (inventoryItems[slotID].type == ItemType.ARMOR && Player.stats.GetStrength() >= inventoryItems[slotID].strengthRequired)
            {
                Item temp = inventoryItems[slotID];
                SubtractItem(slotID);
                if (inventoryItems[1] != null)
                {
                    AddItem(inventoryItems[1]);
                }

                inventoryItems[1] = temp;
                Player.stats.minDefence = inventoryItems[1].statChangeMin;
                Player.stats.maxDefence = inventoryItems[1].statChangeMax;
                inventorySlots[1].UpdateItem(inventoryItems[1]);
            }
        }
        else
        {
            if(inventoryItems[inventoryItems.Length - 1] == null || inventoryItems[inventoryItems.Length - 1].amount < 1)
            {
                AddItem(inventoryItems[slotID]);
            }
            else
            {
                ItemPickup temp = Instantiate(itemTemplate, Player.instance.transform.position, Quaternion.identity);
                temp.SetItem(new Item(inventoryItems[slotID], 1));               
            }

            if (inventoryItems[slotID].type == ItemType.WEAPON)
            {
                Player.stats.ResetDamage();
            }
            else if(inventoryItems[slotID].type == ItemType.ARMOR)
            {
                Player.stats.ResetDefence();
            }

            SubtractItem(slotID);
        }
    }

    public void AddGold(int gold)
    {
        goldAmount += gold;
        goldText.text = "" + goldAmount;
    }
}
