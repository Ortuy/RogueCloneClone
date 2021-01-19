using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombiningTable : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    private bool used;
    [SerializeField]
    private string usedDescription;

    private DecorativeObject decorativeObject;

    public ItemType requiredType;

    private List<string> itemResults;

    [SerializeField]
    private ParticleSystem useFX, normalFX;

    private bool interactionStarted;

    [SerializeField]
    private AudioClip useSound;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        audioSource = GetComponent<AudioSource>();
        SetTileUnwalkable();
        SetItemsAside();
        interactionDescription = interactionDescription.Replace("$n", "\n");

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }

        if (requiredType == ItemType.POTION)
        {
            itemResults = new List<string>(IdentifyingMenager.instance.potionEffectNames);
        }
        else if (requiredType == ItemType.SCROLL)
        {
            itemResults = new List<string>(IdentifyingMenager.instance.scrollEffectNames);
        }

        InitChoiceButtons();
    }

    private void Update()
    {
        if(!used && interactionStarted && InventoryManager.instance.discardComplete)
        {
            interactionStarted = false;
            ShowChoiceMenu(itemResults);
        }
    }

    public override void StartInteraction()
    {
        if (!used)
        {
            var amount = 0;

            for(int i = 4; i < InventoryManager.instance.inventoryItems.Length; i++)
            {
                if(InventoryManager.instance.inventoryItems[i] != null && InventoryManager.instance.inventoryItems[i].type == requiredType)
                {
                    amount += InventoryManager.instance.inventoryItems[i].amount;
                }
            }

            base.StartInteraction();
            if (amount < 3)
            {
                UIManager.instance.interactionButton.interactable = false;
            }
        }
    }

    public override void DoInteraction()
    {
        UIManager.instance.ToggleInventory();
        interactionStarted = true;
        InventoryManager.instance.DiscardItems(requiredType);
    }

    public override void DoChoiceInteraction(int choiceID)
    {
        Debug.Log(choiceID);

        audioSource.loop = false;
        audioSource.clip = useSound;
        audioSource.Play();

        Item drop = null;

        if (requiredType == ItemType.POTION)
        {
            drop = IdentifyingMenager.instance.potions[choiceID];
        }
        else if (requiredType == ItemType.SCROLL)
        {
            drop = IdentifyingMenager.instance.scrolls[choiceID];
        }
        /**
        var item = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x, transform.position.y - 1f, 0), Quaternion.identity);
        item.SetItem(new ItemInstance(drop, 1));
        **/
        StartCoroutine(DropItem(drop));

        ShowChoiceMenu(itemResults);
        MouseBlocker.mouseBlocked = false;
        //normalFX.Stop();
        //useFX.Play();
        animator.SetBool("Used", true);
        used = true;
        decorativeObject.objectDesc = usedDescription;
        
        InventoryManager.instance.discardComplete = false;

        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }

    IEnumerator DropItem(Item drop)
    {
        yield return new WaitForSeconds(0.8f);
        var item = Instantiate(InventoryManager.instance.itemTemplate, new Vector3(transform.position.x, transform.position.y - 1f, 0), Quaternion.identity);
        item.SetItem(new ItemInstance(drop, 1));
        item.PlaySoundOnInit(item.dropSound);
    }
}
