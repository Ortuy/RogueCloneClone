using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider playerHealthBar;

    public static UIManager instance;

    [SerializeField]
    private GameObject inventory;

    [SerializeField]
    private GameObject characterMenu;

    [SerializeField]
    private GameObject deathScreen;

    public Text levelText;
    public Text xpText;
    public Text healthText;
    public Text strengthText;
    public Text accuracyText;
    public Text evasionText;

    public Sprite nullSprite;

    public Button pickUpButton;
    public Button useButton;

    public GameObject itemMenu;
    public Text itemMenuText, itemDescriptionText;

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
    }

    public void ToggleInventory()
    {
        if(inventory.activeInHierarchy)
        {
            inventory.SetActive(false);
        }
        else
        {
            inventory.SetActive(true);
        }
    }

    public void ToggleDeathScreen()
    {
        if (deathScreen.activeInHierarchy)
        {
            deathScreen.SetActive(false);
        }
        else
        {
            deathScreen.SetActive(true);
        }
    }

    public void ToggleItemMenu(int slotID)
    {
        if (itemMenu.activeInHierarchy)
        {
            itemMenu.SetActive(false);
        }
        else
        {
            itemMenu.SetActive(true);
            if(slotID >= 4)
            {
                useButton.GetComponentInChildren<Text>().text = "Equip";
            }
            else
            {
                useButton.GetComponentInChildren<Text>().text = "Unequip";
            }
            itemMenu.GetComponent<ItemMenu>().slotID = slotID;
            itemMenuText.text = InventoryManager.instance.inventoryItems[slotID].itemName;
            itemDescriptionText.text = InventoryManager.instance.inventoryItems[slotID].description;
            itemMenu.transform.position = InventoryManager.instance.inventorySlots[slotID].transform.position;
            if(InventoryManager.instance.inventoryItems[slotID].type == ItemType.NONE)
            {
                useButton.gameObject.SetActive(false);
            }
            else
            {
                useButton.gameObject.SetActive(true);
            }
        }
    }

    public void ToggleCharacterMenu()
    {
        if (characterMenu.activeInHierarchy)
        {
            characterMenu.SetActive(false);
        }
        else
        {
            characterMenu.SetActive(true);
            levelText.text = "Level: " + Player.stats.GetLevel();
            xpText.text = "XP: " + Player.stats.GetCurrentXP() + "/" + Player.stats.GetRequiredXP();
            healthText.text = "HP: " + Player.stats.GetHealth() + "/" + Player.stats.GetMaxHealth();
            strengthText.text = "Strength: " + Player.stats.GetStrength();
            accuracyText.text = "Accuracy: " + Player.stats.GetAccuracy();
            evasionText.text = "Evasion: " + Player.stats.GetEvasion();
        }
    }
}
