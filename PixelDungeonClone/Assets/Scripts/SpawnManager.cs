using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField]
    private ObjectPooler[] enemySpawners;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public Enemy SpawnEnemy(int enemyTypeID, Vector2 postion)
    {
        Enemy tempEnemy = enemySpawners[enemyTypeID].GetNewPooledObject().GetComponent<Enemy>();
        if(tempEnemy != null)
        {
            tempEnemy.transform.position = postion;
            tempEnemy.gameObject.SetActive(true);
        }
        TurnManager.instance.enemies.Add(tempEnemy);
        return tempEnemy;
    }
}
