using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField]
    protected int level;

    [SerializeField]
    protected int xp;
    protected int[] xpRequired;

    [SerializeField]
    protected int strength;
    [SerializeField]
    protected float health, maxHealth;
    [SerializeField]
    protected float evasion;
    [SerializeField]
    protected float accuracy;

    [SerializeField]
    protected int minDefBase, maxDefBase, minDmgBase, maxDmgBase;

    //Minimum and maximum defence. INCLUSIVE
    public int minDefence, maxDefence;
    //Minimum and maximum damage dealt. INCLUSIVE
    public int minBaseDamage, maxBaseDamage;

    public List<StatusEffect> statusEffects;

    public bool immune;
    public bool invisible;

    public int dmgModifier, evaModifier, defModifier, accModifier;

    public GameObject damagePopupText;

    public bool HasStatus(int effectID)
    {
        var temp = false;
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if(statusEffects[i].effectID == effectID)
            {
                temp = true;
                break;
            }
        }
        return temp;
    }

    public virtual void AddStatusEffect(StatusEffect status)
    {
        if (!immune)
        {
            var temp = false;

            for(int i = 0; i < statusEffects.Count; i++)
            {
                if(statusEffects[i].effectID == status.effectID && statusEffects[i].effectValue == status.effectValue)
                {
                    temp = true;
                    statusEffects[i].durationLeft += status.durationLeft;
                    break;
                }
            }
            if(!temp)
            {
                statusEffects.Add(status);
                status.OnEffectApplied();
            }            
            //UIManager.instance.AddStatusToDisplay(status.icon);
            //statusEffects[statusEffects.Count - 1].iconDisplay = UIManager.instance.statusDisplay[UIManager.instance.statusDisplay.Count - 1].gameObject;
        }
    }

    public virtual void TickStatusEffects()
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            statusEffects[i].OnEffectTick();
            statusEffects[i].durationLeft--;
            if (statusEffects[i].durationLeft == 0)
            {
                EndStatusEffect(i);
            }
        }
    }

    public virtual void EndStatusEffect(int effectID)
    {
        statusEffects[effectID].OnEffectEnd();
        //var temp = statusEffects[effectID].iconDisplay;
        //UIManager.instance.statusDisplay.Remove(temp.GetComponent<Image>());
        //Destroy(temp);
        statusEffects.RemoveAt(effectID);
    }

    public virtual void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos, out bool dodged)
    {
        dodged = false;
    }

    public virtual void TakeTrueDamage(int damage)
    {

    }

    public float GetAccuracy()
    {
        return accuracy;
    }

    public float GetEvasion()
    {
        return evasion;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    protected void ShowDamageText(string text)
    {
        var popup = Instantiate(damagePopupText, transform.position, Quaternion.identity).GetComponent<TextMesh>();
        popup.text = text;
    }
}
