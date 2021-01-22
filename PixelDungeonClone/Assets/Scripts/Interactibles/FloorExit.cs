using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorExit : InteractibleObject
{
    private bool transitionStarted;

    [SerializeField]
    private SpriteRenderer mapIcon;
    private AudioSource audioSource;

    private void Start()
    {
        //StartCoroutine(CheckForPlayer());
        SetTileUnwalkable();
        mapIcon.color = Color.white;
        audioSource = GetComponent<AudioSource>();
        interactionDescription = interactionDescription.Replace("$n", "\n");
        SetItemsAside();
    }

    private void Update()
    {
        if(transitionStarted && UIManager.instance.fadeOutDone)
        {
            GameManager.instance.mapRevealed = false;
            //UIManager.instance.mapButton.gameObject.SetActive(false);
            transitionStarted = false;
            //Player.instance.transform.position = new Vector2(0.5f, 0.5f);
            Player.movement.StopMovement();
            TurnManager.instance.ReturnEnemiesToPools();
            if(GameManager.instance.currentFloor > 10)
            {
                Player.stats.CreateSuccessLog();
            }
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
        Player.instance.transform.position = transform.position;
        transitionStarted = true;
        UIManager.instance.StartFadeOut();
        audioSource.Play();
        GameManager.instance.currentFloor++;
    }

    public override void DoInteraction()
    {
        base.DoInteraction();
        Descend();
    }

    /**
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Descend();   
        }
    }   
    **/
}
