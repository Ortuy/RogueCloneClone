using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarArea : InteractibleObject
{
    private List<Enemy> nearbyEnemies = new List<Enemy>();
    [SerializeField]
    private ParticleSystem[] areaFX;
    [SerializeField]
    private ParticleSystem burstFX;

    public SpriteRenderer[] secondaryObjects;

    [SerializeField]
    private string usedDescription;
    [SerializeField]
    private DecorativeObject decorativeObject;

    private void Start()
    {
        SetTileUnwalkable();
        SetItemsAside();

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<Enemy>();
            enemy.isByAltar = true;
            enemy.nearbyAltarArea = this;
            nearbyEnemies.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var enemy = collision.GetComponent<Enemy>();
            enemy.isByAltar = false;
            nearbyEnemies.Remove(enemy);
        }
    }

    public void EndAltarEffect()
    {
        foreach (Enemy enemy in nearbyEnemies)
        {
            if(enemy != null)
            {
                enemy.isByAltar = false;
            }
        }

        burstFX.Play();
        foreach (ParticleSystem fx in areaFX)
        {
           fx.Stop();
        }
        decorativeObject.objectDesc = usedDescription;

        gameObject.SetActive(false);
    }
}
