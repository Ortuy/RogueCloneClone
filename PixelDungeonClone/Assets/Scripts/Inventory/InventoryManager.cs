using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    //19 inventory slots
    public Item[] inventoryItems;
    public InventorySlot[] inventorySlots;

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

        inventoryItems = new Item[19];
        
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].ResetItem();
        }

        goldText.text = "" + goldAmount;
    }

    public void AddItem(Item itemToAdd)
    {
        for(int i = 0; i < inventoryItems.Length; i++)
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
        for (int i = 0; i < inventoryItems.Length; i++)
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

    public void AddGold(int gold)
    {
        goldAmount += gold;
        goldText.text = "" + goldAmount;
    }
}
