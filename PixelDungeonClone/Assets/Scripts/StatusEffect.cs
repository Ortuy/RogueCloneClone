using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public int effectID;

    public Sprite icon;

    public float effectValue;

    public int durationLeft;
    //public int iconDisplayID;

    public GameObject iconDisplay;

    public Entity targetEntity;

    public StatusEffect(float multiplier, int duration, Entity target)
    {
        //icon = newIcon;
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
    }
    /**
    public StatusEffect(float multiplier, int duration)
    {
        //icon = newIcon;
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = null;
    }**/

    public virtual void OnEffectApplied()
    {

    }

    public virtual void OnEffectTick()
    {

    }

    public virtual void OnEffectEnd()
    {

    }
}

public class PoisonEffect : StatusEffect
{
    /**
    public PoisonEffect(float multiplier, int duration) : base(multiplier, duration)
    {
        icon = Player.stats.statusIcons[2];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = null;
        effectID = 2;
    }**/

    public PoisonEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[2];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 2;
    }

    public override void OnEffectApplied()
    {
        //base.OnEffectApplied();
    }

    public override void OnEffectEnd()
    {
        //base.OnEffectEnd();
    }

    public override void OnEffectTick()
    {
        targetEntity.TakeTrueDamage(Mathf.RoundToInt(effectValue));
        /**
        if (targetEntity != null)
        {
            targetEntity.TakeTrueDamage(Mathf.RoundToInt(effectValue));
        }
        else
        {
            Player.stats.TakeTrueDamage(Mathf.RoundToInt(effectValue));
        }**/
        //base.OnEffectTick();
    }

}

public class CleansingEffect : StatusEffect
{
    public CleansingEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[0];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 0;
    }

    public override void OnEffectApplied()
    {
        int temp = 0;
        for(int i = 0; i < targetEntity.statusEffects.Count; i++)
        {
            if(targetEntity.statusEffects[temp].effectID > 0 && targetEntity.statusEffects[temp].effectID < 7)
            {
                targetEntity.EndStatusEffect(temp);
            }
            else
            {
                temp++;
            }
        }
        targetEntity.immune = true;
    }

    public override void OnEffectEnd()
    {
        targetEntity.immune = false;
    }

    public override void OnEffectTick()
    {

    }
}

public class InvisibilityEffect : StatusEffect
{
    public InvisibilityEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[1];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 1;
    }

    public override void OnEffectApplied()
    {
        targetEntity.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        targetEntity.invisible = true;
    }

    public override void OnEffectEnd()
    {
        targetEntity.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        targetEntity.invisible = false;
    }

    public override void OnEffectTick()
    {

    }
}

public class FrostEffect : StatusEffect
{
    public FrostEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[3];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 3;
    }

    public override void OnEffectApplied()
    {
        if(targetEntity == Player.stats)
        {
            TurnManager.instance.playerFrozen = true;
        }
        else if(targetEntity.GetComponent<Enemy>() != null)
        {
            targetEntity.GetComponent<Enemy>().turnCost = durationLeft;
        }
    }

    public override void OnEffectEnd()
    {
        if (targetEntity == Player.stats)
        {
            TurnManager.instance.playerFrozen = false;
        }
        else if (targetEntity.GetComponent<Enemy>() != null)
        {
            targetEntity.GetComponent<Enemy>().turnCost = 0;
        }
    }

    public override void OnEffectTick()
    {

    }
}

public class WeaknessEffect : StatusEffect
{
    private int tempModifier;

    public WeaknessEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[4];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 4;
    }

    public override void OnEffectApplied()
    {
        tempModifier = Mathf.RoundToInt(targetEntity.maxBaseDamage * 0.3f);
        targetEntity.dmgModifier -= tempModifier;
    }

    public override void OnEffectEnd()
    {
        targetEntity.dmgModifier += tempModifier;
    }

    public override void OnEffectTick()
    {

    }
}

public class FragilityEffect : StatusEffect
{
    private int tempModifierEva;
    private int tempModifierDef;

    public FragilityEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[5];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 5;
    }

    public override void OnEffectApplied()
    {
        tempModifierEva = Mathf.RoundToInt(targetEntity.GetEvasion() * 0.6f);
        tempModifierDef = Mathf.RoundToInt(targetEntity.maxDefence * 0.3f);
        targetEntity.defModifier -= tempModifierDef;
        targetEntity.evaModifier -= tempModifierEva;
    }

    public override void OnEffectEnd()
    {
        targetEntity.defModifier += tempModifierDef;
        targetEntity.evaModifier += tempModifierEva;
    }

    public override void OnEffectTick()
    {

    }
}

public class BlindnessEffect : StatusEffect
{
    private int tempModifier;

    public BlindnessEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[6];
        effectValue = multiplier;
        durationLeft = duration;
        targetEntity = target;
        effectID = 6;
    }

    public override void OnEffectApplied()
    {
        tempModifier = Mathf.RoundToInt(targetEntity.GetAccuracy() * 0.6f);
        targetEntity.accModifier -= tempModifier;

        if(targetEntity.TryGetComponent(out PlayerStatistics stats))
        {
            stats.blindLight.SetActive(true);
            stats.normalLight.SetActive(false);
        }

    }

    public override void OnEffectEnd()
    {
        targetEntity.accModifier += tempModifier;
        if (targetEntity.TryGetComponent(out PlayerStatistics stats))
        {
            stats.blindLight.SetActive(false);
            stats.normalLight.SetActive(true);
        }
    }

    public override void OnEffectTick()
    {

    }
}

public class HungerEffect : StatusEffect
{
    public int type;

    public HungerEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[duration];
        effectValue = multiplier;
        durationLeft = -1;
        targetEntity = target;
        effectID = 7;
        type = duration;
    }

    public override void OnEffectApplied()
    {

    }

    public override void OnEffectEnd()
    {
        if (Player.stats.foodPoints > 96)
        {
            Player.stats.AddStatusEffect(new HungerEffect(0f, 7, Player.stats));
        }
        else if(Player.stats.foodPoints == 96)
        {
            Player.stats.AddStatusEffect(new HungerEffect(0f, 8, Player.stats));
        }
        else
        {
            Player.stats.AddStatusEffect(new StarvationEffect(0f, 7, Player.stats));
        }
    }

    public override void OnEffectTick()
    {
        Player.stats.foodPoints--;
        if(Player.stats.foodPoints == 96 || Player.stats.foodPoints == 0)
        {
            Player.stats.EndStatusEffectOfType(7);
        }
        if(Player.stats.foodPoints > 96)
        {
            if (Random.Range(0, 5) == 0)
            {
                Player.stats.Heal(1);
            }
            if(type == 8)
            {
                Player.stats.EndStatusEffectOfType(7);
            }          
        }
    }
}

public class StarvationEffect : StatusEffect
{

    public StarvationEffect(float multiplier, int duration, Entity target) : base(multiplier, duration, target)
    {
        icon = Player.stats.statusIcons[9];
        effectValue = multiplier;
        durationLeft = -1;
        targetEntity = target;
        effectID = 8;
    }

    public override void OnEffectApplied()
    {

    }

    public override void OnEffectEnd()
    {
        Player.stats.AddStatusEffect(new HungerEffect(0f, 7, Player.stats));
    }

    public override void OnEffectTick()
    {
        if (Player.stats.foodPoints > 0)
        {
            Player.stats.EndStatusEffectOfType(8);
        }
        else
        {
            if(Random.Range(0, 5) == 0)
            {
                Player.stats.TakeTrueDamage(1);
            }            
        }
    }
}