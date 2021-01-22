using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunLogDisplay : MonoBehaviour
{
    private RunLog log;

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text descText;

    public void SetLog(RunLog newLog)
    {
        log = newLog;
        nameText.text = log.characterName;
        descText.text = log.description;
    }
}
