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
    public ItemInstance[] inventoryItems;
    public InventorySlot[] inventorySlots;
    public ParticleSystem[] scrollUseFX, potionUseFX;
    public AudioClip potionSound, weaponSound, armourSound, ringSound, scrollSound, torchSound, foodSound, itemDropSound;

    //public Item weapon, armour;
    //public InventorySlot weaponSlot, armourSlot;

    public int goldAmount;
    public Text goldText;

    public Gas[] potionGases;

    public ItemPickup itemTemplate;

    public Color cursedColor, unknownColor, upgradeColor;

    public int selectedSlotID;
    public bool waitingForSelection;

    public int[] ringEquipped;

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

        inventoryItems = new ItemInstance[23];
        
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].ResetItem();
        }

        UIManager.instance.ToggleInventory();

        goldText.text = "" + goldAmount;
        ringEquipped = new int[]{ -1, -1};
    }

    public void AddItem(ItemInstance itemToAdd)
    {
        AddItem(itemToAdd, out bool succ);
    }

    public void AddItem(ItemInstance itemToAdd, out bool success)
    {
        success = true;
        
        if(!itemToAdd.pickedUpOnce)
        {
            if (ringEquipped[0] == 8 && !inventoryItems[2].cursed)
            {
                int chance = 10 * inventoryItems[2].baseStatChangeMax;
                if (Random.Range(0, 100) < chance)
                {
                    Debug.LogWarning("The ring worked!");
                    IdentifyingMenager.instance.IdentifyItem(itemToAdd);
                }
            }
            if (ringEquipped[1] == 8 && !inventoryItems[3].cursed)
            {
                int chance = 10 * inventoryItems[3].baseStatChangeMax;
                if (Random.Range(0, 100) < chance)
                {
                    Debug.LogWarning("The ring worked!");
                    IdentifyingMenager.instance.IdentifyItem(itemToAdd);
                }
            }
            itemToAdd.pickedUpOnce = true;
        }        
        
        for (int i = 4; i < inventoryItems.Length; i++)
        {
            if (inventoryItems[i] != null && inventoryItems[i].itemImage == itemToAdd.itemImage && itemToAdd.stackable)
            {
                inventoryItems[i].amount += itemToAdd.amount;
                success = true;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else if (inventoryItems[i] == null)
            {
                Debug.Log(itemToAdd.level);
                inventoryItems[i] = itemToAdd;
                if (itemToAdd.type == ItemType.POTION)
                {
                    IdentifyingMenager.instance.CheckIfPotionIdentified(inventoryItems[i]);
                }
                else if (itemToAdd.type == ItemType.SCROLL)
                {
                    IdentifyingMenager.instance.CheckIfScrollIdentified(inventoryItems[i]);
                }
                else if (itemToAdd.type == ItemType.RING)
                {
                    IdentifyingMenager.instance.CheckIfRingIdentified(inventoryItems[i]);
                }
                success = true;
                
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else
            {
                //Debug.Log("asdfghhdgs");
                success = false;
            }
        }       
    }

    public void SubtractItem(ItemInstance itemToSubtract)
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

    public void DrinkPotion(int slotID)
    {
        var effID = inventoryItems[slotID].effectID;
        switch (effID)
        {
            case 0:
                Player.stats.Heal(Mathf.RoundToInt(Player.stats.GetMaxHealth() * 0.5f));
                break;
            case 1:
                Player.stats.IncreaseStrength();
                break;
            case 2:
                Player.stats.LevelUp();
                break;
            case 3:
                Player.stats.AddStatusEffect(new CleansingEffect(0, 24, Player.stats));
                break;
            case 4:
                Player.stats.AddStatusEffect(new InvisibilityEffect(0, 24, Player.stats));
                break;
            case 5:
                Player.stats.AddStatusEffect(new PoisonEffect(Player.stats.GetMaxHealth() / 20, 8, Player.stats));
                break;
            case 6:
                Player.stats.AddStatusEffect(new FrostEffect(0, 4, Player.stats));
                break;
            case 7:
                Player.stats.AddStatusEffect(new WeaknessEffect(0, 16, Player.stats));
                break;
            case 8:
                Player.stats.AddStatusEffect(new FragilityEffect(0, 16, Player.stats));
                break;
            case 9:
                Player.stats.AddStatusEffect(new BlindnessEffect(1.1f, 16, Player.stats));
                break;

        }
        IdentifyingMenager.instance.IdentifyPotion(inventoryItems[slotID]);
        SubtractItem(slotID);
        UIManager.instance.ToggleInventory();
        potionUseFX[effID].Play();
    }

    private void EquipRing(int slotID)
    {
        var effID = inventoryItems[slotID].effectID;
        var modifier = inventoryItems[slotID].statChangeMax;
        if(inventoryItems[slotID].cursed)
        {
            modifier = -modifier;
        }
        ringEquipped[slotID - 2] = effID;
        switch (effID)
        {
            case 0:
                Player.stats.evaModifier += modifier;
                break;
            case 1:
                Player.stats.accModifier += modifier;
                break;
            case 2:
                Player.stats.IncreaseStrength(modifier);
                break;
            case 3:
                Player.stats.CheckForWrathRingModifiers(Player.stats.GetHealth(), Player.stats.GetMaxHealth());
                break;
            case 9:
                Player.stats.evaModifier -= modifier;
                Player.stats.accModifier -= modifier;
                Player.stats.IncreaseMaxHP(-2*modifier);
                break;
        }
    }

    private void UnequipRing(int slotID)
    {
        var effID = inventoryItems[slotID].effectID;
        var modifier = inventoryItems[slotID].statChangeMax;
        if (inventoryItems[slotID].cursed)
        {
            modifier = -modifier;
        }
        switch (effID)
        {
            case 0:
                Player.stats.evaModifier -= modifier;
                break;
            case 1:
                Player.stats.accModifier -= modifier;
                break;
            case 2:
                Player.stats.IncreaseStrength(-modifier);
                break;
            case 3:
                Player.stats.CheckForWrathRingModifiers(Player.stats.GetMaxHealth(), Player.stats.GetHealth());
                break;
            case 9:
                Player.stats.evaModifier += modifier;
                Player.stats.accModifier += modifier;
                Player.stats.IncreaseMaxHP(2*modifier);
                break;
        }
        ringEquipped[slotID - 2] = -1;
    }

    private void UseScroll(int slotID)
    {
        IdentifyingMenager.instance.IdentifyScroll(inventoryItems[slotID]);
        var effectID = inventoryItems[slotID].effectID;
        SubtractItem(slotID);
        switch(effectID)
        {
            case 0:
                StartCoroutine(IdentifyItem());
                break;
            case 1:
                StartCoroutine(UpgradeItem());
                break;
            case 2:
                StartCoroutine(CleanseItem());
                break;
            case 3:
                StartCoroutine(TransmuteItem());
                break;
            case 4:
                List<ItemInstance> equipment = new List<ItemInstance>();
                for(int i = 0; i < 4; i++)
                {
                    if(inventoryItems[i] != null)
                    {
                        equipment.Add(inventoryItems[i]);
                    }
                }
                if(equipment.Count > 0)
                {
                    var rand = Random.Range(0, equipment.Count);
                    var previouslyCursed = equipment[rand].cursed;

                    equipment[rand].cursed = true;

                    if(equipment[rand].type == ItemType.RING && !previouslyCursed)
                    {
                        for(int i = 2; i < 4; i++)
                        {
                            if(inventoryItems[i] == equipment[rand])
                            {
                                UpdateItemModifiers(i);
                                UpdateItemModifiers(i);
                            }
                        }
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (inventoryItems[i] != null)
                    {
                        inventorySlots[i].UpdateItem(inventoryItems[i]);
                    }
                }
                break;
            case 5:
                var nearbyEnemies = Physics2D.OverlapCircleAll(Player.instance.transform.position, 6f, LayerMask.GetMask("Enemies"));
                int damage = Mathf.RoundToInt((3 + Player.stats.GetLevel()) / (Player.stats.GetHealth() / Player.stats.GetMaxHealth()));
                for(int i = 0; i < nearbyEnemies.Length; i++)
                {
                    var enemy = nearbyEnemies[i].GetComponent<Enemy>();
                    if(enemy != null)
                    {
                        enemy.TakeTrueDamage(damage);
                    }
                }
                Player.stats.AddStatusEffect(new WeaknessEffect(0, 12, Player.stats));
                Player.stats.AddStatusEffect(new FragilityEffect(0, 12, Player.stats));
                Player.stats.AddStatusEffect(new BlindnessEffect(1.1f, 12, Player.stats));
                UIManager.instance.ToggleInventory();
                scrollUseFX[0].Play();
                break;
            case 6:
                TurnManager.instance.playerExtraTurns += 4;
                UIManager.instance.ToggleInventory();
                scrollUseFX[1].Play();
                break;
            case 7:
                nearbyEnemies = Physics2D.OverlapCircleAll(Player.instance.transform.position, 6f, LayerMask.GetMask("Enemies"));
                for (int i = 0; i < nearbyEnemies.Length; i++)
                {
                    var enemy = nearbyEnemies[i].GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        var temp = new List<Entity>();
                        for(int j = 0; j < nearbyEnemies.Length; j++)
                        {
                            if(j != i)
                            {
                                temp.Add(nearbyEnemies[j].GetComponent<Entity>());
                            }
                        }
                        enemy.GoAmok(temp[Random.Range(0, temp.Count)]);
                    }
                }
                UIManager.instance.ToggleInventory();
                scrollUseFX[2].Play();
                break;
            case 8:
                var generator = FindObjectOfType<LevelGenerator>();
                int roomID = Random.Range(0, generator.rooms.Count);

                int posX = Random.Range(generator.rooms[roomID].minCorner.x, generator.rooms[roomID].maxCorner.x + 1);
                int posY = Random.Range(generator.rooms[roomID].minCorner.y, generator.rooms[roomID].maxCorner.y + 1);

                Player.movement.StopMovement();
                Player.instance.transform.position = new Vector2(posX + 0.5f, posY + 0.5f);

                UIManager.instance.ToggleInventory();
                scrollUseFX[3].Play();
                break;
        }
        
        
    }

    IEnumerator IdentifyItem()
    {
        UIManager.instance.commandText.text = "Identify an item";
        UIManager.instance.inventoryButton.interactable = false;
        waitingForSelection = true;
        yield return StartCoroutine(WaitForSelection());
        IdentifyingMenager.instance.IdentifyItem(inventoryItems[selectedSlotID]);
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
        Player.movement.PlaySound(scrollSound);
    }

    IEnumerator UpgradeItem()
    {
        UIManager.instance.commandText.text = "Upgrade an item";
        UIManager.instance.inventoryButton.interactable = false;
        waitingForSelection = true;
        yield return StartCoroutine(WaitForSelection());
        if(inventoryItems[selectedSlotID].requiresStrength)
        {
            inventoryItems[selectedSlotID].LevelUp(1);
            UpdateItemModifiers(selectedSlotID);
        }
        else if(inventoryItems[selectedSlotID].type == ItemType.RING)
        {
            if(selectedSlotID < 4)
            {
                UnequipRing(selectedSlotID);
            }
            
            inventoryItems[selectedSlotID].LevelUp(1);
            if (selectedSlotID < 4)
            {
                UpdateItemModifiers(selectedSlotID);
            }
            
        }
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
        Player.movement.PlaySound(scrollSound);
    }

    IEnumerator CleanseItem()
    {
        UIManager.instance.commandText.text = "Cleanse an item";
        UIManager.instance.inventoryButton.interactable = false;
        waitingForSelection = true;
        yield return StartCoroutine(WaitForSelection());
        if (inventoryItems[selectedSlotID].level < 0)
        {
            inventoryItems[selectedSlotID].LevelUp(0 - inventoryItems[selectedSlotID].level);
            UpdateItemModifiers(selectedSlotID);
        }
        inventoryItems[selectedSlotID].cursed = false;
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
        Player.movement.PlaySound(scrollSound);
    }

    IEnumerator TransmuteItem()
    {
        UIManager.instance.commandText.text = "Transmute an item";
        UIManager.instance.inventoryButton.interactable = false;
        waitingForSelection = true;
        yield return StartCoroutine(WaitForSelection());
        if(inventoryItems[selectedSlotID].type == ItemType.RING && selectedSlotID < 4)
        {
            UnequipRing(selectedSlotID);
        }
        IdentifyingMenager.instance.TransmuteItem(selectedSlotID);
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UpdateItemModifiers(selectedSlotID);
        if (selectedSlotID < 4 && inventoryItems[selectedSlotID].strengthRequired > Player.stats.GetStrength())
        {
            if (inventoryItems[inventoryItems.Length - 1] == null || inventoryItems[inventoryItems.Length - 1].amount < 1)
            {
                AddItem(inventoryItems[selectedSlotID]);
            }
            else
            {
                ItemPickup temp = Instantiate(itemTemplate, Player.instance.transform.position, Quaternion.identity);
                temp.SetItem(new ItemInstance(inventoryItems[selectedSlotID], 1));
                Player.movement.PlaySound(itemDropSound);
            }

            if (inventoryItems[selectedSlotID].type == ItemType.WEAPON)
            {
                Player.stats.ResetDamage();
            }
            else if (inventoryItems[selectedSlotID].type == ItemType.ARMOR)
            {
                Player.stats.ResetDefence();
            }

            SubtractItem(selectedSlotID);
        }
        if (inventoryItems[selectedSlotID].type == ItemType.RING && selectedSlotID < 4)
        {
            EquipRing(selectedSlotID);
        }
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
        Player.movement.PlaySound(scrollSound);
    }

    IEnumerator WaitForSelection()
    {
        while (waitingForSelection)
            yield return null;
    }

    public void EquipItem(int slotID)
    {
        if(slotID >= 4)
        {
            if(inventoryItems[slotID].type == ItemType.POTION)
            {
                DrinkPotion(slotID);
                Player.movement.PlaySound(potionSound);
            }
            else if(inventoryItems[slotID].type == ItemType.SCROLL)
            {
                UseScroll(slotID);
                Player.movement.PlaySound(scrollSound);
            }
            else if (inventoryItems[slotID].type == ItemType.TORCH)
            {
                Player.stats.AddStatusEffect(new TorchEffect(1, 96, Player.stats));
                SubtractItem(slotID);
                Player.movement.PlaySound(torchSound);
                potionUseFX[11].Play();
                UIManager.instance.ToggleInventory();
            }
            else if(inventoryItems[slotID].type == ItemType.FOOD)
            {
                int foodmodifier = 0;

                if(ringEquipped[0] == 5)
                {
                    if(inventoryItems[2].cursed)
                    {
                        foodmodifier -= 24 * inventoryItems[2].baseStatChangeMax;
                    }
                    else
                    {
                        foodmodifier += 24 * inventoryItems[2].baseStatChangeMax;
                    }
                }
                if (ringEquipped[1] == 5)
                {
                    if (inventoryItems[3].cursed)
                    {
                        foodmodifier -= 24 * inventoryItems[3].baseStatChangeMax;
                    }
                    else
                    {
                        foodmodifier += 24 * inventoryItems[3].baseStatChangeMax;
                    }
                }

                Player.stats.foodPoints += 192 + foodmodifier;
                Player.actions.turnCost = 3;
                SubtractItem(slotID);
                UIManager.instance.ToggleInventory();
                Player.movement.PlaySound(foodSound);
            }
            else if (inventoryItems[slotID].type == ItemType.WEAPON && Player.stats.GetStrength() >= inventoryItems[slotID].strengthRequired)
            {
                IdentifyingMenager.instance.IdentifyItem(inventoryItems[slotID]);
                if(inventoryItems[0] == null || (inventoryItems[0] != null && !inventoryItems[0].cursed))
                {
                    ItemInstance temp = inventoryItems[slotID];
                    SubtractItem(slotID);
                    if (inventoryItems[0] != null)
                    {
                        AddItem(inventoryItems[0]);
                    }

                    inventoryItems[0] = temp;
                    Player.stats.minBaseDamage = inventoryItems[0].statChangeMin;
                    Player.stats.maxBaseDamage = inventoryItems[0].statChangeMax;
                    inventorySlots[0].UpdateItem(inventoryItems[0]);

                    switch(inventoryItems[0].effectID)
                    {
                        case 1:
                            Player.stats.accModifier += 3;
                            Player.stats.evaModifier += 3;
                            break;
                        case 2:
                            Player.actions.attackExtraTurnCost++;
                            break;
                    }

                    Player.movement.PlaySound(weaponSound);
                }               
            }
            else if (inventoryItems[slotID].type == ItemType.ARMOR && Player.stats.GetStrength() >= inventoryItems[slotID].strengthRequired)
            {
                IdentifyingMenager.instance.IdentifyItem(inventoryItems[slotID]);
                if (inventoryItems[1] == null || (inventoryItems[1] != null && !inventoryItems[1].cursed))
                {
                    ItemInstance temp = inventoryItems[slotID];
                    SubtractItem(slotID);
                    if (inventoryItems[1] != null)
                    {
                        AddItem(inventoryItems[1]);
                    }

                    inventoryItems[1] = temp;
                    Player.stats.minDefence = inventoryItems[1].statChangeMin;
                    Player.stats.maxDefence = inventoryItems[1].statChangeMax;
                    inventorySlots[1].UpdateItem(inventoryItems[1]);
                    Player.movement.PlaySound(armourSound);
                }               
            }
            else if (inventoryItems[slotID].type == ItemType.RING)
            {
                IdentifyingMenager.instance.IdentifyItem(inventoryItems[slotID]);
                if (inventoryItems[2] == null)
                {
                    ItemInstance temp = inventoryItems[slotID];
                    SubtractItem(slotID);
                    inventoryItems[2] = temp;
                    EquipRing(2);
                    inventorySlots[2].UpdateItem(inventoryItems[2]);
                    Player.movement.PlaySound(ringSound);
                }
                else if (inventoryItems[3] == null)
                {
                    ItemInstance temp = inventoryItems[slotID];
                    SubtractItem(slotID);
                    inventoryItems[3] = temp;
                    EquipRing(3);
                    inventorySlots[3].UpdateItem(inventoryItems[3]);
                    Player.movement.PlaySound(ringSound);
                }
            }
        }
        else if(!inventoryItems[slotID].cursed)
        {
            if(inventoryItems[inventoryItems.Length - 1] == null || inventoryItems[inventoryItems.Length - 1].amount < 1)
            {
                AddItem(inventoryItems[slotID]);
            }
            else
            {
                ItemPickup temp = Instantiate(itemTemplate, Player.instance.transform.position, Quaternion.identity);
                temp.SetItem(new ItemInstance(inventoryItems[slotID], 1));
                Player.movement.PlaySound(itemDropSound);
            }

            if (inventoryItems[slotID].type == ItemType.WEAPON)
            {
                Player.stats.ResetDamage();

                switch (inventoryItems[0].effectID)
                {
                    case 1:
                        Player.stats.accModifier -= 3;
                        Player.stats.evaModifier -= 3;
                        break;
                    case 2:
                        Player.actions.attackExtraTurnCost--;
                        break;
                }

                Player.movement.PlaySound(weaponSound);
            }
            else if(inventoryItems[slotID].type == ItemType.ARMOR)
            {
                Player.stats.ResetDefence();
                Player.movement.PlaySound(armourSound);
            }
            else
            {
                UnequipRing(slotID);
                Player.movement.PlaySound(ringSound);
            }

            SubtractItem(slotID);
        }
    }

    public void UpdateItemModifiers(int slotID)
    {
        switch(slotID)
        {
            case 0:
                Player.stats.minBaseDamage = inventoryItems[0].statChangeMin;
                Player.stats.maxBaseDamage = inventoryItems[0].statChangeMax;
                break;
            case 1:
                Player.stats.minDefence = inventoryItems[1].statChangeMin;
                Player.stats.maxDefence = inventoryItems[1].statChangeMax;
                break;
            case 2:
                EquipRing(2);
                break;
            case 3:
                EquipRing(3);
                break;
        }
    }

    public void AddGold(int gold)
    {
        goldAmount += gold;
        goldText.text = "" + goldAmount;
    }
}
