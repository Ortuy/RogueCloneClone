using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public Image backgroundIcon, curseIcon;
    public Button itemButton;
    public Text amountText, requirementText, levelText;

    private void Start()
    {
        itemIcon = itemButton.GetComponent<Image>();
        amountText.fontSize = 16;
        requirementText.fontSize = 16;
        levelText.fontSize = 16;
        levelText.fontStyle = FontStyle.Bold;
    }

    //TODO: using string builders
    public void UpdateItem(ItemInstance item)
    {
        if(backgroundIcon != null)
        {
            backgroundIcon.gameObject.SetActive(false);
        }      

        itemButton.interactable = true;

        itemIcon.sprite = item.itemImage;
        
        if(item.stackable && item.amount > 1)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = " x" + item.amount;
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }

        if(item.requiresStrength)
        {
            requirementText.gameObject.SetActive(true);
            requirementText.text = " " + item.strengthRequired;
        }
        else
        {
            requirementText.gameObject.SetActive(false);
        }

        if(item.identified && item.requiresStrength)
        {
            if(item.level > 0)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = "+" + item.level + " ";
                levelText.color = InventoryManager.instance.upgradeColor;
            }
            else if(item.level < 0)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = "" + item.level + " ";
                levelText.color = InventoryManager.instance.cursedColor;
            }
            else
            {
                levelText.gameObject.SetActive(false);
            }
            if(item.cursed)
            {
                curseIcon.gameObject.SetActive(true);
            }
            else
            {
                curseIcon.gameObject.SetActive(false);
            }
        }
        else if(item.requiresStrength)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = "? ";
            levelText.color = InventoryManager.instance.unknownColor;
            curseIcon.gameObject.SetActive(false);
        }
        else
        {
            levelText.gameObject.SetActive(false);
            curseIcon.gameObject.SetActive(false);
        }
    }

    public bool IsEmpty()
    {
        return !itemButton.interactable;
    }

    public void ResetItem()
    {
        itemIcon.sprite = UIManager.instance.nullSprite;
        itemButton.interactable = false;
        amountText.gameObject.SetActive(false);
        requirementText.gameObject.SetActive(false);
        curseIcon.gameObject.SetActive(false);
        levelText.gameObject.SetActive(false);
        if (backgroundIcon != null)
        {
            backgroundIcon.gameObject.SetActive(true);
        }
    }
}
