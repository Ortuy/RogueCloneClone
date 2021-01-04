using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapObject : MonoBehaviour
{
    public Vector2 minCorner, maxCorner;

    private List<SpriteRenderer> itemImages;
    private List<SpriteRenderer> enemyImages;
    private Tilemap tilemap;
    private BoxCollider2D boxCollider;

    public bool hideObjects;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        itemImages = new List<SpriteRenderer>();
        enemyImages = new List<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        StartCoroutine(FindItemsInside());
        if(hideObjects)
        {
            tilemap.color = new Color(1, 1, 1, 0);
        }
        else
        {
            tilemap.color = Color.white;
        }
    }

    IEnumerator FindItemsInside()
    {
        yield return null;
        boxCollider.size = new Vector2(maxCorner.x - minCorner.x + 1, maxCorner.y - minCorner.y + 1);
        boxCollider.offset = new Vector2(minCorner.x + (maxCorner.x - minCorner.x) / 2 + 0.5f, minCorner.y + (maxCorner.y - minCorner.y) / 2 + 0.5f);
        yield return null;
        var temp = Physics2D.OverlapAreaAll(minCorner, maxCorner, LayerMask.GetMask("Items"));
        for(int i = 0; i < temp.Length; i++)
        {
            if(!temp[i].CompareTag("Map"))
            {
                var item = temp[i].GetComponent<ItemPickup>().mapImage;
                if (hideObjects)
                {
                    item.color = new Color(1, 1, 1, 0);
                }
                itemImages.Add(item);
            }
           
        }
        temp = Physics2D.OverlapAreaAll(new Vector2(minCorner.x - 1, minCorner.y -1), new Vector2(maxCorner.x + 1, maxCorner.y + 1), LayerMask.GetMask("Enemies"));
        for (int i = 0; i < temp.Length; i++)
        {
            var enemy = temp[i].GetComponent<Enemy>().mapImage;
            if (hideObjects)
            {
                enemy.color = new Color(1, 1, 1, 0);
            }
            enemyImages.Add(enemy);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            tilemap.color = Color.white;
            for(int i = 0; i < itemImages.Count; i++)
            {
                if(itemImages[i] != null)
                {
                    itemImages[i].color = Color.white;
                }               
            }
            for (int i = 0; i < enemyImages.Count; i++)
            {
                if(enemyImages[i] != null)
                {
                    enemyImages[i].color = Color.white;
                }
                
            }
        }
    }
}
