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

    public Sprite nullSprite;

    public Button pickUpButton;

    public GameObject itemMenu;
    public Text itemMenuText;

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

    public void ToggleItemMenu(int slotID)
    {
        if (itemMenu.activeInHierarchy)
        {
            itemMenu.SetActive(false);
        }
        else
        {
            itemMenu.SetActive(true);
            itemMenu.GetComponent<ItemMenu>().slotID = slotID;
            itemMenuText.text = InventoryManager.instance.inventoryItems[slotID].itemName;
            itemMenu.transform.position = InventoryManager.instance.inventorySlots[slotID].transform.position;
        }
    }
}
