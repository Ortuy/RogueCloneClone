using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    public ItemInstance shopItem;
    public Item shopItemPrefab;

    private DecorativeObject decorativeObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        SetTileUnwalkable();
        SetItemsAside();       

        if (shopItem == null)
        {
            shopItem = new ItemInstance(shopItemPrefab, 1);

        }

        if(shopItem.type == ItemType.POTION)
        {
            IdentifyingMenager.instance.CheckIfPotionIdentified(shopItem);
        }
        else if (shopItem.type == ItemType.SCROLL)
        {
            IdentifyingMenager.instance.CheckIfScrollIdentified(shopItem);
        }
        else if (shopItem.type == ItemType.RING)
        {
            IdentifyingMenager.instance.CheckIfRingIdentified(shopItem);
        }

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }

        buttonText = buttonText.Replace("$cost", shopItem.goldPrice.ToString());
        decorativeObject.objectName = shopItem.itemName;
        interactionName = shopItem.itemName;
        decorativeObject.objectDesc = UIManager.instance.ParseItemDescription(shopItem.description, shopItem);
        interactionDescription = UIManager.instance.ParseItemDescription(shopItem.description, shopItem);

        secondaryObjects[0].sprite = shopItem.itemImage;
    }

    public override void StartInteraction()
    {
        base.StartInteraction();
        if (InventoryManager.instance.goldAmount < shopItem.goldPrice)
        {
            UIManager.instance.interactionButton.interactable = false;
        }
    }

    public override void DoInteraction()
    {
        //GetComponent<AudioSource>().Play();

        InventoryManager.instance.AddGold(-shopItem.goldPrice);
        Player.actions.PlaySound(GetComponent<AudioSource>().clip);
        InventoryManager.instance.AddItem(shopItem);
        Player.actions.ShowItemText(shopItem.itemName);
        SetTileWalkable();

        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
        Destroy(gameObject);
    }
}
