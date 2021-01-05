using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioSource : MonoBehaviour
{
    private AudioSource audioSource;
    public float minDelay, maxDelay;
    private bool ready = true;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying && ready)
        {
            StartCoroutine(WaitRandom(minDelay, maxDelay));
        }
    }

    IEnumerator WaitRandom(float min, float max)
    {
        float timeToWait = Random.Range(min, max);
        ready = false;
        yield return new WaitForSecondsRealtime(timeToWait);
        audioSource.Play();
        ready = true;
    }
}
