using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    private DecorativeObject decorativeObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        SetTileUnwalkable();
        SetItemsAside();

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }
    }

    public override void StartInteraction()
    {
        
    }   
}