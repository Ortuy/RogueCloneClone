using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTester : MonoBehaviour
{
    public ItemPickup item0, item1, item2;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitAndDoStuff());
    }

    IEnumerator WaitAndDoStuff()
    {
        yield return null;
        item0.itemInside.LevelUp(3);
        item1.itemInside.LevelUp(-2);
        item1.itemInside.cursed = true;
        item2.itemInside.identified = false;
    }
}
