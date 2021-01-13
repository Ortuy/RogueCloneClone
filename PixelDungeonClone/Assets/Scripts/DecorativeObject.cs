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
    }
}
