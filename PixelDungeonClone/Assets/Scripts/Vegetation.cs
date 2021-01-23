using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetation : MonoBehaviour
{
    public SpriteRenderer[] spritesFront;
    public SpriteRenderer[] spritesBack;
    private Animator animator;
    private bool rustled = true;

    [SerializeField]
    private bool hidingSpot;

    [SerializeField]
    private ParticleSystem burstFX;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rustled = true;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        var modifier = -3 * Mathf.FloorToInt(transform.position.y + 0.5f);

        //if(Physics2D.OverlapCircle(new Vector2()))

        foreach (SpriteRenderer sRenderer in spritesFront)
        {
            sRenderer.sortingOrder += modifier + 2;
        }
        foreach (SpriteRenderer sRenderer in spritesBack)
        {
            sRenderer.sortingOrder += modifier;
        }
        StartCoroutine(WaitAndUnrustle());
    }

    IEnumerator WaitAndUnrustle()
    {
        yield return new WaitForSeconds(1f);
        Unrustle();
    }

    public void Unrustle()
    {
        rustled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if(!rustled)
        {            
            animator.SetTrigger("Rustle");
            audioSource.Play();
            burstFX.Play();
            rustled = true;

            if(collision.CompareTag("Player") && hidingSpot)
            {
                if(!Player.stats.invisible && !TurnManager.instance.EnemiesAlerted())
                {
                    Player.stats.invisible = true;
                }
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && hidingSpot)
        {
            if (!Player.stats.HasStatus(1))
            {
                Player.stats.invisible = false;
            }
        }
    }
}
