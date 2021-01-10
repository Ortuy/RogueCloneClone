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
    public GameObject mapMenu, mapKnown, mapUnknown;
    public Text mapText;

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
    private AudioSource audioSource;
    public AudioClip buttonSound, buttonSoundAlt;

    private AudioLowPassFilter[] ambiance;

    [SerializeField]
    private GameObject examinePopup;
    [SerializeField]
    private Text examineNametext, examineDescText;

    public InteractibleObject usedInteractible;
    [SerializeField]
    private GameObject interactionMenu;
    [SerializeField]
    private Text interactionNametext, interactionDescText, interactionButtonText;

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
        audioSource = GetComponent<AudioSource>();
        ambiance = FindObjectsOfType<AudioLowPassFilter>();
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

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayButtonSound(bool alternativeSound)
    {
        if(alternativeSound)
        {
            PlaySound(buttonSoundAlt);
        }
        else
        {
            PlaySound(buttonSound);
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
        ambiance = FindObjectsOfType<AudioLowPassFilter>();
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
            mapText.text = "Floor " + GameManager.instance.currentFloor;
            if(GameManager.instance.mapRevealed)
            {
                mapKnown.SetActive(true);
                mapUnknown.SetActive(false);
            }
            else
            {
                mapKnown.SetActive(false);
                mapUnknown.SetActive(true);
            }
        }
    }

    public void ToggleDeathScreen()
    {
        if (deathScreen.activeInHierarchy)
        {
            deathScreen.SetActive(false);
            UnmuffleAmbiance();
        }
        else
        {
            deathScreen.SetActive(true);
            MuffleAmbiance();
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

                itemDescriptionText.text = ParseItemDescription(InventoryManager.instance.inventoryItems[slotID].description, InventoryManager.instance.inventoryItems[slotID]);              

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
        Destroy(Camera.main);
        Time.timeScale = 1;
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
            UnmuffleAmbiance();
            Time.timeScale = 1;
        }
        else
        {
            pauseMenu.SetActive(true);
            MuffleAmbiance();
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

    public void MuffleAmbiance()
    {
        for(int i = 0; i < ambiance.Length; i++)
        {
            ambiance[i].cutoffFrequency = 2000;
        }
    }

    public void UnmuffleAmbiance()
    {
        for (int i = 0; i < ambiance.Length; i++)
        {
            ambiance[i].cutoffFrequency = 22000;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    private string ParseItemDescription(string baseDesc, ItemInstance item)
    {
        string output = item.description;

        if (item.type == ItemType.WEAPON)
        {
            if (!item.identified)
            {
                output = output + " " + (item.baseStatChangeMin) + "-" + item.baseStatChangeMax + " base damage. Probably.";
                output = output + "\nThis piece of gear is unidentified. It may hold secrets, pleasant and unpleasant alike.";

            }
            else if (item.cursed)
            {
                output = output + " " + item.statChangeMin + "-" + item.statChangeMax + " base damage.";
                output = output + "\nThis piece of gear is accursed with foul magic. Using it will bind it to your body.";

            }
            else
            {
                output = output + " " + item.statChangeMin + "-" + item.statChangeMax + " base damage.";
            }
        }
        else if (item.type == ItemType.ARMOR)
        {
            if (!item.identified)
            {
                output = output + " " + item.baseStatChangeMin + "-" + item.baseStatChangeMax + " points of damage per hit. Probably.";
                output = output + "\nThis piece of armour is unidentified. It may hold secrets, pleasant and unpleasant alike.";

            }
            else if (item.cursed)
            {
                output = output + " " + item.statChangeMin + "-" + item.statChangeMax + " points of damage per hit.";
                output = output + "\nThis piece of armour is accursed with foul magic. Using it will bind it to your body.";

            }
            else
            {
                output = output + " " + item.statChangeMin + "-" + item.statChangeMax + " points of damage per hit.";
            }
        }
        else if(item.identified && item.cursed)
        {
            output = output + "\nThis item is accursed with foul magic. Using it will bind it to your body.";
        }

        return output;
    }

    public void ShowExaminePopup(Vector3 pos, string objectName, string objectDesc)
    {
        examinePopup.gameObject.SetActive(true);
        examinePopup.transform.position = pos;
        examineNametext.text = objectName;
        examineDescText.text = objectDesc;
        PlayButtonSound(false);
        StartCoroutine(RefreshPopup((RectTransform)examinePopup.transform));
    }

    public void ShowItemExaminePopup(Vector3 pos, ItemInstance item)
    {
        examinePopup.gameObject.SetActive(true);
        examinePopup.transform.position = pos;
        examineNametext.text = item.itemName;
        examineDescText.text = ParseItemDescription(item.description, item);
        PlayButtonSound(true);
        StartCoroutine(RefreshPopup((RectTransform)examinePopup.transform));
    }

    public void ToggleInteractionMenu()
    {
        if(interactionMenu.gameObject.activeInHierarchy)
        {
            interactionMenu.gameObject.SetActive(false);
            MouseBlocker.mouseBlocked = false;
        }
        else
        {
            interactionMenu.gameObject.SetActive(true);
            Debug.Log(usedInteractible);
            interactionNametext.text = usedInteractible.interactionName;
            interactionDescText.text = usedInteractible.interactionDescription;
            interactionButtonText.text = usedInteractible.buttonText;
            StartCoroutine(RefreshPopup((RectTransform)interactionMenu.transform));
        }
    }

    public void DoInteraction()
    {
        usedInteractible.DoInteraction();
        ToggleInteractionMenu();
    }

    IEnumerator RefreshPopup(RectTransform popup)
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
    }
}
