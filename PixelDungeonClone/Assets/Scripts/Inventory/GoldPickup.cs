using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    public int amount;

    private AudioSource audioSource;
    public AudioClip dropSound;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = (-3 * Mathf.FloorToInt(transform.position.y + 0.5f)) + 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            spriteRenderer.sortingOrder--;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.sortingOrder++;
            UIManager.instance.pickUpButton.gameObject.SetActive(false);
        }
        if (collision.CompareTag("Enemy"))
        {
            spriteRenderer.sortingOrder++;
        }
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlaySoundOnInit(AudioClip clip)
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
