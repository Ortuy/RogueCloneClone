using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName;
    public int amount;
    public int strengthRequired;
    public bool stackable = true;
    public bool requiresStrength = false;

    public Item itemInside;

    // Start is called before the first frame update
    void Start()
    {
        itemInside = new Item();
        itemInside.itemName = itemName;
        itemInside.itemImage = GetComponent<SpriteRenderer>().sprite;
        itemInside.amount = amount;
        itemInside.strengthRequired = strengthRequired;
        itemInside.stackable = stackable;
        itemInside.requiresStrength = requiresStrength;
    }

    public void SetItem(Item item)
    {
        itemInside = item;
        itemName = itemInside.itemName;
        GetComponent<SpriteRenderer>().sprite = itemInside.itemImage;
        amount = itemInside.amount;
        strengthRequired = itemInside.strengthRequired;
        stackable = itemInside.stackable;
        requiresStrength = itemInside.requiresStrength;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            /**
            Debug.Log("fdsgfhjkgfds");
            InventoryManager.instance.AddItem(itemInside, out bool itemDelivered);
            if (itemDelivered)
            {
                gameObject.SetActive(false);

                //Destroy(gameObject);
            }
            **/
            UIManager.instance.pickUpButton.gameObject.SetActive(true);
        }       
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager.instance.pickUpButton.gameObject.SetActive(false);
        }
    }
}
