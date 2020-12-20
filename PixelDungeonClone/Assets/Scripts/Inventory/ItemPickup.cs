using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    /**
    //TODO: Items instead of this crap 
    public string itemName;

    public string description;

    public int amount;
    public int strengthRequired;
    public bool stackable = true;
    public bool requiresStrength = false;

    public ItemType type = ItemType.NONE;
    **/
    public Item itemPrefab;
    public ItemInstance itemInside;

    public int amount;

    // Start is called before the first frame update
    void Start()
    {
        if(itemInside == null)
        {
            itemInside = new ItemInstance(itemPrefab, amount);
        }
        GetComponent<SpriteRenderer>().sprite = itemInside.itemImage;
        if (itemInside.stackable)
        {
            itemInside.amount = amount;
        }
        
        /**
        itemInside.itemName = itemName;
        itemInside.description = description;
        itemInside.itemImage = GetComponent<SpriteRenderer>().sprite;
        itemInside.amount = amount;
        itemInside.strengthRequired = strengthRequired;
        itemInside.stackable = stackable;
        itemInside.requiresStrength = requiresStrength;
        itemInside.type = type;
        **/
    }

    public void SetItem(ItemInstance item)
    {
        itemInside = item;
        GetComponent<SpriteRenderer>().sprite = itemInside.itemImage;
    }

    /**
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {          
            UIManager.instance.pickUpButton.gameObject.SetActive(true);
        }       
    }**/

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager.instance.pickUpButton.gameObject.SetActive(false);
        }
    }
}
