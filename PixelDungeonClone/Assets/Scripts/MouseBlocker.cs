using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class MouseBlocker : MonoBehaviour
{
    public static bool mouseBlocked;

    private EventTrigger eventTrigger;

    // Start is called before the first frame update
    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if(eventTrigger != null)
        {
            //Pointer enter
            EventTrigger.Entry enterBlockingObject = new EventTrigger.Entry();
            enterBlockingObject.eventID = EventTriggerType.PointerEnter;
            enterBlockingObject.callback.AddListener((eventData) => { EnterBlock(); });
            eventTrigger.triggers.Add(enterBlockingObject);

            //Pointer exit
            EventTrigger.Entry exitBlockingObject = new EventTrigger.Entry();
            exitBlockingObject.eventID = EventTriggerType.PointerExit;
            exitBlockingObject.callback.AddListener((eventData) => { ExitBlock(); });
            eventTrigger.triggers.Add(exitBlockingObject);
        }
    }

    public void EnterBlock()
    {
        Debug.Log("ARSE");
        mouseBlocked = true;
    }

    public void ExitBlock()
    {
        mouseBlocked = false;
    }
}
