using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedGate : InteractibleObject
{
    public string keyType;
    public Animator animator;
    public ParticleSystem openParticle;

    [SerializeField]
    private SpriteRenderer[] secondaryObjects;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip lockedSound, openSound;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        SetTileUnwalkable();
        SetItemsAside();

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }
        audioSource = GetComponent<AudioSource>();
    }

    public override void StartInteraction()
    {
        DoInteraction();
    }

    public override void DoInteraction()
    {
        int slot = InventoryManager.instance.findItemSlot(keyType);

        if(slot != -1)
        {
            InventoryManager.instance.SubtractItem(slot);
            animator.SetTrigger("Open");
            openParticle.Play();
            PlaySound(openSound);
        }
        else
        {
            animator.SetTrigger("Locked");
            PlaySound(lockedSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void OpenGate()
    {
        SetTileWalkable();
        openParticle.transform.SetParent(null);
        Destroy(openParticle, 3f);
        Destroy(gameObject);
    }
}
