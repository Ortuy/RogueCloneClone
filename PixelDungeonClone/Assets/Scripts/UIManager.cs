using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Image itemPickupImage;
    public Button useButton, throwButton, dropButton, mapButton;

    public GameObject itemMenu;
    public Text itemMenuText, itemDescriptionText;

    public GameObject pauseMenu;
    public GameObject mapMenu;

    public Image fadeImage;
    private bool fadeIn, fadeOut;
    public bool fadeInDone, fadeOutDone;
    public float fadeLength;

    public Canvas canvas;

    public Transform statusDisplayParent;
    public List<Image> statusDisplay;
    public Image statusDisplayTemplate;

    public Text commandText;
    public Button inventoryButton;

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
        SceneManager.sceneLoaded += OnLevelLoaded;
        StartFadeIn();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Cancel") && TurnManager.instance.turnState == TurnState.PLAYER)
        {
            TogglePauseMenu();
        }

        if(fadeIn)
        {
            fadeImage.color = new Color(0, 0, 0, fadeImage.color.a - (Time.deltaTime / fadeLength));
            if(fadeImage.color.a <= 0)
            {
                fadeImage.color = new Color(0, 0, 0, 0);
                fadeIn = false;
                fadeImage.gameObject.SetActive(false);
                fadeInDone = true;
                MouseBlocker.mouseBlocked = false;
            }
        }
        else if (fadeOut)
        {
            fadeImage.color = new Color(0, 0, 0, fadeImage.color.a + (Time.deltaTime / fadeLength));
            if (fadeImage.color.a >= 1)
            {
                fadeImage.color = new Color(0, 0, 0, 1);
                fadeOut = false; ;
                fadeOutDone = true;                
            }
        }
    }

    public void AddStatusToDisplay(Sprite icon)
    {
        statusDisplay.Add(Instantiate(statusDisplayTemplate, statusDisplayParent));
        statusDisplay[statusDisplay.Count - 1].sprite = icon;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        StartFadeIn();
    }

    public void ToggleInventory()
    {
        if(inventory.activeInHierarchy)
        {
            inventory.SetActive(false);
            MouseBlocker.mouseBlocked = false;
        }
        else
        {
            inventory.SetActive(true);
        }
    }

    public void ToggleMap()
    {
        if (mapMenu.activeInHierarchy)
        {
            mapMenu.SetActive(false);
            MouseBlocker.mouseBlocked = false;
        }
        else
        {
            mapMenu.SetActive(true);
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
        if(!InventoryManager.instance.waitingForSelection)
        {
            if (itemMenu.activeInHierarchy)
            {
                itemMenu.SetActive(false);
            }
            else
            {
                itemMenu.SetActive(true);
                StartCoroutine(RefreshItemMenu());
                if (slotID >= 4)
                {
                    throwButton.gameObject.SetActive(true);
                    dropButton.gameObject.SetActive(true);
                    useButton.GetComponentInChildren<Text>().text = "Equip";
                }
                else
                {
                    throwButton.gameObject.SetActive(false);
                    dropButton.gameObject.SetActive(false);                   
                    useButton.GetComponentInChildren<Text>().text = "Unequip";
                }
                itemMenu.GetComponent<ItemMenu>().slotID = slotID;
                itemMenuText.text = InventoryManager.instance.inventoryItems[slotID].itemName;

                itemDescriptionText.text = InventoryManager.instance.inventoryItems[slotID].description;

                if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.WEAPON)
                {                   
                    if (!InventoryManager.instance.inventoryItems[slotID].identified)
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + (InventoryManager.instance.inventoryItems[slotID].baseStatChangeMin) + "-" + InventoryManager.instance.inventoryItems[slotID].baseStatChangeMax + " base damage. Probably.";
                        itemDescriptionText.text = itemDescriptionText.text + "\nThis piece of gear is unidentified. It may hold secrets, pleasant and unpleasant alike.";
                        
                    }
                    else if (InventoryManager.instance.inventoryItems[slotID].cursed)
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + InventoryManager.instance.inventoryItems[slotID].statChangeMin + "-" + InventoryManager.instance.inventoryItems[slotID].statChangeMax + " base damage.";
                        itemDescriptionText.text = itemDescriptionText.text + "\nThis piece of gear is accursed with foul magic. Using it will bind it to your body.";
                        
                    }
                    else
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + InventoryManager.instance.inventoryItems[slotID].statChangeMin + "-" + InventoryManager.instance.inventoryItems[slotID].statChangeMax + " base damage.";
                    }
                }
                else if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.ARMOR)
                {
                    if (!InventoryManager.instance.inventoryItems[slotID].identified)
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + InventoryManager.instance.inventoryItems[slotID].baseStatChangeMin + "-" + InventoryManager.instance.inventoryItems[slotID].baseStatChangeMax + " points of damage per hit. Probably.";
                        itemDescriptionText.text = itemDescriptionText.text + "\nThis piece of armour is unidentified. It may hold secrets, pleasant and unpleasant alike.";
                        
                    }
                    else if (InventoryManager.instance.inventoryItems[slotID].cursed)
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + InventoryManager.instance.inventoryItems[slotID].statChangeMin + "-" + InventoryManager.instance.inventoryItems[slotID].statChangeMax + " points of damage per hit.";
                        itemDescriptionText.text = itemDescriptionText.text + "\nThis piece of armour is accursed with foul magic. Using it will bind it to your body.";
                        
                    }
                    else
                    {
                        itemDescriptionText.text = itemDescriptionText.text + " " + InventoryManager.instance.inventoryItems[slotID].statChangeMin + "-" + InventoryManager.instance.inventoryItems[slotID].statChangeMax + " points of damage per hit.";
                    }
                }

                itemMenu.transform.position = InventoryManager.instance.inventorySlots[slotID].transform.position;

                var popupHeight = itemMenu.GetComponent<RectTransform>().sizeDelta.y;
                var rect = itemMenu.GetComponent<RectTransform>();

                if (rect.anchoredPosition.y - (popupHeight / 2) < -768)
                {
                    Debug.Log("AAAAAA");
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + 10 + (popupHeight / 2) - (768 + rect.anchoredPosition.y));
                }
                else if (rect.anchoredPosition.y + (popupHeight / 2) > 0)
                {
                    Debug.Log("BBBB");
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 10 - ((popupHeight / 2) + rect.anchoredPosition.y));
                }
                else
                {
                    itemMenu.transform.position = InventoryManager.instance.inventorySlots[slotID].transform.position;
                }

                if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.NONE)
                {
                    useButton.gameObject.SetActive(false);
                }
                else
                {
                    if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.POTION)
                    {
                        useButton.GetComponentInChildren<Text>().text = "Drink";
                    }
                    else if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.SCROLL)
                    {
                        useButton.GetComponentInChildren<Text>().text = "Break";
                    }
                    else if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.FOOD)
                    {
                        useButton.GetComponentInChildren<Text>().text = "Eat";
                    }
                    else if (InventoryManager.instance.inventoryItems[slotID].type == ItemType.TORCH)
                    {
                        useButton.GetComponentInChildren<Text>().text = "Light";
                    }

                    useButton.gameObject.SetActive(true);
                    if (slotID < 4 && InventoryManager.instance.inventoryItems[slotID].effectID == 10)
                    {
                        useButton.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            InventoryManager.instance.selectedSlotID = slotID;
            InventoryManager.instance.waitingForSelection = false;
        }
        
    }

    IEnumerator RefreshItemMenu()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)itemMenu.transform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)itemMenu.transform);
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
            accuracyText.text = "Accuracy: " + (Player.stats.GetAccuracy() + Player.stats.accModifier);
            evasionText.text = "Evasion: " + (Player.stats.GetEvasion() + Player.stats.evaModifier);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TogglePauseMenu()
    {
        if (pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            inventory.SetActive(false);
            itemMenu.SetActive(false);
            characterMenu.SetActive(false);
        }
    }

    public void StartFadeIn()
    {
        fadeInDone = false;
        fadeImage.gameObject.SetActive(true);
        fadeIn = true;
    }

    public void StartFadeOut()
    {
        fadeOutDone = false;
        fadeImage.gameObject.SetActive(true);
        fadeOut = true;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }
}
