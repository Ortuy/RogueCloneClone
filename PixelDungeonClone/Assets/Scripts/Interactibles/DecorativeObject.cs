using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativeObject : MonoBehaviour
{
    public string objectName;
    public string objectDesc;

    private void Start()
    {
        objectDesc = objectDesc.Replace("$n", "\n");
        objectDesc = objectDesc.Replace("$p0", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID]);
        objectDesc = objectDesc.Replace("$p1", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 3]);
        objectDesc = objectDesc.Replace("$p2", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 6]);
        objectDesc = objectDesc.Replace("$p3", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 9]);
        objectDesc = objectDesc.Replace("$p4", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 12]);
        objectDesc = objectDesc.Replace("$p5", GameManager.instance.playerPronouns[GameManager.instance.playerPronounID + 15]);
    }
}
