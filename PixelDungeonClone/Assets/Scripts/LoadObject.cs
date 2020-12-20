using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 0)
        {
            Destroy(gameObject);
        }
        else if(scene.buildIndex != 6)
        {
            StartCoroutine(MovePlayerToStartPosition());
        }
    }

    IEnumerator MovePlayerToStartPosition()
    {
        yield return new WaitForSeconds(1);
        Player.instance.transform.position = FindObjectOfType<LevelGenerator>().playerStartPos;
    }
}
