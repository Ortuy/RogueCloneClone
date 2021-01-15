using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemPrefab;
    public ItemInstance itemInside;

    private bool thrown;
    private Vector3 throwDestination;
    public float throwSpeed;

    public int amount;
    private Animator animator;
    private Rigidbody2D rigidBody;

    [SerializeField]
    private Sprite[] mapIcons;
    public SpriteRenderer mapImage;
    private AudioSource audioSource;
    public AudioClip dropSound, breakSound;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        if(itemInside == null)
        {
            itemInside = new ItemInstance(itemPrefab, amount);
        }
        //spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemInside.itemImage;
        spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
        if (itemInside.stackable)
        {
            itemInside.amount = amount;
        }
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetThrown(Vector2 destination)
    {
        thrown = true;
        throwDestination = destination;
        GetComponent<Animator>().Play("ItemThrow");
        GetComponent<Rigidbody2D>().velocity = -(transform.position - throwDestination).normalized * throwSpeed;
    }

    void Update()
    {
        if(thrown)
        {
            if(Vector3.Distance(throwDestination, transform.position) <= 0.1)
            {
                ThrowLanding();
            }
        }
    }

    private void ThrowLanding()
    {
        animator.Play("Item");
        thrown = false;
        rigidBody.velocity = Vector3.zero;
        spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;

        if (itemInside.type == ItemType.POTION)
        {
            IdentifyingMenager.instance.IdentifyItem(itemInside);
            switch(itemInside.effectID)
            {
                case 3:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[0], transform.position, Quaternion.identity));
                    break;
                case 5:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[1], transform.position, Quaternion.identity));
                    break;
                case 6:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[2], transform.position, Quaternion.identity));
                    break;
                case 7:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[3], transform.position, Quaternion.identity));
                    break;
                case 8:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[4], transform.position, Quaternion.identity));
                    break;
                case 9:
                    TurnManager.instance.gases.Add(Instantiate(InventoryManager.instance.potionGases[5], transform.position, Quaternion.identity));
                    break;
            }
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            PlaySound(breakSound);
            SetItem(null);
            Destroy(gameObject, 1f);
        }
        else
        {
            PlaySound(dropSound);
        }
        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }

    public void SetItem(ItemInstance item)
    {
        itemInside = item;
        GetComponent<SpriteRenderer>().sprite = itemInside.itemImage;
        switch(item.type)
        {
            case ItemType.WEAPON:
                mapImage.sprite = mapIcons[0];
                break;
            case ItemType.ARMOR:
                mapImage.sprite = mapIcons[1];
                break;
            case ItemType.RING:
                mapImage.sprite = mapIcons[2];
                break;
            case ItemType.POTION:
                mapImage.sprite = mapIcons[3];
                break;
            case ItemType.SCROLL:
                mapImage.sprite = mapIcons[4];
                break;
            case ItemType.FOOD:
                mapImage.sprite = mapIcons[5];
                break;
            case ItemType.TORCH:
                mapImage.sprite = mapIcons[6];
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            spriteRenderer.sortingOrder--;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager.instance.pickUpButton.gameObject.SetActive(false);
            spriteRenderer.sortingOrder++;
        }
        if (collision.CompareTag("Enemy"))
        {
            spriteRenderer.sortingOrder++;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlaySoundOnInit(AudioClip clip)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
