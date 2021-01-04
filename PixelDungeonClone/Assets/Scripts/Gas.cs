using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : MonoBehaviour
{
    public int turnsLeft, maxSpread;
    public GasTile gasTileBase;

    public List<GasTile> gasTiles;

    public bool active = true;

    public int effectID;

    private void Start()
    {
        gasTiles = new List<GasTile>
        {
            Instantiate(gasTileBase, transform.position, Quaternion.identity, transform)
        };
        TurnManager.instance.gases.Add(this);
    }

    public void TickGas()
    {
        if(turnsLeft == 0)
        {
            for (int i = gasTiles.Count - 1; i >= 0; i--)
            {
                gasTiles[i].gasFX.Stop();
            }
            TurnManager.instance.gases.Remove(this);
            active = false;
            Destroy(gameObject, 2f);
        }
        if(maxSpread > 0)
        {
            for (int i = gasTiles.Count - 1; i >= 0; i--)
            {
                gasTiles[i].Spread();
            }
        }
        maxSpread--;
        turnsLeft--;
    }

    public void DoGasEffect(Entity target)
    {
        if(active)
        {
            switch(effectID)
            {
                case 1:
                    target.TakeTrueDamage(Mathf.CeilToInt(target.GetMaxHealth() * 0.05f));
                    break;
                case 2:
                    if(target == Player.stats)
                    {
                        Player.actions.turnCost = 2;
                    }
                    else
                    {
                        target.GetComponent<Enemy>().turnCost = 2;
                    }
                    break;
                case 3:
                    target.AddStatusEffect(new WeaknessEffect(0, 4, target));
                    break;
                case 4:
                    target.AddStatusEffect(new FragilityEffect(0, 4, target));
                    break;
                case 5:
                    target.AddStatusEffect(new BlindnessEffect(1.1f, 4, target));
                    break;
            }
        }
    }
}
