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

    //public Item weapon, armour;
    //public InventorySlot weaponSlot, armourSlot;

    public int goldAmount;
    public Text goldText;

    

    public ItemPickup itemTemplate;

    public Color cursedColor, unknownColor, upgradeColor;

    public int selectedSlotID;
    public bool waitingForSelection;

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
    }

    public void AddItem(ItemInstance itemToAdd)
    {
        for(int i = 4; i < inventoryItems.Length; i++)
        {
            if(inventoryItems[i] != null && inventoryItems[i].itemImage == itemToAdd.itemImage && itemToAdd.stackable)
            {
                inventoryItems[i].amount += itemToAdd.amount;
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
            else if(inventoryItems[i] == null)
            {
                Debug.Log(itemToAdd.level);
                inventoryItems[i] = itemToAdd;
                if (itemToAdd.type == ItemType.POTION)
                {
                    IdentifyingMenager.instance.CheckIfPotionIdentified(inventoryItems[i]);
                }
                else if(itemToAdd.type == ItemType.SCROLL)
                {
                    IdentifyingMenager.instance.CheckIfScrollIdentified(inventoryItems[i]);
                }
                inventorySlots[i].UpdateItem(inventoryItems[i]);
                break;
            }
        }
    }

    public void AddItem(ItemInstance itemToAdd, out bool success)
    {
        success = true;
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
        switch (inventoryItems[slotID].effectID)
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
                Player.stats.AddStatusEffect(new BlindnessEffect(0, 16, Player.stats));
                break;

        }
        IdentifyingMenager.instance.IdentifyPotion(inventoryItems[slotID]);
        SubtractItem(slotID);
        UIManager.instance.ToggleInventory();
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
                    equipment[Random.Range(0, equipment.Count)].cursed = true;
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
                Player.stats.AddStatusEffect(new BlindnessEffect(0, 12, Player.stats));
                UIManager.instance.ToggleInventory();
                break;
            case 6:
                TurnManager.instance.playerExtraTurns += 4;
                UIManager.instance.ToggleInventory();
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
                break;
            case 8:
                var generator = FindObjectOfType<LevelGenerator>();
                int roomID = Random.Range(0, generator.rooms.Count);

                int posX = Random.Range(generator.rooms[roomID].minCorner.x, generator.rooms[roomID].maxCorner.x + 1);
                int posY = Random.Range(generator.rooms[roomID].minCorner.y, generator.rooms[roomID].maxCorner.y + 1);

                Player.movement.StopMovement();
                Player.instance.transform.position = new Vector2(posX + 0.5f, posY + 0.5f);

                UIManager.instance.ToggleInventory();
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
        }
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
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
        }
        inventoryItems[selectedSlotID].cursed = false;
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
    }

    IEnumerator TransmuteItem()
    {
        UIManager.instance.commandText.text = "Transmute an item";
        UIManager.instance.inventoryButton.interactable = false;
        waitingForSelection = true;
        yield return StartCoroutine(WaitForSelection());
        IdentifyingMenager.instance.TransmuteItem(selectedSlotID);
        inventorySlots[selectedSlotID].UpdateItem(inventoryItems[selectedSlotID]);
        if(selectedSlotID < 4 && inventoryItems[selectedSlotID].strengthRequired > Player.stats.GetStrength())
        {
            if (inventoryItems[inventoryItems.Length - 1] == null || inventoryItems[inventoryItems.Length - 1].amount < 1)
            {
                AddItem(inventoryItems[selectedSlotID]);
            }
            else
            {
                ItemPickup temp = Instantiate(itemTemplate, Player.instance.transform.position, Quaternion.identity);
                temp.SetItem(new ItemInstance(inventoryItems[selectedSlotID], 1));
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
        UIManager.instance.inventoryButton.interactable = true;
        UIManager.instance.commandText.text = "Inventory";
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
            }
            else if(inventoryItems[slotID].type == ItemType.SCROLL)
            {
                UseScroll(slotID);
            }
            else if(inventoryItems[slotID].type == ItemType.FOOD)
            {
                Player.stats.foodPoints += 192;
                Player.actions.turnCost = 3;
                SubtractItem(slotID);
                UIManager.instance.ToggleInventory();
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
