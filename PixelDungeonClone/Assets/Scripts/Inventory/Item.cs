using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public string itemName;
    public Sprite itemImage;
    public int amount;
    public int strengthRequired;
    public bool stackable = true;
    public bool requiresStrength = false;

    public Item(Item baseItem, int newAmount)
    {
        itemName = baseItem.itemName;
        itemImage = baseItem.itemImage;
        amount = newAmount;
        strengthRequired = baseItem.strengthRequired;
        stackable = baseItem.stackable;
        requiresStrength = baseItem.requiresStrength;
    }

    public Item()
    {

    }
}
