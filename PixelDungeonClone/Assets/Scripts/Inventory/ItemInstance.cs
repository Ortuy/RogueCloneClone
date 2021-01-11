using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance
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
    public int baseStatChangeMin, baseStatChangeMax;
    public int effectID;
    public bool identified;
    public int level;
    public bool cursed;

    public bool pickedUpOnce;

    public int levelFactor;

    public ItemInstance(Item baseItem, int newAmount)
    {
        itemName = baseItem.itemName;
        description = baseItem.description.Replace("$n", "\n");
        itemImage = baseItem.itemImage;
        amount = newAmount;
        strengthRequired = baseItem.strengthRequired;
        stackable = baseItem.stackable;
        requiresStrength = baseItem.requiresStrength;
        statChangeMin = baseItem.statChangeMin;
        statChangeMax = baseItem.statChangeMax;
        baseStatChangeMin = statChangeMin;
        baseStatChangeMax = statChangeMax;
        effectID = baseItem.effectID;
        identified = baseItem.identified;
        type = baseItem.type;
        level = 0;
        cursed = false;
        levelFactor = 1 + Mathf.RoundToInt((statChangeMin + statChangeMax) / 20);
        pickedUpOnce = false;
    }

    public ItemInstance(ItemInstance baseItemInstance, int newAmount)
    {
        itemName = baseItemInstance.itemName;
        description = baseItemInstance.description.Replace("$n", "\n");
        itemImage = baseItemInstance.itemImage;
        amount = newAmount;
        strengthRequired = baseItemInstance.strengthRequired;
        stackable = baseItemInstance.stackable;
        requiresStrength = baseItemInstance.requiresStrength;
        statChangeMin = baseItemInstance.statChangeMin;
        statChangeMax = baseItemInstance.statChangeMax;
        baseStatChangeMin = baseItemInstance.baseStatChangeMin;
        baseStatChangeMax = baseItemInstance.baseStatChangeMax;
        effectID = baseItemInstance.effectID;
        identified = baseItemInstance.identified;
        type = baseItemInstance.type;
        level = baseItemInstance.level;
        cursed = baseItemInstance.cursed;
        pickedUpOnce = baseItemInstance.pickedUpOnce;
        levelFactor = baseItemInstance.levelFactor;
    }

    public void LevelUp(int levelAmount)
    {
        level += levelAmount;
        statChangeMin += levelAmount * levelFactor;
        statChangeMax += levelAmount * levelFactor;
    }
}
