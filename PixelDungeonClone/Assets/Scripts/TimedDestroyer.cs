using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroyer : MonoBehaviour
{
    public float lifetime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3f);
    }
}
