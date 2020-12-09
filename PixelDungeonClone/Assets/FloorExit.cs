using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Player.instance.transform.position = new Vector2(0.5f, 0.5f);
            Player.movement.StopMovement();
            SceneManager.LoadScene(FindObjectOfType<LevelGenerator>().floorID + 1);
        }
    }
}
