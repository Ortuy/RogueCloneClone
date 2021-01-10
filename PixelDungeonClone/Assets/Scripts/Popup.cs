using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Popup : MonoBehaviour
{
    private EventTrigger eventTrigger;

    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            //Pointer exit
            EventTrigger.Entry exitUIObject = new EventTrigger.Entry();
            exitUIObject.eventID = EventTriggerType.PointerExit;
            exitUIObject.callback.AddListener((eventData) => { ExitUI(); });
            eventTrigger.triggers.Add(exitUIObject);
        }
    }

    public void ExitUI()
    {
        gameObject.SetActive(false);
        MouseBlocker.mouseBlocked = false;
    }
}
