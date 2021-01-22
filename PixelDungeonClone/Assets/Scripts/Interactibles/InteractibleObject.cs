using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InteractibleObject : MonoBehaviour
{
    public string interactionName, interactionDescription, buttonText;
    public GameObject choiceMenu;
    public List<Text> choiceTexts = new List<Text>();
    public List<Button> choiceButtons;

    protected void SetTileUnwalkable()
    {
        Pathfinding.instance.GetGrid().GetGridObject(transform.position).SetWalkability(false);
    }

    protected void SetTileWalkable()
    {
        Pathfinding.instance.GetGrid().GetGridObject(transform.position).SetWalkability(true);
    }

    protected void InitChoiceButtons()
    {
        choiceButtons.Clear();
        choiceTexts.Clear();
        var temp = choiceMenu.GetComponentsInChildren<Button>();
        for(int i = 0; i < temp.Length; i++)
        {
            choiceButtons.Add(temp[i]);
            choiceTexts.Add(temp[i].GetComponentInChildren<Text>());
        }

        /**
        for(int i = 0; i < choiceButtons.Count; i++)
        {
            Debug.Log(i);
            choiceButtons[i].onClick.AddListener(() => DoChoiceInteraction(i));
        }**/

        choiceButtons[0].onClick.AddListener(() => DoChoiceInteraction(0));
        choiceButtons[1].onClick.AddListener(() => DoChoiceInteraction(1));
        choiceButtons[2].onClick.AddListener(() => DoChoiceInteraction(2));
        choiceButtons[3].onClick.AddListener(() => DoChoiceInteraction(3));
        choiceButtons[4].onClick.AddListener(() => DoChoiceInteraction(4));
        choiceButtons[5].onClick.AddListener(() => DoChoiceInteraction(5));
        choiceButtons[6].onClick.AddListener(() => DoChoiceInteraction(6));
        choiceButtons[7].onClick.AddListener(() => DoChoiceInteraction(7));
        choiceButtons[8].onClick.AddListener(() => DoChoiceInteraction(8));
        choiceButtons[9].onClick.AddListener(() => DoChoiceInteraction(9));
    }

    protected void ShowChoiceMenu(List<string> buttonTexts)
    {
        if(choiceMenu != null)
        {
            if(choiceMenu.activeInHierarchy)
            {
                choiceMenu.SetActive(false);
            }
            else
            {
                choiceMenu.SetActive(true);
                for (int i = 0; i < choiceButtons.Count; i++)
                {
                    if(i < buttonTexts.Count)
                    {
                        choiceButtons[i].gameObject.SetActive(true);
                        choiceTexts[i].text = buttonTexts[i];
                    }
                    else
                    {
                        choiceButtons[i].gameObject.SetActive(false);
                    }
                }
                UIManager.instance.RefreshPopupRect((RectTransform)choiceMenu.transform);
            }
        }        
    }

    public virtual void DoChoiceInteraction(int choiceID)
    {

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
