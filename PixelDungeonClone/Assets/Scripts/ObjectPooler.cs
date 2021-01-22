using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    private List<GameObject> pooledObjects;
    [SerializeField]
    private GameObject objectToPool;
    [SerializeField]
    private int amountToPool;

    // Start is called before the first frame update
    void Awake()
    {
        pooledObjects = new List<GameObject>();
        for(int i = 0; i < amountToPool; i++)
        {
            GameObject temp = Instantiate(objectToPool, transform);
            temp.SetActive(false);
            pooledObjects.Add(temp);
        }
    }

    public GameObject GetNewPooledObject()
    {
        for(int i = 0; i < pooledObjects.Count; i++)
        {
            if(!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
