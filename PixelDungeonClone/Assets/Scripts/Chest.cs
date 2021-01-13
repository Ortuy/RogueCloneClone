using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    public ItemInstance chestItem;
    public Item chestItemPrefab;

    private bool open;
    [SerializeField]
    private string openDescription;

    private DecorativeObject decorativeObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        SetTileUnwalkable();
        SetItemsAside();

        if(chestItem == null)
        {
            chestItem = new ItemInstance(chestItemPrefab, 1);
            
        }

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }
    }

    public override void StartInteraction()
    {
        if (!open)
        {
            DoInteraction();
        }
    }

    public override void DoInteraction()
    {
        open = true;
        animator.SetBool("Open", open);
        decorativeObject.objectDesc = openDescription;

        var pickup = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x, transform.position.y - 1f, 0), Quaternion.identity);
        pickup.SetItem(chestItem);
        pickup.PlaySoundOnInit(InventoryManager.instance.itemDropSound);

        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }
}
