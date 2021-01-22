using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WishingWell : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    private int effectID;
    private int cost;

    private bool spent;
    [SerializeField]
    private string spentDescription;
    [SerializeField]
    private Sprite spentSprite;

    [SerializeField]
    private ParticleSystem idleFX;
    [SerializeField]
    private ParticleSystem[] useFX;

    [SerializeField]
    private int floorModifier;

    private DecorativeObject decorativeObject; 

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        SetTileUnwalkable();
        SetItemsAside();

        StartCoroutine(SetCost());
        effectID = Random.Range(0, 7);
        
        interactionDescription = interactionDescription.Replace("$n", "\n");

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }
    }

    IEnumerator SetCost()
    {
        yield return null;
        cost = 30 + (20 * GameManager.instance.currentFloor);
        interactionDescription = interactionDescription.Replace("$cost", cost.ToString());
    }

    public override void StartInteraction()
    {
        if(!spent)
        {
            base.StartInteraction();
            if (InventoryManager.instance.goldAmount < cost)
            {
                UIManager.instance.interactionButton.interactable = false;
            }
        }        
    }

    public override void DoInteraction()
    {
        Debug.Log("WishingWell: " + effectID);
        InventoryManager.instance.AddGold(-cost);
        useFX[effectID].Play();
        idleFX.Stop();

        GetComponent<AudioSource>().Play();

        spent = true;
        decorativeObject.objectDesc = spentDescription;
        secondaryObjects[0].sprite = spentSprite;
        switch(effectID)
        {
            case 0:
                List<ItemInstance> unidentifiedItems = new List<ItemInstance>();
                for(int i = 4; i < InventoryManager.instance.inventoryItems.Length; i++)
                {
                    if(InventoryManager.instance.inventoryItems[i] != null && !InventoryManager.instance.inventoryItems[i].identified)
                    {
                        unidentifiedItems.Add(InventoryManager.instance.inventoryItems[i]);
                    }
                }

                for(int i = 0; i < 4; i++)
                {
                    if (unidentifiedItems.Count > 0)
                    {
                        var rand = Random.Range(0, unidentifiedItems.Count);
                        IdentifyingMenager.instance.IdentifyItem(unidentifiedItems[rand]);
                        unidentifiedItems.RemoveAt(rand);
                    }
                }                             
                break;
            case 1:
                Player.stats.LevelUp();
                break;
            case 2:
                for (int i = 0; i < InventoryManager.instance.inventoryItems.Length; i++)
                {
                    if (InventoryManager.instance.inventoryItems[i] != null && InventoryManager.instance.inventoryItems[i].cursed)
                    {
                        InventoryManager.instance.CleanseItem(i);
                    }
                }
                break;
            case 3:
                Player.stats.IncreaseMaxHP(-4);
                Player.stats.TakeTrueDamage(4, 3);
                Player.stats.IncreaseStrength(2);
                break;
            case 4:
                InventoryManager.instance.AddGold(cost * 4);
                break;
            case 5:
                for(int i = 0; i < 4; i++)
                {
                    if(InventoryManager.instance.inventoryItems[i] != null)
                    {
                        InventoryManager.instance.CurseItem(InventoryManager.instance.inventoryItems[i]);                        
                    }
                    InventoryManager.instance.UpdateEquipmentSlots();
                }
                break;
            case 6:
                Player.stats.AddStatusEffect(new BlindnessEffect(1.1f, 16, Player.stats));
                SpawnManager.instance.SpawnEnemy(Random.Range(0 + floorModifier, 5 + floorModifier), new Vector2(transform.position.x + 2, transform.position.y + 2));
                SpawnManager.instance.SpawnEnemy(Random.Range(0 + floorModifier, 5 + floorModifier), new Vector2(transform.position.x - 2, transform.position.y + 2));
                SpawnManager.instance.SpawnEnemy(Random.Range(0 + floorModifier, 5 + floorModifier), new Vector2(transform.position.x + 2, transform.position.y - 2));
                SpawnManager.instance.SpawnEnemy(Random.Range(0 + floorModifier, 5 + floorModifier), new Vector2(transform.position.x - 2, transform.position.y - 2));
                break;
        }
        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }
}
