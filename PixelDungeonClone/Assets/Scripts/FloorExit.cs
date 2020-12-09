using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorExit : MonoBehaviour
{
    private bool transitionStarted;

    private void Update()
    {
        if(transitionStarted && UIManager.instance.fadeOutDone)
        {
            transitionStarted = false;
            Player.instance.transform.position = new Vector2(0.5f, 0.5f);
            Player.movement.StopMovement();
            SceneManager.LoadScene(FindObjectOfType<LevelGenerator>().floorID + 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            transitionStarted = true;
            UIManager.instance.StartFadeOut();         
        }
    }
}
