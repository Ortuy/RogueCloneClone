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
    public Button useButton;

    public GameObject itemMenu;
    public Text itemMenuText, itemDescriptionText;

    public GameObject pauseMenu;

    public Image fadeImage;
    private bool fadeIn, fadeOut;
    public bool fadeInDone, fadeOutDone;
    public float fadeLength;

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

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        StartFadeIn();
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
                if(InventoryManager.instance.inventoryItems[slotID].type == ItemType.POTION)
                {
                    useButton.GetComponentInChildren<Text>().text = "Use";
                }
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
