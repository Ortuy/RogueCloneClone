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
    private ParticleSystem burstFX;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        
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
        yield return null;
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
            Debug.Log(collision.name);
            Debug.Log(animator);
            animator.SetTrigger("Rustle");
            burstFX.Play();
            rustled = true;
        }
    }
    /**
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!rustled)
        {
            animator.SetTrigger("Rustle");
            rustled = true;
        }
    }**/
}
