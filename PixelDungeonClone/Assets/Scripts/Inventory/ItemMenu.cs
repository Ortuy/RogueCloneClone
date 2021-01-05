using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemMenu : MonoBehaviour
{
    //public static bool mouseBlocked;

    private EventTrigger eventTrigger;

    public int slotID;

    // Start is called before the first frame update
    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {           
            //Pointer exit
            EventTrigger.Entry exitUIObject = new EventTrigger.Entry();
            exitUIObject.eventID = EventTriggerType.PointerExit;
            exitUIObject.callback.AddListener((eventData) => { ExitUI(); });
            eventTrigger.triggers.Add(exitUIObject);
        }
    }

    public void ExitUI()
    {
        Debug.Log("dsafghjkl");
        gameObject.SetActive(false);
        //mouseBlocked = false;
    }

    public void DropItem()
    {
        if(TurnManager.instance.turnState == TurnState.PLAYER)
        {
            //Drops item
            ItemPickup temp = Instantiate(InventoryManager.instance.itemTemplate, Player.instance.transform.position, Quaternion.identity);
            temp.SetItem(new ItemInstance(InventoryManager.instance.inventoryItems[slotID], 1));
            InventoryManager.instance.SubtractItem(slotID);
            TurnManager.instance.SwitchTurn(TurnState.ENEMY);
            gameObject.SetActive(false);
            Player.movement.PlaySound(InventoryManager.instance.itemDropSound);
        }        
    }

    public void EquipItem()
    {
        if (TurnManager.instance.turnState == TurnState.PLAYER)
        {
            
            InventoryManager.instance.EquipItem(slotID);
            TurnManager.instance.SwitchTurn(TurnState.ENEMY);


            gameObject.SetActive(false);
        }
    }

    public void ThrowItem()
    {
        //TODO: Turn off the inventory button
        if (TurnManager.instance.turnState == TurnState.PLAYER && slotID >= 4)
        {
            Player.actions.throwQueued = true;
            Player.actions.itemToThrow = new ItemInstance(InventoryManager.instance.inventoryItems[slotID], 1);
            InventoryManager.instance.SubtractItem(slotID);

            

            //TurnManager.instance.SwitchTurn(TurnState.ENEMY);
            UIManager.instance.ToggleInventory();
            gameObject.SetActive(false);
        }
    }
}
