using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public Image backgroundIcon;
    public Button itemButton;
    public Text amountText, requirementText;

    private void Start()
    {
        itemIcon = itemButton.GetComponent<Image>();
    }

    public void UpdateItem(Item item)
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
            amountText.text = "x" + item.amount;
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }

        if(item.requiresStrength)
        {
            requirementText.gameObject.SetActive(true);
            requirementText.text = "" + item.strengthRequired;
        }
        else
        {
            requirementText.gameObject.SetActive(false);
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
        if (backgroundIcon != null)
        {
            backgroundIcon.gameObject.SetActive(true);
        }
    }
}
