using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { NONE, WEAPON, ARMOR }

[CreateAssetMenu(fileName = "Item", menuName = "Item", order = 1)]
public class Item : ScriptableObject
{
    public string itemName;

    public string description;

    public Sprite itemImage;
    public int amount;
    public int strengthRequired;
    public bool stackable = true;
    //TODO: Use strength = -1 instead
    public bool requiresStrength = false;

    public ItemType type = ItemType.NONE;

    public int statChangeMin, statChangeMax;

    public Item(Item baseItem, int newAmount)
    {
        itemName = baseItem.itemName;
        description = baseItem.description;
        itemImage = baseItem.itemImage;
        amount = newAmount;
        strengthRequired = baseItem.strengthRequired;
        stackable = baseItem.stackable;
        requiresStrength = baseItem.requiresStrength;

        type = baseItem.type;
    }

    public Item()
    {

    }
}
