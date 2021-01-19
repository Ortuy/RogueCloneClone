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
    private ParticleSystem hitFX, spiritStoneFX;

    [SerializeField]
    private AudioSource hitPlayer;

    [SerializeField]
    private AudioClip hitSound, trueDamageSound, levelUpSound, deathSound, spiritStoneSound;

    //public List<StatusEffect> statusEffects;

    public Sprite[] statusIcons;

    public int foodPoints = 192;

    public GameObject normalLight, blindLight, torchLight;
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
        if(!immune || status.effectID >= 7)
        {
            var temp = false;

            for (int i = 0; i < statusEffects.Count; i++)
            {
                if (statusEffects[i].effectID == status.effectID && statusEffects[i].effectValue == status.effectValue)
                {
                    temp = true;
                    statusEffects[i].durationLeft += status.durationLeft;
                    break;
                }
            }
            if (!temp)
            {
                statusEffects.Add(status);
                status.OnEffectApplied();
                UIManager.instance.AddStatusToDisplay(status.icon);
                statusEffects[statusEffects.Count - 1].iconDisplay = UIManager.instance.statusDisplay[UIManager.instance.statusDisplay.Count - 1].gameObject;
            }            
        }       
    }

    private void PlayHitSound(AudioClip clip)
    {
        hitPlayer.clip = clip;
        hitPlayer.Play();
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

    public void IncreaseMaxHP(int amount)
    {
        maxHealth += amount;
        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;
    }

    public void IncreaseStrength(int amount)
    {
        strength += amount;
    }

    public void Heal(int healedHP)
    {
        if(healedHP + health > maxHealth)
        {
            healedHP += Mathf.RoundToInt(maxHealth) - (Mathf.RoundToInt(health) + healedHP);
        }
        
        if(health < maxHealth)
        {
            CheckForWrathRingModifiers(health + healedHP, health);
            ShowHealingText(healedHP.ToString());
            health += healedHP;
        }
        //health += healedHP;        
        
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        
        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;
    }

    public override void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos, out bool dodged)
    {
        
        //Evasion chance
        int evasionPercent = Mathf.FloorToInt(((evasion + evaModifier - attackerAccuracy) / (evasion + evaModifier + 10)) * 100);
        int evasionRoll = Random.Range(1, 101);
        if (evasionRoll <= evasionPercent)
        {
            ShowDamageText("Miss!");
            dodged = true;
        }
        else
        {
            dodged = false;
            var hitDirection = (attackerPos - transform.position).normalized;
            Debug.DrawRay(transform.position, hitDirection, Color.yellow, 1000);
            var angle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
            hitFX.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
            //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
            hitFX.Play();
            PlayHitSound(hitSound);

            var defence = Random.Range(minDefence, maxDefence + 1);

            var totaldmg = damage - (defence + defModifier);
            if(totaldmg < 1)
            {
                totaldmg = 1;
            }

            ShowDamageText(totaldmg.ToString());
            CheckForWrathRingModifiers(health - totaldmg, health);

            health -= totaldmg;
            float value = (health / maxHealth);
            UIManager.instance.playerHealthBar.value = value;

            if (health <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        bool deathPrevented = false;
        if (InventoryManager.instance.ringEquipped[0] == 10)
        {
            deathPrevented = true;
            InventoryManager.instance.ringEquipped[0] = -1;
        }
        if (InventoryManager.instance.ringEquipped[1] == 10 && !deathPrevented)
        {
            deathPrevented = true;
            InventoryManager.instance.ringEquipped[0] = -1;
        }

        if (!deathPrevented)
        {
            UIManager.instance.ToggleDeathScreen();
            Debug.LogError("DEATH!");
            PlayHitSound(deathSound);
            Camera.main.transform.SetParent(null);
            hitPlayer.transform.SetParent(null);
            gameObject.SetActive(false);
        }
        else
        {
            Heal(Mathf.RoundToInt(maxHealth / 2));
            AddStatusEffect(new StatusEffect(0, -1, this));
            PlayHitSound(spiritStoneSound);
            spiritStoneFX.Play();
        }
    }

    public void CheckForWrathRingModifiers(float currentHP, float oldHP)
    {
        if(currentHP <= maxHealth/2 && oldHP > maxHealth / 2)
        {
            if(InventoryManager.instance.ringEquipped[0] == 3)
            {
                dmgModifier += InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
            }
            if (InventoryManager.instance.ringEquipped[1] == 3)
            {
                dmgModifier += InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
            }
        }
        else if(currentHP > maxHealth / 2 && oldHP <= maxHealth / 2)
        {
            if (InventoryManager.instance.ringEquipped[0] == 3)
            {
                dmgModifier -= InventoryManager.instance.inventoryItems[2].baseStatChangeMax;
            }
            if (InventoryManager.instance.ringEquipped[1] == 3)
            {
                dmgModifier -= InventoryManager.instance.inventoryItems[3].baseStatChangeMax;
            }
        }
    }

    public override void TakeTrueDamage(int damage)
    {
        //hitFX.transform.rotation = Quaternion.AngleAxis(angle - 120, Vector3.forward);
        //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
        //hitFX.Play();

        CheckForWrathRingModifiers(health - damage, health);
        PlayHitSound(trueDamageSound);

        health -= damage;
        float value = (health / maxHealth);
        UIManager.instance.playerHealthBar.value = value;

        ShowDamageText(damage.ToString());

        if (health <= 0)
        {
            Die();
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
        CheckForWrathRingModifiers(maxHealth, health);
        Debug.Log("Level up!!");
        maxHealth += 5;
        ShowHealingText((maxHealth - health).ToString());
        health = maxHealth;
        evasion++;
        accuracy++;
        level++;

        float value = (health / maxHealth);
        
        UIManager.instance.playerHealthBar.value = value;
        PlayHitSound(levelUpSound);
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
