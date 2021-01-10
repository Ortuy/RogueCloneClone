using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleObject : MonoBehaviour
{
    public string interactionName, interactionDescription, buttonText;

    protected void SetTileUnwalkable()
    {
        Pathfinding.instance.GetGrid().GetGridObject(transform.position).SetWalkability(false);
    }

    public virtual void StartInteraction()
    {
        UIManager.instance.usedInteractible = this;
        UIManager.instance.ToggleInteractionMenu();
        
    }

    protected void SetItemsAside()
    {
        var temp = Physics2D.OverlapCircleAll(transform.position, 0.5f, LayerMask.GetMask("Items"));
        foreach (Collider2D item in temp)
        {
            var randomDir = Random.Range(0, 4);
            Vector3 direction = Vector3.zero;
            switch (randomDir)
            {
                case 0:
                    direction = Vector3.up;
                    break;
                case 1:
                    direction = Vector3.right;
                    break;
                case 2:
                    direction = Vector3.down;
                    break;
                case 3:
                    direction = Vector3.left;
                    break;
            }

            item.transform.position += direction;
        }
    }

    public virtual void DoInteraction()
    {
        
    }
}
