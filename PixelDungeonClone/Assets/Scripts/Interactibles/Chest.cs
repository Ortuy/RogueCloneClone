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
        Debug.Log("Chest");
        open = true;
        animator.SetBool("Open", open);
        decorativeObject.objectDesc = openDescription;

        GetComponent<AudioSource>().Play();

        /**
        var below = Physics2D.OverlapPoint(new Vector2(transform.position.x, transform.position.y - 1f), LayerMask.GetMask("Decor"));
        var left = Physics2D.OverlapPoint(new Vector2(transform.position.x - 1, transform.position.y), LayerMask.GetMask("Decor"));
        var above = Physics2D.OverlapPoint(new Vector2(transform.position.x, transform.position.y + 1f), LayerMask.GetMask("Decor"));

        
        if ((below != null && !below.CompareTag("Interactible")) || below == null)
        {
            var pickup = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x, transform.position.y - 1f, 0), Quaternion.identity);
            pickup.SetItem(chestItem);
            pickup.PlaySoundOnInit(InventoryManager.instance.itemDropSound);
        }
        else if ((left != null && !left.CompareTag("Interactible")) || left == null)
        {
            var pickup = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x - 1f, transform.position.y, 0), Quaternion.identity);
            pickup.SetItem(chestItem);
            pickup.PlaySoundOnInit(InventoryManager.instance.itemDropSound);
        }
        else if ((above != null && !above.CompareTag("Interactible")) || above == null)
        {
            var pickup = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x, transform.position.y + 1f, 0), Quaternion.identity);
            pickup.SetItem(chestItem);
            pickup.PlaySoundOnInit(InventoryManager.instance.itemDropSound);
        }
        else
        {
            var pickup = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x + 1f, transform.position.y, 0), Quaternion.identity);
            pickup.SetItem(chestItem);
            pickup.PlaySoundOnInit(InventoryManager.instance.itemDropSound);
        }
        **/
        InventoryManager.instance.AddItem(chestItem);
        Player.actions.ShowItemText(chestItem.itemName);

        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }
}
