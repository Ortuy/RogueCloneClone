using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatistics : Entity
{
    //public static PlayerStatistics instance;
    /**
    [Header("Stats")]
    [SerializeField]
    private int level;

    [SerializeField]
    private int xp;
    private int[] xpRequired;

    [SerializeField]
    private int strength;
    [SerializeField]
    private float health, maxHealth;
    [SerializeField]
    private float evasion;
    [SerializeField]
    private float accuracy;

    [SerializeField]
    private int minDefBase, maxDefBase, minDmgBase, maxDmgBase;

    //Minimum and maximum defence. INCLUSIVE
    public int minDefence, maxDefence;
    //Minimum and maximum damage dealt. INCLUSIVE
    public int minBaseDamage, maxBaseDamage;
    **/
    [SerializeField]
    private ParticleSystem hitFX;

    //public List<StatusEffect> statusEffects;

    public Sprite[] statusIcons;

    public int foodPoints = 192;

    public GameObject normalLight, blindLight;
    /**
    public bool immune;
    public bool invisible;

    public int dmgModifier, evaModifier, defModifier, accModifier;
    **/

    // Start is called before the first frame update
    void Start()
    {
        xpRequired = new int[50];
        for(int i = 0; i < xpRequired.Length; i++)
        {
            xpRequired[i] = 10 + 5 * i;
        }

        health = maxHealth;
        statusEffects = new List<StatusEffect>();
        StartCoroutine(WaitAndAddHungerStatus());
    }

    IEnumerator WaitAndAddHungerStatus()
    {
        yield return null;
        AddStatusEffect(new HungerEffect(0f, 7, this));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AddStatusEffect(StatusEffect status)
    {
        if(!immune || status.effectID == 7 || status.effectID == 8)
        {
            statusEffects.Add(status);
            status.OnEffectApplied();
            UIManager.instance.AddStatusToDisplay(status.icon);
            statusEffects[statusEffects.Count - 1].iconDisplay = UIManager.instance.statusDisplay[UIManager.instance.statusDisplay.Count - 1].gameObject;
        }       
    }

    public override void TickStatusEffects()
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            statusEffects[i].OnEffectTick();
            statusEffects[i].durationLeft--;
            if(statusEffects[i].durationLeft == 0)
            {
                EndStatusEffect(i);
            }
        }
    }

    public override void EndStatusEffect(int effectID)
    {
        statusEffects[effectID].OnEffectEnd();
        var temp = statusEffects[effectID].iconDisplay;
        UIManager.instance.statusDisplay.Remove(temp.GetComponent<Image>());
        Destroy(temp);
        statusEffects.RemoveAt(effectID);
    }

    public void EndStatusEffectOfType(int effectType)
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            if(statusEffects[i].effectID == effectType)
            {
                statusEffects[i].OnEffectEnd();
                var temp = statusEffects[i].iconDisplay;
                UIManager.instance.statusDisplay.Remove(temp.GetComponent<Image>());
                Destroy(temp);
                statusEffects.RemoveAt(i);
                break;
            }
        }
        
    }

    public int GetStrength()
    {
        return strength;
    }

    public void IncreaseStrength()
    {
        strength++;
    }

    public void Heal(int healedHP)
    {
        health += healedHP;
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;
    }

    public override void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos)
    {
        
        //Evasion chance
        int evasionPercent = Mathf.FloorToInt(((evasion + evaModifier - attackerAccuracy) / (evasion + evaModifier + 10)) * 100);
        Debug.Log("Evasion chacne: " + evasionPercent + "%");
        int evasionRoll = Random.Range(1, 101);
        if (evasionRoll <= evasionPercent)
        {
            Debug.Log("Dodge!");
            ShowDamageText("Miss!");
        }
        else
        {
            var hitDirection = (attackerPos - transform.position).normalized;
            Debug.DrawRay(transform.position, hitDirection, Color.yellow, 1000);
            var angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
            hitFX.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
            //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
            hitFX.Play();

            var defence = Random.Range(minDefence, maxDefence + 1);

            var totaldmg = damage - (defence + defModifier);
            if(totaldmg < 1)
            {
                totaldmg = 1;
            }

            ShowDamageText(totaldmg.ToString());

            health -= totaldmg;
            float value = (health / maxHealth);
            UIManager.instance.playerHealthBar.value = value;

            if (health <= 0)
            {
                UIManager.instance.ToggleDeathScreen();
                Debug.LogError("DEATH!");
                gameObject.SetActive(false);
            }
        }
    }

    public override void TakeTrueDamage(int damage)
    {        
        //hitFX.transform.rotation = Quaternion.AngleAxis(angle - 120, Vector3.forward);
        //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
        //hitFX.Play();

        health -= damage;
        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;

        ShowDamageText(damage.ToString());

        if (health <= 0)
        {
            UIManager.instance.ToggleDeathScreen();
            Debug.LogError("DEATH!");
            gameObject.SetActive(false);
        }
    }

    public int GetRandomDamageValue()
    {
        return Random.Range(Player.stats.minBaseDamage, Player.stats.maxBaseDamage + 1);
    }

    public int GetRandomDefenceValue()
    {
        return Random.Range(Player.stats.minDefence, Player.stats.maxDefence + 1);
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetCurrentXP()
    {
        return xp;
    }

    public int GetRequiredXP()
    {
        return xpRequired[level - 1];
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void AddXP(int xpToAdd)
    {
        xp += xpToAdd;
        if(xp >= xpRequired[level - 1])
        {
            xp -= xpRequired[level - 1];
            LevelUp();            
        }
    }

    public void LevelUp()
    {
        Debug.Log("Level up!!");
        maxHealth += 5;
        health = maxHealth;
        evasion++;
        accuracy++;
        level++;

        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;
        InventoryManager.instance.potionUseFX[2].Play();
    }

    public void ResetDefence()
    {
        minDefence = minDefBase;
        maxDefence = maxDefBase;
    }

    public void ResetDamage()
    {
        minBaseDamage = minDmgBase;
        maxBaseDamage = maxDmgBase;
    }
}
