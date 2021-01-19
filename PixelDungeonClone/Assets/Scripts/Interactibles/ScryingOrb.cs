using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScryingOrb : InteractibleObject
{
    public SpriteRenderer[] secondaryObjects;

    public Animator animator;

    private bool used;
    [SerializeField]
    private string usedDescription;

    private DecorativeObject decorativeObject;

    [SerializeField]
    private List<string> roomChoices;

    [SerializeField]
    private ParticleSystem useFX, normalFX;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip useSound;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        decorativeObject = GetComponent<DecorativeObject>();
        audioSource = GetComponent<AudioSource>();
        SetTileUnwalkable();
        SetItemsAside();
        interactionDescription = interactionDescription.Replace("$n", "\n");

        foreach (SpriteRenderer renderer in secondaryObjects)
        {
            renderer.sortingOrder += (-3 * Mathf.FloorToInt(transform.position.y + 0.5f));
        }

        InitChoiceButtons();
    }

    public override void StartInteraction()
    {
        if (!used)
        {
            base.StartInteraction();
            if (Player.stats.GetHealth() <= (Player.stats.GetMaxHealth() * 0.3f))
            {
                UIManager.instance.interactionButton.interactable = false;
            }
        }
    }

    public override void DoInteraction()
    {
        ShowChoiceMenu(roomChoices);
    }

    public override void DoChoiceInteraction(int choiceID)
    {
        Debug.Log(choiceID);
        GameManager.instance.guaranteedSpecialRoom = choiceID;
        ShowChoiceMenu(roomChoices);
        MouseBlocker.mouseBlocked = false;
        audioSource.loop = false;
        audioSource.clip = useSound;
        audioSource.Play();
        normalFX.Stop();
        useFX.Play();
        animator.SetBool("Used", true);
        used = true;
        decorativeObject.objectDesc = usedDescription;
        Player.stats.TakeTrueDamage(Mathf.FloorToInt(Player.stats.GetMaxHealth() * 0.3f));
        TurnManager.instance.SwitchTurn(TurnState.ENEMY);
    }
}
