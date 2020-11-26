using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatistics : MonoBehaviour
{
    //public static PlayerStatistics instance;

    [Header("Stats")]
    [SerializeField]
    private int level;
    [SerializeField]
    private int strength;
    [SerializeField]
    private float health, maxHealth;
    [SerializeField]
    private float evasion;
    [SerializeField]
    private float accuracy;
    [SerializeField]
    private int minDefence, maxDefence;
    //Minimum and maximum damage dealt. INCLUSIVE
    [SerializeField]
    private int minBaseDamage, maxBaseDamage;

    [SerializeField]
    private ParticleSystem hitFX;

    // Start is called before the first frame update
    void Start()
    {
        /**
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }**/
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage, float attackerAccuracy, Vector3 attackerPos)
    {
        //Evasion chance
        int evasionPercent = Mathf.FloorToInt(((evasion - attackerAccuracy) / (evasion + 10)) * 100);
        Debug.Log("Evasion chacne: " + evasionPercent + "%");
        int evasionRoll = Random.Range(1, 101);
        if (evasionRoll <= evasionPercent)
        {
            Debug.Log("Dodge!");
        }
        else
        {
            var hitDirection = (attackerPos - transform.position).normalized;
            Debug.DrawRay(transform.position, hitDirection, Color.yellow, 1000);
            var angle = Mathf.Atan2(hitDirection.y, hitDirection.x);
            //hitFX.transform.rotation = Quaternion.AngleAxis(angle - 120, Vector3.forward);
            //hitFX.transform.Rotate(new Vector3(-90, 0, 0));
            hitFX.Play();

            var defence = Random.Range(minDefence, maxDefence + 1);
            health -= (damage - defence);
            float value = (health / maxHealth);
            UIManager.instance.playerHealthBar.value = value;

            if (health <= 0)
            {
                Debug.LogError("DEATH!");
                Destroy(gameObject);
            }
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

    public float getAccuracy()
    {
        return accuracy;
    }
}
