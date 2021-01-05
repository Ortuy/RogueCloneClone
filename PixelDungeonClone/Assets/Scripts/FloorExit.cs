using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorExit : MonoBehaviour
{
    private bool transitionStarted;

    [SerializeField]
    private SpriteRenderer mapIcon;
    private AudioSource audioSource;

    private void Start()
    {
        //StartCoroutine(CheckForPlayer());
        mapIcon.color = Color.white;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(transitionStarted && UIManager.instance.fadeOutDone)
        {
            UIManager.instance.mapButton.gameObject.SetActive(false);
            transitionStarted = false;
            //Player.instance.transform.position = new Vector2(0.5f, 0.5f);
            Player.movement.StopMovement();
            SceneManager.LoadScene(FindObjectOfType<LevelGenerator>().floorID + 1);
        }
    }

    IEnumerator CheckForPlayer()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            if(Vector3.Distance(transform.position, Player.instance.transform.position) <= 7f)
            {
                mapIcon.color = Color.white;
                break;
            }
        }
    }

    public void Descend()
    {
        transitionStarted = true;
        UIManager.instance.StartFadeOut();
        audioSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Descend();   
        }
    }    
}
