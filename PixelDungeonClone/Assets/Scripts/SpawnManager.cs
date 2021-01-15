using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    [SerializeField]
    private ObjectPooler[] enemySpawners;

    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
    }

    public Enemy SpawnEnemy(int enemyTypeID, Vector2 postion)
    {
        Enemy tempEnemy = enemySpawners[enemyTypeID].GetNewPooledObject().GetComponent<Enemy>();
        if(tempEnemy != null)
        {
            tempEnemy.transform.position = postion;
            tempEnemy.gameObject.SetActive(true);
        }
        //TurnManager.instance.enemies.Add(tempEnemy);
        tempEnemy.SetSortingOrder();
        return tempEnemy;
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
}
